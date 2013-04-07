using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PostSharp.Aspects.Dependencies;
using Zieschang.Net.Projects.PostsharpAspects.Utilities;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    /// <summary>
    /// Measure the time from entering method till leaving the method.
    /// The result is logged to the <see cref="log4net.ILog"/> of
    /// the class.
    /// </summary>
    [Serializable]
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor
        , AllowMultiple= false, Inherited= false)]
    [ProvideAspectRole(StandardRoles.Tracing)]
    [AspectRoleDependency(AspectDependencyAction.Order,AspectDependencyPosition.Before, StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Tracing)]
    [StringIntern()]
#if(!DEBUG)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public sealed class TimeProfileAttribute : PostSharp.Aspects.MethodInterceptionAspect
    {
        private string _typeName;
        /// <summary>
        /// Creates a new instance of a <see cref="TimeProfileAttribute"/>
        /// </summary>
        /// <param name="message">Name of the scope measured.</param>
        public TimeProfileAttribute(string message)
            : this(message, TraceLevel.Info)
        {
        }
        /// <summary>
        /// Creates a new instance of a <see cref="TimeProfileAttribute"/>
        /// </summary>
        /// <param name="message">Name of the scope measured.</param>
        /// <param name="traceLevel">Logging level to output the value for</param>
        public TimeProfileAttribute(string message, TraceLevel traceLevel)
        {
            Level=traceLevel;
            Message = message;
        }

        /// <summary>
        /// </summary>
        /// <param name="method"></param>
        /// <param name="aspectInfo"></param>
        public override void CompileTimeInitialize(System.Reflection.MethodBase method, PostSharp.Aspects.AspectInfo aspectInfo)
        {
            _typeName = string.Intern(method.ReflectedType.Name);
        }
        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        public override void OnInvoke(PostSharp.Aspects.MethodInterceptionArgs args)
        {
            string msg = string.Format(Message, args.Arguments);
            using(new Profiler(msg, _typeName,Level, EnableHirachy))
                args.Proceed();
        }
        /// <summary>
        /// Message for the naming scope
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// If nesting of time measuerements should also be tracked, like stack.
        /// </summary>
        public bool EnableHirachy { get; set; }
        /// <summary>
        /// Logging level for the output, default is Info
        /// </summary>
        public TraceLevel Level { get; set; }
    }
}
