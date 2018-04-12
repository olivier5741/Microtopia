using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microtopia.Dto;

#pragma warning disable 1998

namespace Microtopia.Tests
{
    // TODO implement IRequestHandler<IRequest> for response
    public class DummyCommandAndEventSink : INotificationHandler<INotification>, IRequestHandler<IRequest>
    {
        private readonly IMediator _gateway;

        public DummyCommandAndEventSink(IMediator gateway)
        {
            _gateway = gateway;
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

        private void Send(object @event)
        {
            var type = @event.GetType();
            if (Routes.ContainsKey(type) == false)
                return;

            switch (Routes[type])
            {
                default:
                    Console.WriteLine("Publish to external bus : " + @event);
                    break;
            }
        }
    }
}