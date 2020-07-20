using Android.Content;
using Android.Util;
using AndroidX.AppCompat.Widget;
using ArcGISRuntime;
using System;
using System.Timers;

namespace ArcGISRuntimeXamarin.Samples.ARToolkit.Controls
{
    public class JoystickSeekBar : AppCompatSeekBar
    {
        private const double DefaultMin = 0;
        private const double DefaultMax = 100;
        private const long DefaultDeltaIntervalMillis = 250;

        private readonly double _min = DefaultMin;
        private readonly double _max = DefaultMax;
        private double _deltaProgress;

        public event EventHandler<DeltaChangedEventArgs> DeltaProgressChanged;

        private readonly Timer _eventTimer = new Timer();

        public JoystickSeekBar(Context context) : base(context)
        {
            Progress = (int)(_max * 0.5);
        }

        public JoystickSeekBar(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            var attributes = context.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.JoystickSeekBar, 0, 0);
            _min = attributes.GetFloat(Resource.Styleable.JoystickSeekBar_jsb_min, (float)DefaultMin);
            _max = attributes.GetFloat(Resource.Styleable.JoystickSeekBar_jsb_max, (float)DefaultMax);

            if (_min > _max)
            {
                throw new AndroidRuntimeException("Attribute jsb_min must be less than attribute jsb_max");
            }

            Min = (int)_min;
            Max = (int)_max;
            Progress = (int)(((_max - _min) * 0.5) + _min);

            _eventTimer.Elapsed += (o, e) =>
            {
                DeltaProgressChanged?.Invoke(this, new DeltaChangedEventArgs() { DeltaProgress = _deltaProgress });
            };

            _eventTimer.Interval = DefaultDeltaIntervalMillis;

            ProgressChanged += JoystickSeekBar_ProgressChanged;
            StartTrackingTouch += JoystickSeekBar_StartTrackingTouch;
            StopTrackingTouch += JoystickSeekBar_StopTrackingTouch;
        }

        private void JoystickSeekBar_StopTrackingTouch(object sender, StopTrackingTouchEventArgs e)
        {
            _deltaProgress = 0;
            _eventTimer.Stop();

            Progress = (int)(((_max - _min) * 0.5) + _min);
        }

        private void JoystickSeekBar_StartTrackingTouch(object sender, StartTrackingTouchEventArgs e)
        {
            _eventTimer.Start();
        }

        private void JoystickSeekBar_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _deltaProgress = (float)(Math.Pow(this.Progress, 2) / 25 * (this.Progress < 0 ? -1.0 : 1.0));
        }
    }

    public class DeltaChangedEventArgs : EventArgs
    {
        public double DeltaProgress;
    }
}