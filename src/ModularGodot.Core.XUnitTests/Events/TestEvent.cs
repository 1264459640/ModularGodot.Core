
using ModularGodot.Core.Contracts.Abstractions.Messaging;

namespace ModularGodot.Core.XUnitTests.Events
{
    public class TestEvent : EventBase
    {
        public string Message { get; set; }
        public int Value { get; set; }

        public TestEvent(string message, int value, string? source = null) : base(source)
        {
            Message = message;
            Value = value;
        }
    }
}