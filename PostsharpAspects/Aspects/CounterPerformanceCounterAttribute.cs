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
    /// Gets incremented before the call and will be decremented after the call
    /// </summary>
    [Serializable]
    [AttributeUsageAttribute(AttributeTargets.Method | AttributeTargets.Constructor,
    AllowMultiple = true, Inherited = true)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Tracing)]
#if(!DEBUG)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public abstract class CounterPerformanceCounterAttribute : PerformanceCounterBaseAttribute
    {
        private readonly long _increaseDecrease = 1;
        /// <summary>
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="counterName"></param>
        protected CounterPerformanceCounterAttribute(string categoryName, string counterName)
            : base(categoryName, counterName, PerformanceCounterType.NumberOfItems64)
        {
        }
        /// <summary>
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="counterName"></param>
        /// <param name="increaseDecreaseCount">Value to increase or decrease the counter. The default value is 1.</param>
        protected CounterPerformanceCounterAttribute(string categoryName, string counterName, long increaseDecreaseCount)
            : this(categoryName, counterName)
        {
            _increaseDecrease = increaseDecreaseCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public override void OnInvoke(MethodInterceptionArgs args)
        {
            PerformanceCounter c = this.GetCounter();
            BeforeCallInvoke(c);
            try
            {
                base.OnInvoke(args);
            }
            finally
            {
                AfterCallInvoke(c);
            }
        }
        /// <summary>
        /// Invoked before passing to the method
        /// </summary>
        /// <param name="c"></param>
        protected virtual void BeforeCallInvoke(PerformanceCounter c)
        {
            if (c != null)
            {
                lock (c)
                {
                    string name = c.InstanceName;
                    c.IncrementBy(_increaseDecrease);
                    //also update overall insance!
                    c.InstanceName = OverallInstance;
                    c.IncrementBy(_increaseDecrease);
                    c.InstanceName = name;
                }
            }
        }
        /// <summary>
        /// Invoked after processed by the method
        /// </summary>
        /// <param name="c"></param>
        protected virtual void AfterCallInvoke(PerformanceCounter c)
        {
            if (c != null)
            {
                lock (c)
                {

                    string name = c.InstanceName;
                    //reset name
                    c.IncrementBy(-_increaseDecrease);
                    //also update overall insance!
                    c.InstanceName = OverallInstance;
                    c.IncrementBy(-_increaseDecrease);
                    c.InstanceName = name;
                }
            }
        }
    }
}
