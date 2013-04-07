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
    [PostSharp.Aspects.Dependencies.ProvideAspectRole(PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Tracing)]
#if(RELEASE)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public sealed class CallsCounterPerformanceCounterAttribute : CounterPerformanceCounterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="counterName"></param>
        public CallsCounterPerformanceCounterAttribute(string categoryName, string counterName)
            : base(categoryName, counterName)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="counterName"></param>
        /// <param name="increaseDecreaseCount"></param>
        public CallsCounterPerformanceCounterAttribute(string categoryName, string counterName, long increaseDecreaseCount)
            : base(categoryName, counterName, increaseDecreaseCount)
        {
        }
    }
}
