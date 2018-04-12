using MediatR;

namespace Microtopia.Dto
{
    public class ChannelMessage : IRequest, IRequest<ChannelMessage>
    {
        public string Address { get; set; }
        public string Message { get; set; }
        public ChannelFlow Flow { get; set; }
    }
}