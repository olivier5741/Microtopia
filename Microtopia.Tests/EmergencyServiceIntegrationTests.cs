using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using MediatR;
using Microtopia.Api;
using Microtopia.Dto;
using NetCoreUtopia;
using ServiceStack;
using StructureMap;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Microtopia.Tests
{
    public class EmergencyServiceIntegrationTests
    {
        public EmergencyServiceIntegrationTests()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType<DummyDb>();
                    scanner.AssemblyContainingType<EmergencyService>();
                    scanner.ConnectImplementationsToTypesClosing(
                        typeof(IRequestHandler<>)); // Handlers with no response
                    scanner.ConnectImplementationsToTypesClosing(
                        typeof(IRequestHandler<,>)); // Handlers with a response
                    scanner.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
                });
                cfg.For<IDb>().Use<DummyDb>().Singleton();
                cfg.For<IBus>().Use(RabbitHutch.CreateBus("host=localhost;username=guest;password=guest")).Singleton();
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => ctx.GetInstance);
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => ctx.GetAllInstances);
                cfg.For<IMediator>().Use<Mediator>();
            });

            var bus = container.GetInstance<IBus>();
            bus.Subscribe<AlarmElapsed>("Emergency", @event => { container.GetInstance<IMediator>().Publish(@event); });
            bus.Subscribe<ChannelMessageReceived>("Emergency",
                @event => { container.GetInstance<IMediator>().Publish(@event); });

            // mock outside service
            bus.SubscribeAsync<Alarm>("Alarm", async cmd =>
            {
                var timeSpan = cmd.Time - DateTime.Now;

                if (timeSpan < new TimeSpan(0))
                    timeSpan = new TimeSpan(0);

                await Task.Delay(timeSpan).ContinueWith(t => bus.Publish(new AlarmElapsed {Flow = cmd.Flow}));
            });

            bus.Subscribe<AlarmCancel>("Alarm", Console.WriteLine);

            bus.SubscribeAsync<ChannelMessage>("ChannelMessage",
                async cmd =>
                {
                    await Task.Delay(100).ContinueWith(t => bus.Publish(new ChannelMessageReceived {Flow = cmd.Flow}));
                });

            _service = container.GetInstance<EmergencyService>();
        }

        // message queue name
        private string ExchangeName(Type type)
        {
            var name = type.Namespace.ToLower() + "." + type.Name;

            if (type.HasInterface(typeof(INotification)))
                name += ".tx";
            else if (type.HasInterface(typeof(INotification)))

                name += ".dx";

            return name;
        }

        private readonly EmergencyService _service;

        [Fact]
        public async void FirstMediumReplies()
        {
            var emergency = new Emergency
            {
                Message = "Help",
                Mediums = new List<EmergencyMedium>
                {
                    new EmergencyMedium
                    {
                        Address = "099/999.999"
                    },
                    new EmergencyMedium
                    {
                        Address = "hello@world.io"
                    }
                }
            };

            await _service.Post(emergency);

            Thread.Sleep(1500);

            emergency = _service.Get(new Emergency {Id = emergency.Id});

            Assert.Equal(EmergencyStatuses.Done, emergency.Status);
            Assert.Equal(1, emergency.Events.Count(e => e.Name == "MessageReceived"));

            Thread.Sleep(10000);

            emergency = _service.Get(new Emergency {Id = emergency.Id});
            Assert.Equal(1, emergency.Events.Count(e => e.Name == "MessageReceived"));
        }
    }
}