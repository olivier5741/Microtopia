using System;
using NetCoreUtopia;

namespace Microtopia.Tests
{
    public class DummyGateway : IGateway
    {
        public object Send<T>(T request) where T : IReturn
        {
            Console.WriteLine(request);
            return request;
        }
    }
}