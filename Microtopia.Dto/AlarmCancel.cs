using NetCoreUtopia;

namespace Microtopia.Dto
{
    public class AlarmCancel : IReturn, ICommand
    {
        public ChannelFlow Flow { get; set; }
    }
}