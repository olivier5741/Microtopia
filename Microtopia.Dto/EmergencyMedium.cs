using System;

namespace Microtopia.Dto
{
    public class EmergencyMedium
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Address { get; set; }
    }
}