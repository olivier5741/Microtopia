using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microtopia.Dto;
using NetCoreUtopia;
using ServiceStack;

namespace Microtopia.Api
{
    [Controller]
    [Route("/emergency")]
    public class EmergencyService : INotificationHandler<ChannelMessageReceived>, INotificationHandler<AlarmElapsed>
    {
        private readonly IDb _db;
        private readonly IMediator _gateway;

        public EmergencyService(IDb db, IMediator gateway)
        {
            _db = db;
            _gateway = gateway;
        }

        private TimeSpan WaitBetweenMediumMessage { get; } = TimeSpan.FromSeconds(5);

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task Handle(AlarmElapsed @event, CancellationToken cancellationToken)
        {
            // ReSharper disable once PossibleInvalidOperationException
            var emergency = Get(new Emergency {Id = @event.Flow.Id.Value});

            if (emergency.Status == EmergencyStatuses.Done)
                return;

            var emergencyFlow = @event.Flow.Data.ConvertTo<EmergencyFlow>();
            var medium = emergency.Mediums.Single(m => m.Id == emergencyFlow.MediumId);

            await _gateway.Send(new ChannelMessage
            {
                Message = emergency.Message,
                Address = medium.Address,
                Flow = @event.Flow
            }, cancellationToken);

            emergency.Events.Add(new EmergencyEvent
            {
                Name = "MessageSent",
                MediumId = emergencyFlow.MediumId
            });

            _db.Save(emergency);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task Handle(ChannelMessageReceived @event, CancellationToken cancellationToken)
        {
            // ReSharper disable once PossibleInvalidOperationException
            var emergency = Get(new Emergency {Id = @event.Flow.Id.Value});

            var emergencyFlow = @event.Flow.Data.ConvertTo<EmergencyFlow>();

            emergency.Events.Add(new EmergencyEvent
            {
                Name = "MessageReceived",
                MediumId = emergencyFlow.MediumId
            });

            _db.Save(emergency);

            if (emergency.Status == EmergencyStatuses.Done)
                return;

            await _gateway.Send(new AlarmCancel {Flow = new ChannelFlow {Id = emergency.Id}}, cancellationToken);

            emergency.Status = EmergencyStatuses.Done;
            _db.Save(emergency);

            var emergencyFinished = emergency.ConvertTo<EmergencyFinished>();
            emergencyFinished.Time = DateTime.Now;
            await _gateway.Publish(emergencyFinished, cancellationToken);
        }

        [HttpPost]
        public async Task<Emergency> Post([FromBody] Emergency request)
        {
            request.Status = EmergencyStatuses.Todo;
            _db.Save(request);

            var now = DateTime.Now;

            foreach (var medium in request.Mediums)
            {
                await _gateway.Send(new Alarm
                {
                    Time = now,
                    Flow = new ChannelFlow {Id = request.Id, Data = new EmergencyFlow {MediumId = medium.Id}}
                });

                now = now.Add(WaitBetweenMediumMessage);
            }

            request.Events.Add(new EmergencyEvent {Name = "AlarmsSent"});

            _db.Save(request);

            return request;
        }

        [HttpGet("{id}")]
        public Emergency Get(Emergency request)
        {
            return _db.SingleById<Emergency>(request.Id);
        }
    }
}