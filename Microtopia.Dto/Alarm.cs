using System;
using NetCoreUtopia;

namespace Microtopia.Dto
{
    public class Alarm : IReturn, ICommand
    {
        public DateTime Time { get; set; }
        public ChannelFlow Flow { get; set; }
    }
}