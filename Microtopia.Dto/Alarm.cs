using System;
using MediatR;

namespace Microtopia.Dto
{
    public class Alarm : IRequest, IRequest<Alarm>
    {
        public DateTime Time { get; set; }
        public ChannelFlow Flow { get; set; }
    }
}