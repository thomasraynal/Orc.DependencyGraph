using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orc.DependencyGraph.Tests.Integration
{
    public class Event: IEvent
    {
        public Event(string name, string subject, object value)
        {
            Name = name;
            Value = value;
            Subject = subject;
        }

        public String Name { get; private set; }
        public String Subject { get; private set; }
        public object Value { get; private set; }
    }
}
