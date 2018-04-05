using System;

namespace NetCoreUtopia
{
    public interface IDb
    {
        void Save<T>(T save) where T : IHaveId;
        T SingleById<T>(Guid flowId);
    }
}