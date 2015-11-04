using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Scheduler.Diagnostics
{
    public class StopwatchListener : IDisposable
    {
        private TimeSpan _monitorDelay = TimeSpan.FromMilliseconds(100);
        private ConcurrentQueue<Stopwatch> _queue;
        private IStopwatchWriter _writer;
        private Thread _worker;
        public StopwatchListener(IStopwatchWriter writer)
        {
            _queue = new ConcurrentQueue<Stopwatch>();
            _writer = writer;
            Stopwatch.OnStopwatchCompleted += Stopwatch_OnStopwatchCompleted;
            _worker = new Thread(new ThreadStart(QueueMonitor));
            _worker.Start();
        }

        private void Stopwatch_OnStopwatchCompleted(object sender, StopwatchCompleteEventArgs e)
        {
            _queue.Enqueue(e.Stopwatch);
        }

        private void QueueMonitor()
        {
            while (true)
            {
                DequeueAllItems();
                Thread.Sleep(_monitorDelay);
            }
        }

        private void DequeueAllItems()
        {
            var currentCount = _queue.Count();
            if(currentCount == 0)
            {
                return;
            }
            for (int i = 0; i < currentCount; i++)
            {
                Stopwatch sw = null;
                if (_queue.TryDequeue(out sw))
                {
                    _writer.Write(sw);
                }
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

            }
        }
    }
}
