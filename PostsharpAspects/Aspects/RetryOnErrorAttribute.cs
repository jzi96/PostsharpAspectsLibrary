using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostSharp.Aspects.Dependencies;
using System.Reflection;
using PostSharp.Aspects;
using Zieschang.Net.Projects.PostsharpAspects.Utilities;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    [Serializable]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(HandleExceptionAttribute))]
    [ProvideAspectRole(StandardRoles.ExceptionHandling)]
    [AttributeUsageAttribute(AttributeTargets.Method | AttributeTargets.Assembly  | AttributeTargets.Class |AttributeTargets.Property,
        AllowMultiple = true, Inherited = true)]
#if(RELEASE)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public sealed class RetryOnErrorAttribute : PostSharp.Aspects.MethodInterceptionAspect
    {
        public RetryOnErrorAttribute(int reties)
            : this(reties, null, true)
        {
        }
        public RetryOnErrorAttribute(int reties, bool raiseException)
            : this(reties, null, raiseException)
        {
        }
        public RetryOnErrorAttribute(int reties, string message)
            : this(reties, message, true)
        {
        }
        public RetryOnErrorAttribute(int reties, string message, bool raiseException)
        {
            MaxRetries = reties;
            _raiseAfterRetries = raiseException;
            _message = message;
        }
        public int RetryDelay { get; set; }
        public Type ExceptionFilterType { get; set; }
        private string _message;
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }
        private bool _raiseAfterRetries;
        public bool RaiseAfterRetries
        {
            get
            {
                return _raiseAfterRetries;
            }
            set
            {
                _raiseAfterRetries = value;
            }
        }
        private int _maxRetries;
        public int MaxRetries
        {
            get { return _maxRetries; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value", "Value must be greater 0");
                }
                _maxRetries = value;
            }
        }
        private string _failureMessage;
        private string _retryMessage;
        private log4net.ILog _staticInstanceField;

        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            _failureMessage = "Failed after {0} retries in " + method.DeclaringType.Name + "." + method.Name + "!";
            _retryMessage = "Failed #{0} retries in " + method.DeclaringType.Name + "." + method.Name + "!";

        }

        public override void RuntimeInitialize(System.Reflection.MethodBase method)
        {
            base.RuntimeInitialize(method);
            _staticInstanceField = InternalFieldFinder.Instance.GetInstance<log4net.ILog>(method, null);
        }
        public override void OnInvoke(PostSharp.Aspects.MethodInterceptionArgs args)
        {
            int counter = 0;
            object instance = args.Instance;
            log4net.ILog lg = null;
            if (_staticInstanceField == null)
            {
                lg = InternalFieldFinder.Instance.GetInstance<log4net.ILog>(args.Method, instance);
            }
            else
            {
                lg = _staticInstanceField;
            }
            do
            {
                //delay on each cycle before continue
                //in future use a strategy 
                if (counter > 0 && RetryDelay > 0)
                    System.Threading.Thread.Sleep(RetryDelay);
                counter++;
                try
                {
                    args.Proceed();
                    counter = Int32.MaxValue;
                }
                catch (Exception ex)
                {
                    if (ExceptionFilterType == null || ExceptionFilterType.IsAssignableFrom(ex.GetType()))
                    {
                        if (counter < _maxRetries)
                        {
                            lg.Info(string.Format(System.Globalization.CultureInfo.InvariantCulture, _retryMessage, counter), ex);
                        }
                        else
                        {
                            string msg = string.Format(System.Globalization.CultureInfo.InvariantCulture, _failureMessage, counter);
                            if (!string.IsNullOrEmpty(_message))
                                msg = Message + " " + msg;
                            lg.Error(msg, ex);
                            if (_raiseAfterRetries)
                                throw;
                        }
                    }
                }
            } while (counter < _maxRetries);
        }

    }
}
