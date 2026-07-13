namespace DiLifeCycleDemo.Services
{
    public interface ICounter
    {
        Guid Id { get; }
        int Value { get; }
        void Increment();
    }
}
