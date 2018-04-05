using NetCoreUtopia;

namespace Microtopia.Dto
{
    public class ChannelMessage : IReturn<ChannelMessage>
    {
        public string Address { get; set; }
        public string Message { get; set; }
        public ChannelFlow Flow { get; set; }
    }
}