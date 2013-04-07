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
    /// <summary>
    /// Inform the specified Counter about changes (rates)
    /// </summary>
    [Serializable]
    [AttributeUsageAttribute(AttributeTargets.Method | AttributeTargets.Constructor,
     AllowMultiple = true, Inherited = true)]
    [PostSharp.Aspects.Dependencies.ProvideAspectRole(PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Tracing)]
#if(RELEASE)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public sealed class RatePerformanceCounterAttribute : PerformanceCounterBaseAttribute
    {
        private readonly CounterInvokeDirection _direction;
        public CounterInvokeDirection Direction
        {
            get { return _direction; }
        }
        public RatePerformanceCounterAttribute(string categoryName, string counterName)
            : this(categoryName, counterName, CounterInvokeDirection.BeforeInvoke)
        {
        }
        public RatePerformanceCounterAttribute(string categoryName, string counterName, CounterInvokeDirection direction)
            : base(categoryName, counterName, PerformanceCounterType.RateOfCountsPerSecond32)
        {
            _direction = direction;
        }
        public override void OnInvoke(MethodInterceptionArgs args)
        {
            PerformanceCounter c = this.GetCounter();
            UpdateCounterConditional(c, CounterInvokeDirection.BeforeInvoke);
            try
            {
                base.OnInvoke(args);
            }
            catch
            {
                UpdateCounterConditional(c, CounterInvokeDirection.OnException);
            }
            finally
            {
                UpdateCounterConditional(c, CounterInvokeDirection.AfterInvoke);
            }
        }

        private void UpdateCounterConditional(PerformanceCounter c, CounterInvokeDirection expectedDirection)
        {
            if (c != null && (expectedDirection & _direction) == expectedDirection)
            {
                lock (c)
                {
                    //reset name
                    string ins = c.InstanceName;
                    c.Increment();
                    //also update overall insance!
                    c.InstanceName = OverallInstance;
                    c.Increment();
                    c.InstanceName = ins;
                }
            }
        }
    }
    /// <summary>
    /// List of enumartion to define the invokation direction of
    /// PerformanceCounterAttributes.
    /// </summary>
    [Flags]
    public enum CounterInvokeDirection
    {
        None = 0,
        /// <summary>
        /// The counter will be manipulated before the intended call to the method
        /// </summary>
        BeforeInvoke = 1,
        /// <summary>
        /// The counter will be manipulated after the intended call to the method
        /// </summary>
        AfterInvoke = 2,
        /// <summary>
        /// The counter will be manipulated on entering and on leaving the intended method
        /// </summary>
        Both = BeforeInvoke | AfterInvoke,
        /// <summary>
        /// The counter will be manipulated, if there was an exception
        /// </summary>
        OnException = 4
    }
}
