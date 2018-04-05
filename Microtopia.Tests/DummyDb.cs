using System;
using System.Collections.Generic;
using NetCoreUtopia;

namespace Microtopia.Tests
{
    public class DummyDb : IDb
    {
        private readonly Dictionary<Guid, object> _dict = new Dictionary<Guid, object>();

        public void Save<T>(T save) where T : IHaveId
        {
            _dict[save.Id] = save;
        }

        public T SingleById<T>(Guid id)
        {
            return (T) _dict[id];
        }
    }
}