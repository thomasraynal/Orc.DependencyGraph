using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orc.DependencyGraph.Tests.Integration
{
    public abstract class IndicatorBase<TPosition, TContext> : IIndicator<TPosition, TContext>
    {
        public String Label { get; private set; }

        public IndicatorBase(String label)
        {
            Label = label;
        }

        public abstract bool Accept(IEvent ev);
        public abstract void Update(IEvent ev, TPosition position, TContext context);

        public bool Equals(IIndicator<TPosition, TContext> other)
        {
            return other.Label == Label;
        }
    }

}
