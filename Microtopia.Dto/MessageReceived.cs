using NetCoreUtopia;

namespace Microtopia.Dto
{
    public class MessageReceived : IEvent
    {
        public ChannelFlow Flow { get; set; }
    }
}