namespace DiLifeCycleDemo.Services
{
    public class Counter : ICounter
    {
        public Guid Id { get; } = Guid.NewGuid();
        public int Value { get; private set; }

        public void Increment() => Value++;
    }
}