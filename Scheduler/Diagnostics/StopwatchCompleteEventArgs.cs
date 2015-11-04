using System;

namespace Scheduler.Diagnostics
{
    public class StopwatchCompleteEventArgs : EventArgs
    {
        public StopwatchCompleteEventArgs(Stopwatch stopwatch)
        {
            Stopwatch = stopwatch;
        }
        public Stopwatch Stopwatch { get; private set; }
    }
}
