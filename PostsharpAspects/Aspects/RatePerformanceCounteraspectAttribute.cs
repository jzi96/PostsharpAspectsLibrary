using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System.Diagnostics;
using System.Threading;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    [Serializable]
    public sealed class RatePerformanceCounterAspectAttribute : PerformanceCounterBaseAspectAttribute
    {
        public RatePerformanceCounterAspectAttribute()
        {
            _counterType = PerformanceCounterType.RateOfCountsPerSecond32;
        }
        public override void OnInvoke(MethodInterceptionArgs args)
        {
            PerformanceCounter c = this.GetCounter();
            if (c != null)
            {
                c.Increment();
                //also update overall insance!
                c.InstanceName = OverallInstance;
                c.Increment();
            }
            base.OnInvoke(args);
            if (c != null)
            {
                //reset name
                c.InstanceName = CounterName;
                c.Decrement();
                //also update overall insance!
                c.InstanceName = OverallInstance;
                c.Decrement();

            }
        }
    }
}
