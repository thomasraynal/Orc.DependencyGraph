using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orc.DependencyGraph.Tests.Integration
{
    public interface IIndicator<TPosition, TContext> : IEquatable<IIndicator<TPosition, TContext>>
    {
        String Label { get; }
        bool Accept(IEvent ev);
        void Update(IEvent ev, TPosition position, TContext context);
    }
}
