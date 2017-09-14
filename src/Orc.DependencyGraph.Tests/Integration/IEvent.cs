namespace Orc.DependencyGraph.Tests.Integration
{
    public interface IEvent
    {
        string Name { get; }
        string Subject { get; }
        object Value { get; }
    }
}