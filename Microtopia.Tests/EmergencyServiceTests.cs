using System.Collections.Generic;
using System.Linq;
using Microtopia.Api;
using Microtopia.Dto;
using Xunit;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Microtopia.Tests
{
    public class EmergencyServiceTests
    {
        private readonly EmergencyService _service;

        public EmergencyServiceTests()
        {
            _service = new EmergencyService(new DummyDb(), new DummyGateway());
        }

        [Fact]
        public void FirstMediumReplies()
        {
            var emergency = new Emergency
            {
                Message = "Help",
                Mediums = new List<EmergencyMedium>
                {
                    new EmergencyMedium
                    {
                        Address = "099/999.999"
                    },
                    new EmergencyMedium
                    {
                        Address = "hello@world.io"
                    }
                }
            };

            _service.Post(emergency);

            _service.Handle(new AlarmElapsed
            {
                Flow = new ChannelFlow
                {
                    Id = emergency.Id,
                    Data = new EmergencyFlow
                    {
                        MediumId = emergency.Mediums.First().Id
                    }
                }
            });

            _service.Handle(new MessageReceived
            {
                Flow = new ChannelFlow
                {
                    Id = emergency.Id,
                    Data = new EmergencyFlow
                    {
                        MediumId = emergency.Mediums.Skip(1).First().Id
                    }
                }
            });

            emergency = _service.Get(new Emergency {Id = emergency.Id});

            Assert.Equal(EmergencyStatuses.Done, emergency.Status);
        }
    }
}