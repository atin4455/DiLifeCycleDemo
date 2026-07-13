namespace DiLifeCycleDemo.Services
{
    public interface ISomeService
    {
        ICounter Counter { get; }
        void DoSomething();
    }
}

