using MediatR;

namespace Microtopia.Dto
{
    public class AlarmCancel : IRequest, IRequest<Alarm>
    {
        public ChannelFlow Flow { get; set; }
    }
}