namespace NetCoreUtopia
{
    public interface IGateway
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        object Send<T>(T request) where T : IReturn;
    }
}