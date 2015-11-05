namespace Scheduler.Diagnostics
{
    public interface IStopwatchWriter
    {
        void Write(Stopwatch stopwatch);
    }
}
