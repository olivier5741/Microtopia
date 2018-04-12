using System;
using System.Collections.Generic;
using MediatR;
using NetCoreUtopia;

namespace Microtopia.Dto
{
    public class EmergencyFinished : IHaveId, INotification
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public List<EmergencyMedium> Mediums { get; set; }
        public List<EmergencyEvent> Events { get; set; } = new List<EmergencyEvent>();
        public EmergencyStatuses Status { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}