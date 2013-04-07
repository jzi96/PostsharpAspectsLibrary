using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    //public sealed class LogCallParameterAttribute : LogCallsAttribute
    //{
    //    public LogCallParameterAttribute()
    //        : this(null, TraceEventType.Information)
    //    { }

    //    public LogCallParameterAttribute(string message)
    //        : this(message, TraceEventType.Information)
    //    { }

    //    public LogCallParameterAttribute(TraceEventType eventType)
    //        : this(null, eventType)
    //    { }

    //    public LogCallParameterAttribute(string message, TraceEventType eventType)
    //        :base(message, eventType)
    //    {
    //    }
    //}


    /// <summary>
    /// Add information to <see cref="log4net.ThreadContext"/>.
    /// </summary>
    /// <remarks><para>The context message is derived from the <see cref="EnhanceContextAttribute.Context"/>
    /// content and formatted with the ToString of the arguments.</para>
    /// <para>If the content of the arguments should be written. Provide the
    /// <see cref="string.Format(string, object)"/> references in the 
    /// <see cref="EnhanceContextAttribute.Context"/> string.</para></remarks>
    [Serializable(), AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(TimeProfileAttribute))]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(LogCallsAttribute))]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Tracing)]
    [ProvideAspectRole(StandardRoles.Tracing)]
    [StringIntern()]
#if(!DEBUG)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public sealed class EnhanceContextAttribute : MethodInterceptionAspect
    {
        private const string DefaultLoggerContextStack = "NDC";
        public EnhanceContextAttribute(string message)
        {
            _context = string.Intern(message);
        }
        private string _context;
        public string Context
        {
            get { return _context; }
            set { _context = value; }
        }

        public override void OnInvoke(PostSharp.Aspects.MethodInterceptionArgs args)
        {
            string exContext = string.Format(System.Globalization.CultureInfo.InvariantCulture, _context, args.Arguments.ToArray());
            using (log4net.ThreadContext.Stacks[Zieschang.Net.Projects.PostsharpAspects.Utilities.LogHelper.DefaultLoggerContextStack].Push(exContext))
            {
                args.Proceed();
            }
        }
    }
}
