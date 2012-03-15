using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    [AspectRoleDependency(AspectDependencyAction.Conflict, StandardRoles.Caching)]
    [ProvideAspectRole(StandardRoles.Caching)]
    public sealed class NotCacheableAttribute : MethodLevelAspect
    {
    }
}