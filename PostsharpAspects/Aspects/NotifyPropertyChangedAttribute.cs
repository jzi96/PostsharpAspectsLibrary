using System;
using System.ComponentModel;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using PostSharp.Aspects.Dependencies;
using System.Diagnostics;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    /// <summary>
    /// Applies the <see cref="INotifyPropertyChanged"/> implementation to the class
    /// and the properties within
    /// </summary>
    [Serializable]
    [IntroduceInterface( typeof(INotifyPropertyChanged), OverrideAction = InterfaceOverrideAction.Ignore )]
    [MulticastAttributeUsage( MulticastTargets.Class, Inheritance = MulticastInheritance.Strict )]
    [PostSharp.Aspects.Dependencies.ProvideAspectRole(PostSharp.Aspects.Dependencies.StandardRoles.DataBinding)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Tracing)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.DataBinding)]
    [AttributeUsageAttribute(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Assembly,
         AllowMultiple = true, Inherited = true)]
#if(!DEBUG)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public sealed class NotifyPropertyChangedAttribute : InstanceLevelAspect, INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        [ImportMember("OnPropertyChanged", IsRequired = false, Order = ImportMemberOrder.AfterIntroductions)]
        public readonly Action<string> OnPropertyChangedMethod;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName"></param>
        [IntroduceMember(Visibility = Visibility.Family, IsVirtual = true, OverrideAction = MemberOverrideAction.OverrideOrIgnore)]
        public void OnPropertyChanged(string propertyName)
        {
            //overcome raceconditions
            PropertyChangedEventHandler pc = PropertyChanged;
            if (pc != null)
            {
                pc(Instance, new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// Notifies of property changes
        /// </summary>
        [IntroduceMember( OverrideAction = MemberOverrideAction.OverrideOrIgnore )]
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// If property value will be set, but only if different to the current implementation
        /// </summary>
        /// <param name="args"></param>
        [OnLocationSetValueAdvice, MulticastPointcut( Targets = MulticastTargets.Property, Attributes = MulticastAttributes.Instance | MulticastAttributes.NonAbstract)]
        public void OnPropertySet( LocationInterceptionArgs args )
        {
            // Don't go further if the new value is equal to the old one.
            // (Possibly use object.Equals here).
            if ( args.Value == args.GetCurrentValue() ) return;

            // Actually sets the value.
            args.ProceedSetValue();

            OnPropertyChangedMethod.Invoke( args.Location.Name );
            
        }
    }
}