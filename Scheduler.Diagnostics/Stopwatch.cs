using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Diagnostics
{
    public class Stopwatch : IDisposable
    {
        private System.Diagnostics.Stopwatch _stopwatch;
        public Stopwatch(string name)
        {
            Name = name;
            Start = DateTime.Now;
            _stopwatch = new System.Diagnostics.Stopwatch();
            _stopwatch.Start();
        }
        public Stopwatch(object item, string name) : this(name)
        {
            Item = Item;
        }

        public string Name { get; set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public TimeSpan Elapsed { get; set; }

        public object Item { get; private set; }

        public static event EventHandler<StopwatchCompleteEventArgs> OnStopwatchCompleted;

        private void RaiseOnStopwatchCOmpleted()
        {
            var obj = OnStopwatchCompleted;
            if (obj != null)
            {
                obj(this, new StopwatchCompleteEventArgs(this));
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_stopwatch != null)
                {
                    _stopwatch.Stop();
                    End = DateTime.Now;
                    Elapsed = _stopwatch.Elapsed;
                    RaiseOnStopwatchCOmpleted();
                }
            }
        }
    }
}
