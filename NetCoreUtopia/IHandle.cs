namespace NetCoreUtopia
{
    public interface IHandle<in T>
    {
        // ReSharper disable once UnusedMemberInSuper.Global
        void Handle(T @event);
    }
}