using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microtopia.Dto;

#pragma warning disable 1998

namespace Microtopia.Tests
{
    public class DummyCommandAndEventSink : IRequestHandler<Alarm>, IRequestHandler<ChannelMessage>,
        IRequestHandler<AlarmCancel>
    {
        private readonly IMediator _gateway;

        public DummyCommandAndEventSink(IMediator _gateway)
        {
            this._gateway = _gateway;
        }

        public async Task Handle(Alarm request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request.ToString());
        }

        public async Task Handle(AlarmCancel request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request.ToString());
        }

        public async Task Handle(ChannelMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request.ToString());
        }
    }
}