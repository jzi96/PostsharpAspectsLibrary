using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostSharp.Extensibility;
using PostSharp.Aspects;
using PostSharp.Reflection;
using System.Diagnostics;
using PostSharp.Aspects.Dependencies;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    /// <summary>
    /// Attribute that when applied, applies string interning to all applicable string fields and properties
    /// </summary>
    /// <remarks>
    /// <para>This is a clone from http://theburningmonk.com/2013/02/aop-string-interning-with-postsharp-attribute/
    /// It uses <see cref="string.Intern"/> for optimizing strings and
    /// finding identical instances.
    /// </para>
    /// </remarks>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly | AttributeTargets.Field | AttributeTargets.Property,
                    Inherited = true)]
    [MulticastAttributeUsage(MulticastTargets.Field | MulticastTargets.Property,
                             Inheritance = MulticastInheritance.Multicast,AllowMultiple=false)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.TransactionHandling)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.DataBinding)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Tracing)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Persistence)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Tracing)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.DataBinding)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.ExceptionHandling)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Caching)]
    [PostSharp.Aspects.Dependencies.ProvideAspectRole("Optimization")]
#if(!DEBUG)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public sealed class StringInternAttribute : LocationInterceptionAspect
    {
        /// <summary>
        /// </summary>
        /// <param name="locationInfo"></param>
        /// <returns></returns>
        public override bool CompileTimeValidate(LocationInfo locationInfo)
        {
            // ignore if not a string property/field
            return locationInfo.LocationType == typeof(string) && base.CompileTimeValidate(locationInfo);
        }
        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        public override void OnSetValue(LocationInterceptionArgs args)
        {
            args.SetNewValue(string.Intern((string)args.Value));
        }
    }
}
