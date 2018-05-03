using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.NonGeneric;
using MediatR;
using Microtopia.Dto;

#pragma warning disable 1998

namespace Microtopia.Api
{
    // TODO implement IRequestHandler<IRequest> for response
    public class DummyCommandAndEventSink : INotificationHandler<INotification>,
        IRequestHandler<IRequest>
    {
        private readonly IBus _bus;
        private readonly IMediator _gateway;

        public DummyCommandAndEventSink(IMediator gateway, IBus bus)
        {
            _gateway = gateway;
            _bus = bus;
        }

        public Dictionary<Type, string> Routes { get; set; } = new Dictionary<Type, string>
        {
            {typeof(Alarm), "http"},
            {typeof(AlarmCancel), "http"},
            {typeof(ChannelMessage), "http"},
            {typeof(EmergencyFinished), "mq"}
        };

        public async Task Handle(INotification @event, CancellationToken cancellationToken)
        {
            Send(@event);
        }


        public async Task Handle(IRequest @event, CancellationToken cancellationToken)
        {
            Send(@event);
        }

        private void Send<T>(T @event) where T : class
        {
            var type = @event.GetType();
            if (Routes.ContainsKey(type) == false)
                return;

            switch (Routes[type])
            {
                default:

                    _bus.Publish(type, @event);

                    Console.WriteLine("Publish to external bus : " + @event);
                    break;
            }
        }
    }
}