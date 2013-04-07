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
    /// Increments or Decrements the counter before the call.
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
    public sealed class AbsolutCounterPerformanceCounterAttribute : CounterPerformanceCounterAttribute
    {
        /// <summary>
        /// Creates a new instance of <see cref="AbsolutCounterPerformanceCounterAttribute"/>
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="counterName"></param>
        public AbsolutCounterPerformanceCounterAttribute(string categoryName, string counterName)
            : base(categoryName, counterName)
        {
        }
        /// <summary>
        /// Creates a new instance of <see cref="AbsolutCounterPerformanceCounterAttribute"/>
        /// </summary>
        public AbsolutCounterPerformanceCounterAttribute(string categoryName, string counterName, long increaseDecreaseCount)
            : base(categoryName, counterName, increaseDecreaseCount)
        {
        }
        /// <summary>
        /// </summary>
        protected override void AfterCallInvoke(PerformanceCounter c)
        {
            //do nothing
        }
    }
}
