using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    [Serializable]
    [AttributeUsageAttribute(AttributeTargets.Method | AttributeTargets.Constructor,
        AllowMultiple = true, Inherited = true)]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(LogCallsAttribute))]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(TimeProfileAttribute))]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Tracing)]
    [ProvideAspectRole(StandardRoles.ExceptionHandling)]
    public sealed class HandleExceptionAttribute : PostSharp.Aspects.OnExceptionAspect
    {
        private bool _suppressException;
        private log4net.ILog _logger;

        private static readonly log4net.ILog FallBackLogger =
            log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().ReflectedType);
        public HandleExceptionAttribute(Type exceptionToHandle, bool suppressException)
        {
            ExceptionToHandle = exceptionToHandle;
            _suppressException = suppressException;
        }

        public Type ExceptionToHandle { get; set; }
        public bool TraceParameters { get; set; }
        public string Msg { get; set; }
        public Type WrapExceptionType { get; set; }
        public override Type GetExceptionType(System.Reflection.MethodBase targetMethod)
        {
            return ExceptionToHandle;
        }
        public override bool CompileTimeValidate(MethodBase method)
        {
            if (WrapExceptionType != null)
            {
                if (typeof(Exception).IsAssignableFrom(WrapExceptionType))
                {
                    Message.Write(SeverityType.Error,"PJZI001","WrapExceptionType must be of type exception!");
                    return false;
                }
            }
            return base.CompileTimeValidate(method);
        }

        public override void OnException(PostSharp.Aspects.MethodExecutionArgs args)
        {
            log4net.ILog log = _logger;

            if(log==null)
                log = InternalFieldFinder.Instance.GetInstance<log4net.ILog>(args.Method, args.Instance);
            if (log == null)
                log = FallBackLogger;
            log.Error(Msg, args.Exception);
            args.FlowBehavior = _suppressException ? FlowBehavior.Continue : FlowBehavior.RethrowException;
            if (TraceParameters && args.Arguments != null && args.Arguments.Count>0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < args.Arguments.Count; i++)
                {
                    object arg = args.Arguments[i];
                    sb.Append(i).Append(". - ");
                    if (arg == null)
                        sb.AppendLine("<NULL>");
                    else
                        sb.AppendLine(arg.ToString());
                }
                log.Error(sb.ToString());
            }
            if (WrapExceptionType != null)
            {
                args.Exception = (Exception)Activator.CreateInstance(WrapExceptionType, Msg, args.Exception);
                args.FlowBehavior = FlowBehavior.ThrowException;
            }
            log = null;
        }
        public override void RuntimeInitialize(System.Reflection.MethodBase method)
        {
            _logger = InternalFieldFinder.Instance.GetInstance<log4net.ILog>(method, null);
        }
    }
}
