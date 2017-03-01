using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBox;

namespace VirtualBoxNotifier
{
    /// <summary>
    /// Implements VirtualBox.IEventListener and provides an event to notify our application
    /// We don't really care about the actual event data so the are never passed on
    /// </summary>
    class VirtualBoxSimpleEventNotifier : IEventListener
    {
        public void HandleEvent(IEvent aEvent)
        {
            EventReceived();
        }

        public event Action EventReceived;
    }
}
