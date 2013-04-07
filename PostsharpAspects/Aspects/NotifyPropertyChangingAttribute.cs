using System;
using System.ComponentModel;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using PostSharp.Aspects.Dependencies;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    /// <summary>
    /// for some reasons, this does not work when integrated in test assembly
    /// </summary>
    [NotifyPropertyChanged]
    [NotifyPropertyChanging]
    internal class CheckNotification
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    /// <summary>
    /// Apply <see cref="INotifyPropertyChanging"/> implementation to the class
    /// </summary>
    [Serializable]
    [IntroduceInterface(typeof(INotifyPropertyChanging), OverrideAction = InterfaceOverrideAction.Ignore)]
    [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict)]
    [ProvideAspectRole(StandardRoles.DataBinding)]
    [AttributeUsageAttribute(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Assembly,
         AllowMultiple = true, Inherited = true)]
    [PostSharp.Aspects.Dependencies.ProvideAspectRole(PostSharp.Aspects.Dependencies.StandardRoles.DataBinding)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Tracing)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.DataBinding)]
#if(RELEASE)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public sealed class NotifyPropertyChangingAttribute : InstanceLevelAspect, INotifyPropertyChanging
    {

        [ImportMember("OnPropertyChanging", IsRequired = false)]
        public readonly Action<string> OnPropertyChangingMethod;
        [IntroduceMember(Visibility = Visibility.Family, IsVirtual = true, OverrideAction = MemberOverrideAction.OverrideOrIgnore)]
        public void OnPropertyChanging(string propertyName)
        {
            PropertyChangingEventHandler pc = PropertyChanging;
            if (pc != null)
            {
                pc(Instance, new PropertyChangingEventArgs(propertyName));
            }
        }
        [IntroduceMember(OverrideAction = MemberOverrideAction.OverrideOrIgnore)]
        public event PropertyChangingEventHandler PropertyChanging;

        [OnLocationSetValueAdvice, MulticastPointcut(Targets = MulticastTargets.Property, Attributes = MulticastAttributes.Instance | MulticastAttributes.NonAbstract)]
        public void OnPropertySet(LocationInterceptionArgs args)
        {
            // Don't go further if the new value is equal to the old one.
            // (Possibly use object.Equals here).
            if (args.Value == args.GetCurrentValue()) return;

            OnPropertyChangingMethod.Invoke(args.Location.Name);

            // Actually sets the value.
            args.ProceedSetValue();

        }
    }
}
