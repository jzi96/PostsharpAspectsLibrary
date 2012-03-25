using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Extensibility;
using PostSharp.Reflection;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    /// <summary>
    /// </summary>
    /// <remarks><para>PostSharp Sample copy</para></remarks>
    public sealed class NoDataMemberAttribute : Attribute { }
    /// <summary>
    /// </summary>
    /// <remarks><para>PostSharp Sample copy</para></remarks>
    [Serializable]
    [MulticastAttributeUsage(MulticastTargets.Class, AllowMultiple = false, Inheritance = MulticastInheritance.Multicast)]
    public sealed class AddDataContractAttribute : TypeLevelAspect, IAspectProvider
    {
        #region IAspectProvider Members

        public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
        {
            var t = (Type)targetElement;
            var props = t.GetFields(BindingFlags.SetField | BindingFlags.Instance | BindingFlags.NonPublic);
            if (props.Length == 0)
                yield return null;
            var objectConstruction = new ObjectConstruction(typeof(DataContractAttribute));
            var ns = Utilities.GetContractNamespace(t);
            objectConstruction.NamedArguments.Add("Namespace", ns);
            var introduceDataContractAttribute =
                new CustomAttributeIntroductionAspect(objectConstruction);
            var introduceDataMemberAttribute =
                new CustomAttributeIntroductionAspect(new ObjectConstruction(typeof(DataMemberAttribute)));
            yield return new AspectInstance(t, introduceDataContractAttribute);
            foreach (var prop in props)
            {
                if (!prop.IsDefined(typeof(NoDataMemberAttribute), true))
                    yield return new AspectInstance(prop, introduceDataMemberAttribute);
            }

        }

        #endregion
    }
}
