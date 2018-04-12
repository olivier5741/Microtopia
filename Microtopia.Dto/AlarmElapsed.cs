using MediatR;

namespace Microtopia.Dto
{
    public class AlarmElapsed : INotification
    {
        public ChannelFlow Flow { get; set; }
    }
}