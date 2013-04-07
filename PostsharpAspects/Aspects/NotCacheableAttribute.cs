using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    [AspectRoleDependency(AspectDependencyAction.Conflict, StandardRoles.Caching)]
    [ProvideAspectRole(StandardRoles.Caching)]
    [AttributeUsageAttribute(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly,
       AllowMultiple = true, Inherited = true)]
#if(RELEASE)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public sealed class NotCacheableAttribute : MethodLevelAspect
    {
    }
}