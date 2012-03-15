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
    [ProvideAspectRole(StandardRoles.Validation)]
    [AttributeUsageAttribute(AttributeTargets.Method | AttributeTargets.Constructor,
        AllowMultiple = true, Inherited = true)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Threading)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.DataBinding)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.ExceptionHandling)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.TransactionHandling)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Persistence)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.EventBroker)]
    public sealed class ParameterNotNullCheckAttribute : OnMethodBoundaryAspect
    {
        private string _parameterName;
        public string ParameterName
        {
            get { return _parameterName; }
            set { _parameterName = value;
                _parameterIndex = -1;
            }
        }

        private int _parameterIndex=-1;
        public int ParameterIndex
        {
            get { return _parameterIndex; }
            set { _parameterIndex = value;
                _parameterName = null;
            }
        }

        public ParameterNotNullCheckAttribute(string parameterName)
        {
            _parameterName = parameterName;
        }
        public ParameterNotNullCheckAttribute(int parameterIndex)
        {
            _parameterIndex = parameterIndex;
        }
        public override bool CompileTimeValidate(System.Reflection.MethodBase method)
        {
            if (string.IsNullOrEmpty(this._parameterName) && this._parameterIndex == -1)
            {
                Message.Write(SeverityType.Error, "JZI001", "Missing paremter declaration! Set [ParameterName] or [ParameterIndex] property!");
                return false;
            }
            var param = method.GetParameters();
            if (param.Length == 0)
            {
                Message.Write(SeverityType.Error, "JZI004", "Method '{0}.{1}' does not have a parameter any parameter", method.DeclaringType.Name, method.Name);
                return false;
            }
            //validate if match
            ParameterInfo info=null;
            if (this._parameterIndex >= 0)
            {
                if (!(_parameterIndex < param.Length))
                {
                    Message.Write(SeverityType.Error, "JZI002", "Method '{0}.{1}' does not have a parameter with index {2}",  method.DeclaringType.Name,method.Name, _parameterIndex);
                    return false;
                }

                info = param[_parameterIndex];
                _parameterName = info.Name;
            }
            else
            {
                var findParam=param.FirstOrDefault(p => string.Compare(p.Name, _parameterName, StringComparison.OrdinalIgnoreCase)==0);
                if (findParam == null)
                {
                    Message.Write(SeverityType.Error, "JZI003", "Method '{0}.{1}' does not have a parameter match name {2}", method.DeclaringType.Name, method.Name, _parameterName);
                    return false;
                }
                for (int index = 0; index < param.Length; index++)
                {
                    info = param[index];
                    if (info == findParam)
                    {
                        _parameterIndex = index;
                        break;
                    }
                    info = null;
                }
            }
            if (info.ParameterType.IsValueType)
            {

                Message.Write(SeverityType.Error, "JZI004", "Method '{0}.{1}' parameter {2} is a value type, which cannot be null!",
                              method.DeclaringType.Name, method.Name, _parameterName);
                return false;
            }
            return true;
        }

        public override void OnEntry(MethodExecutionArgs args)
        {
            Debug.WriteLine("Validation parameter " + _parameterName + " at index " + _parameterIndex);
            if (args.Arguments[_parameterIndex] == null) throw new ArgumentNullException(_parameterName);
        }
    }
}
