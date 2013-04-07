using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostSharp.Aspects.Dependencies;
using System.Diagnostics;
using PostSharp.Aspects;
using System.Threading;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    /// <summary>
    /// Used to syncronize method access.
    /// </summary>
    /// <remarks><para>Method access will only be controlled, 
    /// if there are different threads. Otherwise <see cref="Monitor.Enter(object)"/>
    /// will not block.</para></remarks>
    [Serializable()]
    [ProvideAspectRole(StandardRoles.Threading)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Tracing)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.ExceptionHandling)]
#if(!DEBUG)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public sealed class TypeSynchronizingAttribute : MethodInterceptionAspect
    {
        [NonSerialized()]
        private static readonly Dictionary<string, LockObject> _typeLock = new Dictionary<string, LockObject>();
        private sealed class LockObject
        {

            private readonly Guid _guid;
            public LockObject()
            {
                _guid = Guid.NewGuid();
            }

            public Guid Guid
            {
                get { return _guid; }
            }
        }

        private string instanceType;
        private int _acquireLockTimeout = int.MaxValue;
        /// <summary>
        /// time in ms to aquire the lock, if exceeded
        /// the operation is skipped.
        /// </summary>
        /// <value>The default value is <see cref="int.MaxValue"/></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int AcquireLockTimeout
        {
            get { return _acquireLockTimeout; }
            set { _acquireLockTimeout = value; }
        }

        public override void CompileTimeInitialize(System.Reflection.MethodBase method, PostSharp.Aspects.AspectInfo aspectInfo)
        {
            instanceType = method.ReflectedType.FullName;
        }
        public override void RuntimeInitialize(System.Reflection.MethodBase method)
        {
            lock (_typeLock)
            {
                if (!_typeLock.ContainsKey(instanceType))
                {
                    _typeLock.Add(instanceType, new LockObject());
                    Debug.WriteLine("Sync initialized for type " + instanceType);
                }
            }
        }
        public override void OnInvoke(PostSharp.Aspects.MethodInterceptionArgs args)
        {
            LockObject t = null;
            if (_typeLock.TryGetValue(instanceType, out t))
            {
                if (Monitor.TryEnter(t, _acquireLockTimeout))
                {
                    try
                    {
                        Debug.WriteLine("Entering for " + instanceType + " Lock: " + t.Guid.ToString());
                        args.Proceed();
                    }
                    finally
                    {
                        Debug.WriteLine("Completed blocking" + instanceType + " Lock: " + t.Guid.ToString());
                        Monitor.Exit(t);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Lock object not found for " + instanceType);
            }
        }
    }
}
