using MediatR;
using NetCoreUtopia;

namespace Microtopia.Dto
{
    public class ChannelMessageReceived : INotification
    {
        public ChannelFlow Flow { get; set; }
    }
}