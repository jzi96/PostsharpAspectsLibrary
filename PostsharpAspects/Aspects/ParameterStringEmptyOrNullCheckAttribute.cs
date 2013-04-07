using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    [Serializable]
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor,
     AllowMultiple = true, Inherited = true)]
    [PostSharp.Aspects.Dependencies.ProvideAspectRole(PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Tracing)]
#if(RELEASE)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public sealed class ParameterStringEmptyOrNullCheckAttribute : ParameterCheckAttribute
    {
        public ParameterStringEmptyOrNullCheckAttribute(string parameterName)
            : base(parameterName)
        {
        }
        public ParameterStringEmptyOrNullCheckAttribute(int parameterIndex)
            : base(parameterIndex)
        {
        }
        public override void OnEntry(MethodExecutionArgs args)
        {
            Debug.WriteLine("Validation parameter " + _parameterName + " at index " + _parameterIndex);
            if (String.IsNullOrEmpty((string)args.Arguments[_parameterIndex])) throw new ArgumentNullException(_parameterName);
        }

    }
}
