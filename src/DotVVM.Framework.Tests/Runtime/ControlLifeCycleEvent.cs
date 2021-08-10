using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tests.Runtime
{
    internal class ControlLifeCycleEvent
    {

        public ControlLifeCycleEvent(ControlLifeCycleMock control, LifeCycleEventType eventType, bool isEntering)
        {
            Control = control;
            EventType = eventType;
            IsEntering = isEntering;
        }

        public ControlLifeCycleMock Control { get; }

        public LifeCycleEventType EventType { get; }

        public bool IsEntering { get; }

        public override string ToString() => $"{Control.Name}: {(IsEntering ? "entering" : "leaving")} {EventType.ToString()}";
    }
}