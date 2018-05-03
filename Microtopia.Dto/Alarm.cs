using System;
using MediatR;

namespace Microtopia.Dto
{
    public class Alarm : IRequest
    {
        public DateTime Time { get; set; }
        public ChannelFlow Flow { get; set; }
    }
}