using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostSharp.Aspects.Dependencies;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(HandleExceptionAttribute))]
    [ProvideAspectRole(StandardRoles.ExceptionHandling)]
    public sealed class RetryOnErrorAttribute : PostSharp.Aspects.MethodInterceptionAspect
    {
    }
}
