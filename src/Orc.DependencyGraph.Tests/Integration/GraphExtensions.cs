using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orc.DependencyGraph.Tests.Integration
{
    public static class GraphExtensions
    {
        public static IOrderedEnumerable<INode<T>> UniqueDescendants<T>(this INode<T> node)
            where T : IEquatable<T>
        {
            return new OrderedEnumerable<INode<T>>(() => node.Descendants.Distinct().OrderBy(x => x.Level));
        }


    }
}
