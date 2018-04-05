using NetCoreUtopia;

namespace Microtopia.Dto
{
    public class AlarmElapsed : IEvent
    {
        public ChannelFlow Flow { get; set; }
    }
}