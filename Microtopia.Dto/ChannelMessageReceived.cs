using MediatR;

namespace Microtopia.Dto
{
    public class ChannelMessageReceived : INotification
    {
        public ChannelFlow Flow { get; set; }
    }
}