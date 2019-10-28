using System;
using System.Timers;
using Xamarin.Forms;

namespace Forms.Resources
{
    class JoystickSlider : Slider
    {
        private const double DefaultMin = 0;
        private const double DefaultMax = 100;
        private const long DefaultDeltaIntervalMillis = 250;

        private readonly double _min = DefaultMin;
        private readonly double _max = DefaultMax;
        private double _deltaProgress;

        public event EventHandler<DeltaChangedEventArgs> DeltaProgressChanged;

        private readonly Timer _eventTimer = new Timer();

        public JoystickSlider()
        {
            if (Minimum >= Maximum)
            {
                throw new Exception("Minimum must be less than Maximum!");
            }

            Minimum = (int)_min;
            Maximum = (int)_max;
            Value = (int)(((_max - _min) * 0.5) + _min);

            _eventTimer.Elapsed += (o, e) =>
            {
                DeltaProgressChanged?.Invoke(this, new DeltaChangedEventArgs() { DeltaProgress = _deltaProgress });
            };

            _eventTimer.Interval = DefaultDeltaIntervalMillis;

            ValueChanged += JoystickValueChanged;
            DragStarted += JoystickDragStarted;
            DragCompleted += JoystickDragCompleted;
        }
        private void JoystickValueChanged(object sender, ValueChangedEventArgs e)
        {
            _deltaProgress = (float)(Math.Pow(e.NewValue, 2) / 25 * (e.NewValue < 0 ? -1.0 : 1.0));
        }

        private void JoystickDragCompleted(object sender, EventArgs e)
        {
            _deltaProgress = 0;
            _eventTimer.Stop();

            Value = (int)(((_max - _min) * 0.5) + _min);
        }

        private void JoystickDragStarted(object sender, EventArgs e)
        {
            _eventTimer.Start();
        }
    }

    public class DeltaChangedEventArgs : EventArgs
    {
        public double DeltaProgress;
    }
}