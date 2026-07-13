namespace DiLifeCycleDemo.Services
{
    public class SomeService : ISomeService
    {
        public ICounter Counter { get; }

        // 這裡也注入了 ICounter
        public SomeService(ICounter counter)
        {
            Counter = counter;
        }

        public void DoSomething() => Counter.Increment();
    }
}
