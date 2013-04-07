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
    /// <summary>
    /// Base implementation for implementing checks on parameter level
    /// </summary>
    [Serializable]
    [ProvideAspectRole(StandardRoles.Validation)]
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor,
        AllowMultiple = true, Inherited = true)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Threading)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.DataBinding)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.ExceptionHandling)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.TransactionHandling)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Persistence)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.EventBroker)]
    [PostSharp.Aspects.Dependencies.ProvideAspectRole(PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Tracing)]
    [StringIntern()]
#if(!DEBUG)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public abstract class ParameterCheckAttribute : OnMethodBoundaryAspect
    {

        [CLSCompliant(false)]
        protected string _parameterName;
        public string ParameterName
        {
            get { return _parameterName; }
            set
            {
                _parameterName = value;
                _parameterIndex = -1;
            }
        }

        [CLSCompliant(false)]
        protected int _parameterIndex = -1;
        public int ParameterIndex
        {
            get { return _parameterIndex; }
            set
            {
                _parameterIndex = value;
                _parameterName = null;
            }
        }

        public ParameterCheckAttribute(string parameterName)
        {
            _parameterName = string.Intern(parameterName);
        }
        public ParameterCheckAttribute(int parameterIndex)
        {
            _parameterIndex = parameterIndex;
        }
        public override bool CompileTimeValidate(System.Reflection.MethodBase method)
        {
            if (string.IsNullOrEmpty(this._parameterName) && this._parameterIndex == -1)
            {
                Message.Write(method, SeverityType.Error, ValidationHelper.ParameterNullCheckParameterNotDefined, ValidationMessages.ResourceManager.GetString(ValidationHelper.ParameterNullCheckParameterNotDefined));
                return false;
            }
            var param = method.GetParameters();
            if (param.Length == 0)
            {
                Message.Write(method, SeverityType.Error, ValidationHelper.ParameterNullCheckParameterNotFound, ValidationMessages.ResourceManager.GetString(ValidationHelper.ParameterNullCheckParameterNotFound), method.DeclaringType.Name, method.Name);
                return false;
            }
            //validate if match
            ParameterInfo info = null;
            if (this._parameterIndex >= 0)
            {
                if (!(_parameterIndex < param.Length))
                {
                    Message.Write(method, SeverityType.Error, ValidationHelper.ParameterNullCheckParameterNotFoundIndex, ValidationMessages.ResourceManager.GetString(ValidationHelper.ParameterNullCheckParameterNotFoundIndex), method.DeclaringType.Name, method.Name, _parameterIndex);
                    return false;
                }

                info = param[_parameterIndex];
                _parameterName = info.Name;
            }
            else
            {
                var findParam = param.FirstOrDefault(p => string.Compare(p.Name, _parameterName, StringComparison.OrdinalIgnoreCase) == 0);
                if (findParam == null)
                {
                    Message.Write(method, SeverityType.Error, ValidationHelper.ParameterNullCheckParameterNotFoundName, ValidationHelper.ParameterNullCheckParameterNotFoundName, method.DeclaringType.Name, method.Name, _parameterName);
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

                Message.Write(method, SeverityType.Error, ValidationHelper.ParameterNullCheckParameterValueType, ValidationMessages.ResourceManager.GetString(ValidationHelper.ParameterNullCheckParameterValueType),
                              method.DeclaringType.Name, method.Name, _parameterName);
                return false;
            }
            return true;
        }

    }
}
