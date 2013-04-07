using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PostSharp.Aspects.Dependencies;
using Zieschang.Net.Projects.PostsharpAspects.Utilities;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    [Serializable]
    [AttributeUsageAttribute(AttributeTargets.Method|AttributeTargets.Constructor
        , AllowMultiple= false, Inherited= false)]
    [ProvideAspectRole(StandardRoles.Tracing)]
        [AspectRoleDependency(AspectDependencyAction.Order,AspectDependencyPosition.Before, StandardRoles.Caching)]
    public sealed class TimeProfileAttribute:PostSharp.Aspects.MethodInterceptionAspect
    {
        private string _typeName;

        public TimeProfileAttribute(string message)
            : this(message, TraceLevel.Info)
        {
        }
        public TimeProfileAttribute(string message, TraceLevel traceLevel)
        {
            Level=traceLevel;
            Message = message;
        }

        public override void CompileTimeInitialize(System.Reflection.MethodBase method, PostSharp.Aspects.AspectInfo aspectInfo)
        {
            _typeName = method.ReflectedType.Name;
        }
        public override void OnInvoke(PostSharp.Aspects.MethodInterceptionArgs args)
        {
            string msg = string.Format(Message, args.Arguments);
            using(new Profiler(msg, _typeName,Level, EnableHirachy))
                args.Proceed();
        }

        public string Message { get; set; }

        public bool EnableHirachy { get; set; }

        public TraceLevel Level { get; set; }
    }
}
