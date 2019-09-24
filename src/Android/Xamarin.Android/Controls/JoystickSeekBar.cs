using System;
using System.Collections.Generic;
using System.Timers;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Widget;
using ArcGISRuntime;

namespace ArcgisRuntime.Samples.ARToolkit.Controls
{
    public class JoystickSeekBar : AppCompatSeekBar, AppCompatSeekBar.IOnSeekBarChangeListener
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
        }

        private float offsetProgress
        {
            get
            {
                if (_min != DEFAULT_MIN || _max != DEFAULT_MAX)
                {
                    return (float)(_min + ((_max - _min) * (Progress * 0.01)));
                }
                else
                {
                    return (float)(_max * (Progress * 0.01));
                }
            }
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            deltaProgress = (float)(Math.Pow(offsetProgress, 2) / 10 * (offsetProgress < 0 ? -1 : 1));
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            eventTimer.Elapsed += (o,e) =>
            {
                DeltaProgressChanged?.Invoke(this, new DeltaChangedEventArgs() { deltaProgress = deltaProgress });
            };
            eventTimer.Interval = DEFAULT_DELTA_INTERVAL_MILLIS;
            eventTimer.Start();
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            eventTimer.Stop();
            deltaProgress = 0;

            Progress = (int)(_max * 0.5);
        }
    }

    public class DeltaChangedEventArgs : EventArgs
    {
        public double deltaProgress;
    }
}
