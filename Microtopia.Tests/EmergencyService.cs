using System;
using System.Linq;
using Microtopia.Dto;
using NetCoreUtopia;

namespace Microtopia.Tests
{
    public class EmergencyService : IService, IHandle<MessageReceived>, IHandle<AlarmElapsed>
    {
        private readonly IDb _db;
        private readonly IGateway _gateway;

        private TimeSpan WaitBetweenMediumMessage { get; } = TimeSpan.FromMinutes(1);

        public EmergencyService(IDb db, IGateway gateway)
        {
            _db = db;
            _gateway = gateway;
        }

        public Emergency Get(Emergency request)
        {
            return _db.SingleById<Emergency>(request.Id);
        }

        public Emergency Post(Emergency request)
        {
            request.Status = EmergencyStatuses.Todo;
            _db.Save(request);

            var now = DateTime.Now;

            foreach (var medium in request.Mediums)
            {
                _gateway.Send(new Alarm
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

        public void Handle(MessageReceived @event)
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

            _gateway.Send(new AlarmCancel {Flow = new ChannelFlow {Id = emergency.Id}});

            emergency.Status = EmergencyStatuses.Done;
            _db.Save(emergency);
        }

        public void Handle(AlarmElapsed @event)
        {
            // ReSharper disable once PossibleInvalidOperationException
            var emergency = Get(new Emergency {Id = @event.Flow.Id.Value});

            if (emergency.Status == EmergencyStatuses.Done)
                return;

            var emergencyFlow = @event.Flow.Data.ConvertTo<EmergencyFlow>();
            var medium = emergency.Mediums.Single(m => m.Id == emergencyFlow.MediumId);

            _gateway.Send(new ChannelMessage
            {
                Message = emergency.Message,
                Address = medium.Address,
                Flow = @event.Flow
            });

            emergency.Events.Add(new EmergencyEvent
            {
                Name = "MessageSent",
                MediumId = emergencyFlow.MediumId
            });

            _db.Save(emergency);
        }
    }
}