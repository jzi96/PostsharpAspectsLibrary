using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using Zieschang.Net.Projects.PostsharpAspects.Utilities;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    /// <summary>
    /// Log an entry when the method is called and also logs the leaving
    /// of the method.
    /// </summary>
    [Serializable] 
	[AttributeUsageAttribute(
        AttributeTargets.Assembly |AttributeTargets.Class |
        AttributeTargets.Method | AttributeTargets.Constructor|AttributeTargets.Property
        , AllowMultiple = true, Inherited= true)]
    [ProvideAspectRole(StandardRoles.Tracing)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Tracing)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching)]
    [AspectTypeDependency(AspectDependencyAction.Order,  AspectDependencyPosition.After
        , typeof(TimeProfileAttribute))]
    [StringIntern()]
#if(!DEBUG)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public class LogCallsAttribute : OnMethodBoundaryAspect
    {
        private const string FallbackLoggerName = "Calls";
        private string _leaving;
        private readonly TraceEventType _level;
        private string _entering;
        private string _leavingWithException;
        private string _error;
        private readonly string _message;
        private log4net.ILog _lg;

        /// <summary>
        /// </summary>
        /// <param name="method"></param>
        public override void RuntimeInitialize(System.Reflection.MethodBase method)
        {
            _lg = InternalFieldFinder.Instance.GetInstance<log4net.ILog>(method, null);
        }
        /// <summary>
        /// Create a new instance of <see cref="LogCallsAttribute"/>
        /// </summary>
        public LogCallsAttribute()
        :this(null, TraceEventType.Information)
        {}
        /// <summary>
        /// Create a new instance of <see cref="LogCallsAttribute"/>
        /// </summary>

        public LogCallsAttribute(string message):this(message, TraceEventType.Information)
        {}
        /// <summary>
        /// Create a new instance of <see cref="LogCallsAttribute"/>
        /// </summary>

        public LogCallsAttribute(TraceEventType eventType):this(null, eventType)
        {}
        /// <summary>
        /// Create a new instance of <see cref="LogCallsAttribute"/>
        /// </summary>

        public LogCallsAttribute(string message, TraceEventType eventType)
        {
            _message = message;
            _level = eventType;
        }
        /// <summary>
        /// </summary>
        /// <param name="method"></param>
        /// <param name="aspectInfo"></param>
        public override void CompileTimeInitialize(System.Reflection.MethodBase method, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(method, aspectInfo);
            Type reflectedType;
            try
            {
                reflectedType = method.ReflectedType;
            }
            catch (Exception)
            {
                reflectedType = method.DeclaringType;
            }
            string methodName = method.Name;
            _entering = "Entering " + reflectedType.Name + "." + methodName;
            if (!string.IsNullOrEmpty(_message))
                _entering += Environment.NewLine;
            _leaving = "Leaving " + reflectedType.Name + "." + methodName;
            _leavingWithException = "Leaving " + reflectedType.Name + "." + methodName +" with Exception: ";
            _error = "Error executing " + reflectedType.Name + "." + methodName;
        }
        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private log4net.ILog GetLogger(MethodExecutionArgs args)
        {
            log4net.ILog lg = _lg ?? InternalFieldFinder.Instance.GetInstance<log4net.ILog>(args.Method, args.Instance);
            return lg ?? log4net.LogManager.GetLogger(FallbackLoggerName);
        }
        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        public override void OnEntry(MethodExecutionArgs args)
        {
            log4net.ILog lg = GetLogger(args);
            string s = _entering;
            if (!string.IsNullOrEmpty(_message)) s += string.Format(_message, args.Arguments.ToArray());
            lg.Send(_level, s);
        }
        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        public override void OnException(MethodExecutionArgs args)
        {
            log4net.ILog lg = GetLogger(args);
            lg.Error(_error, args.Exception);
            if (lg.IsDebugEnabled)
            {
                foreach (var argument in args.Arguments)
                {
                    lg.Debug(argument);
                }
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        public override void OnExit(MethodExecutionArgs args)
        {
            log4net.ILog lg = GetLogger(args);
            if (args.Exception == null)
            {
                lg.Send(_level, _leaving);
            }
            else
            {
                lg.Send(_level, _leavingWithException + args.Exception.Message);
            }
        }
    }
}