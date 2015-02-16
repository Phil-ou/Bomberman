using System;
using System.Threading;

namespace Bomberman.Server.Console
{
    //http://stackoverflow.com/questions/6379541/reliably-stop-system-threading-timer
    public class TimerHelper<T> : IDisposable
    {
        private Timer _timer;
        private readonly object _threadLock = new object();

        public Action<Timer, T> TimerAction { get; set; }

        public void Start(TimeSpan timerInterval, bool triggerAtStart = false, T state = default(T))
        {
            Stop();
            _timer = new Timer(Timer_Elapsed, state, Timeout.Infinite, Timeout.Infinite);
            _timer.Change(triggerAtStart ? TimeSpan.FromTicks(0) : timerInterval, timerInterval);
        }

        public void Stop()
        {
            // Wait for timer queue to be emptied, before we continue (Timer threads should have left the callback method given)
            // - http://woowaabob.blogspot.dk/2010/05/properly-disposing-systemthreadingtimer.html
            // - http://blogs.msdn.com/b/danielvl/archive/2011/02/18/disposing-system-threading-timer.aspx
            lock (_threadLock)
            {
                if (_timer != null)
                {
                    WaitHandle waitHandle = new AutoResetEvent(false);
                    _timer.Dispose(waitHandle);
                    WaitHandle.WaitAll(new[] { waitHandle }, TimeSpan.FromMinutes(2));
                    _timer = null;
                }
            }
        }

        void Timer_Elapsed(object state)
        {
            // Ensure that we don't have multiple timers active at the same time
            // - Also prevents ObjectDisposedException when using Timer-object inside this method
            // - Maybe consider to use _timer.Change(interval, Timeout.Infinite) (AutoReset = false)
            if (Monitor.TryEnter(_threadLock))
            {
                try
                {
                    if (_timer == null)
                        return;
                    Action<Timer, T> timerEvent = TimerAction;
                    if (timerEvent != null)
                    {
                        T o = (T)state;
                        timerEvent(_timer, o);
                    }
                }
                finally
                {
                    Monitor.Exit(_threadLock);
                }
            }
        }


        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                TimerAction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
