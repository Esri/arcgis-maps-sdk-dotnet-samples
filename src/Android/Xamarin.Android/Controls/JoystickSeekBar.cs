using System;
using System.Timers;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;
using ArcGISRuntime;

namespace ArcgisRuntime.Samples.ARToolkit.Controls
{
    public class JoystickSeekBar : AppCompatSeekBar
    {
        private const double DEFAULT_MIN = 0;
        private const double DEFAULT_MAX = 100;
        private const long DEFAULT_DELTA_INTERVAL_MILLIS = 250;

        private double _min = DEFAULT_MIN;
        private double _max = DEFAULT_MAX;
        private double deltaProgress = 0;

        public event EventHandler<DeltaChangedEventArgs> DeltaProgressChanged;

        Timer eventTimer = new Timer();

        public JoystickSeekBar(Context context) : base(context)
        {
            Progress = (int)(_max * 0.5);
        }

        public JoystickSeekBar(Context context, IAttributeSet attrs): base(context, attrs)
        {
            var attributes = context.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.JoystickSeekBar, 0, 0);
            _min = attributes.GetFloat(Resource.Styleable.JoystickSeekBar_jsb_min, (float)DEFAULT_MIN);
            _max = attributes.GetFloat(Resource.Styleable.JoystickSeekBar_jsb_max, (float)DEFAULT_MAX);

            if (_min > _max)
            {
                throw new AndroidRuntimeException("Attribute jsb_min must be less than attribute jsb_max");
            }

            Min = (int)_min;
            Max = (int)_max;
            Progress = (int)(((_max - _min) * 0.5) + _min);

            eventTimer.Elapsed += (o, e) =>
            {
                DeltaProgressChanged?.Invoke(this, new DeltaChangedEventArgs() { deltaProgress = deltaProgress });
            };

            eventTimer.Interval = DEFAULT_DELTA_INTERVAL_MILLIS;

            ProgressChanged += JoystickSeekBar_ProgressChanged;
            StartTrackingTouch += JoystickSeekBar_StartTrackingTouch;
            StopTrackingTouch += JoystickSeekBar_StopTrackingTouch;
        }

        private void JoystickSeekBar_StopTrackingTouch(object sender, StopTrackingTouchEventArgs e)
        {
            deltaProgress = 0;
            eventTimer.Stop();

            Progress = (int)(((_max - _min) * 0.5) + _min);
        }

        private void JoystickSeekBar_StartTrackingTouch(object sender, StartTrackingTouchEventArgs e)
        {
            eventTimer.Start();
        }

        private void JoystickSeekBar_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            deltaProgress = (float)(Math.Pow(this.Progress, 2) / 25 * (this.Progress < 0 ? -1.0 : 1.0));
        }
    }

    public class DeltaChangedEventArgs : EventArgs
    {
        public double deltaProgress;
    }
}
