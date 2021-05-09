using System;
using System.Timers;

namespace W65C02S.Clock
{

    public class ClockModule : IDisposable
    {
        private Timer timer;
        private ClockEdge Edge;


        public event EventHandler<ClockEventArg> OnClock;

        public ClockModule(double intervalInMilliSeconds = 1000)
        {
            if (intervalInMilliSeconds < 2)
                intervalInMilliSeconds = 2;

            timer = new System.Timers.Timer();
            timer.Interval = (intervalInMilliSeconds / 2);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = false;
            
        }

        public void Start()
        {
            timer.AutoReset = true;
            timer.Enabled = true;
        }
        public void Stop()
        {
            timer.AutoReset = false;
            timer.Enabled = false;
        }
        public void Step()
        {
            if (timer.Enabled == true)
                Stop();

            timer.AutoReset = false;
            timer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (Edge == ClockEdge.Falling)
                Edge = ClockEdge.Rising;
            else
                Edge = ClockEdge.Falling;

            if(OnClock != null)
            {
                var state = timer.Enabled;
                if (state == true)
                    timer.Enabled = false;

                var arg = new ClockEventArg
                {
                    Edge = this.Edge
                };
                OnClock.Invoke(this, arg);
                timer.Enabled = state; 
            }
        }

        public void Dispose()
        {
            timer.Elapsed -= OnTimedEvent;
        }
    }
}
