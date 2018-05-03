using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microtopia.Dto
{
    public class Emergency
    {
        public string Message { get; set; }
        public List<EmergencyMedium> Mediums { get; set; }
        public List<EmergencyEvent> Events { get; set; } = new List<EmergencyEvent>();

        [JsonConverter(typeof(StringEnumConverter))]
        public EmergencyStatuses Status { get; set; }

        public Guid Id { get; set; }
    }
}