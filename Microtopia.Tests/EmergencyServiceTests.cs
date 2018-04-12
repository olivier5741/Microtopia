using System.Collections.Generic;
using System.Linq;
using MediatR;
using Microtopia.Api;
using Microtopia.Dto;
using NetCoreUtopia;
using StructureMap;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Microtopia.Tests
{
    public class EmergencyServiceTests
    {
        public EmergencyServiceTests()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType<DummyCommandAndEventSink>();
                    scanner.AssemblyContainingType<EmergencyService>();
                    scanner.ConnectImplementationsToTypesClosing(
                        typeof(IRequestHandler<>)); // Handlers with no response
                    scanner.ConnectImplementationsToTypesClosing(
                        typeof(IRequestHandler<,>)); // Handlers with a response
                    scanner.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
                });
                cfg.For<IDb>().Use<DummyDb>().Singleton();
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => ctx.GetInstance);
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => ctx.GetAllInstances);
                cfg.For<IMediator>().Use<Mediator>();
            });

            _gateway = container.GetInstance<IMediator>();
            _service = container.GetInstance<EmergencyService>();
            ;
        }

        private readonly EmergencyService _service;
        private readonly IMediator _gateway;

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

            await _gateway.Publish(new AlarmElapsed
            {
                Flow = new ChannelFlow
                {
                    Id = emergency.Id,
                    Data = new EmergencyFlow
                    {
                        MediumId = emergency.Mediums.First().Id
                    }
                }
            });

            await _gateway.Publish(new ChannelMessageReceived
            {
                Flow = new ChannelFlow
                {
                    Id = emergency.Id,
                    Data = new EmergencyFlow
                    {
                        MediumId = emergency.Mediums.Skip(1).First().Id
                    }
                }
            });

            emergency = _service.Get(new Emergency {Id = emergency.Id});

            Assert.Equal(EmergencyStatuses.Done, emergency.Status);
        }
    }
}