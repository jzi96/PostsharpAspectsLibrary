using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    [Serializable] 
	[AttributeUsageAttribute(AttributeTargets.Assembly |AttributeTargets.Class |AttributeTargets.Method | AttributeTargets.Constructor|AttributeTargets.Property
        , AllowMultiple = true, Inherited= true)]
    [ProvideAspectRole(StandardRoles.Tracing)]
    [AspectRoleDependency(AspectDependencyAction.Order,AspectDependencyPosition.After, StandardRoles.Caching)]
    [AspectTypeDependency(AspectDependencyAction.Order,  AspectDependencyPosition.After
        , typeof(TimeProfileAttribute))]
    public sealed class LogCallsAttribute : OnMethodBoundaryAspect
    {
        private const string FallbackLoggerName = "Calls";
        private string _leaving;
        private readonly TraceEventType _level;
        private string _entering;
        private string _leavingWithException;
        private string _error;
        private readonly string _message;
        private log4net.ILog _lg;


        public override void RuntimeInitialize(System.Reflection.MethodBase method)
        {
            _lg = InternalFieldFinder.Instance.GetInstance<log4net.ILog>(method, null);
        }

        public LogCallsAttribute()
        :this(null, TraceEventType.Information)
        {}

        public LogCallsAttribute(string message):this(message, TraceEventType.Information)
        {}

        public LogCallsAttribute(TraceEventType eventType):this(null, eventType)
        {}

        public LogCallsAttribute(string message, TraceEventType eventType)
        {
            _message = message;
            _level = eventType;
        }
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
        private log4net.ILog GetLogger(MethodExecutionArgs args)
        {
            log4net.ILog lg = _lg ?? InternalFieldFinder.Instance.GetInstance<log4net.ILog>(args.Method, args.Instance);
            return lg ?? log4net.LogManager.GetLogger(FallbackLoggerName);
        }
        public override void OnEntry(MethodExecutionArgs args)
        {
            log4net.ILog lg = GetLogger(args);
            string s = _entering;
            if (!string.IsNullOrEmpty(_message)) s += string.Format(_message, args.Arguments.ToArray());
            lg.Send(_level, s);
        }
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
