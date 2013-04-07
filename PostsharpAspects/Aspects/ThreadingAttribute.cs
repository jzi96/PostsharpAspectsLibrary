using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.ComponentModel;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using System.Threading;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    [Serializable]
    [ProvideAspectRole(StandardRoles.Threading)]
    [AttributeUsageAttribute(AttributeTargets.Assembly | AttributeTargets.Class 
        | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property
        , AllowMultiple = false, Inherited = true)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.TransactionHandling)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Tracing)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Tracing)]
#if(RELEASE)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public sealed class BackgroundThreadingAttribute : MethodInterceptionAspect
    {
        public override void OnInvoke(MethodInterceptionArgs args)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs e) => { args.Proceed(); });
            bw.RunWorkerAsync();
        }
    }
    [Serializable]
    [AttributeUsageAttribute(AttributeTargets.Assembly | AttributeTargets.Class
        | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property
        , AllowMultiple = false, Inherited = true)]
    [ProvideAspectRole(StandardRoles.Threading)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.TransactionHandling)]
    public sealed class SyncronizeWindowThreadAttribute : MethodInterceptionAspect
    {
        //private delegate void InvokeDelegate();
        public override void OnInvoke(MethodInterceptionArgs args)
        {
            System.Windows.Forms.Control main = (System.Windows.Forms.Control)args.Instance;
            if (main.InvokeRequired)
                main.BeginInvoke(new Action(args.Proceed));
            else
                args.Proceed();
        }
    }
    [Serializable]
    [AttributeUsageAttribute(AttributeTargets.Assembly | AttributeTargets.Class
        | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property
        , AllowMultiple = false, Inherited = true)]
    [ProvideAspectRole(StandardRoles.Threading)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.TransactionHandling)]
    public sealed class SyncronizeWpfThreadAttribute : MethodInterceptionAspect
    {
        //private delegate void InvokeDelegate();
        public override void OnInvoke(MethodInterceptionArgs args)
        {
            var dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            if (dispatcher != null)
            {
                if (dispatcher.Thread.ManagedThreadId == System.Threading.Thread.CurrentThread.ManagedThreadId)
                    args.Proceed();
                else
                    dispatcher.BeginInvoke(new Action(args.Proceed));
            }
            else
            {
                args.Proceed();
            }
        }
    }
    [Serializable]
    [AttributeUsageAttribute(AttributeTargets.Assembly | AttributeTargets.Class
        | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property
        , AllowMultiple = false, Inherited = true)]
    [ProvideAspectRole(StandardRoles.Threading)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.TransactionHandling)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Tracing)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.ExceptionHandling)]
    public sealed class TypeSyncronizeAttribute : OnMethodBoundaryAspect
    {
        [NonSerialized]
        private static readonly Dictionary<string, object> _lockDictionary = new Dictionary<string, object>();
        private string _instanceType;
        [NonSerialized]
        private object _synchronizer;

        public int AquireTimeout { get; set; }
        public TypeSyncronizeAttribute()
            : this(Int32.MaxValue)
        { }

        public TypeSyncronizeAttribute(int aquireTimeout)
        {
            this.AquireTimeout = aquireTimeout;
        }
        public override void CompileTimeInitialize(System.Reflection.MethodBase method, AspectInfo aspectInfo)
        {
            _instanceType = method.ReflectedType.FullName;
        }
        public override void RuntimeInitialize(System.Reflection.MethodBase method)
        {
            if (!_lockDictionary.TryGetValue(_instanceType, out _synchronizer))
            {
                lock (_lockDictionary)
                {
                    if (!_lockDictionary.TryGetValue(_instanceType, out _synchronizer))
                    {
                        _synchronizer = new object();
                        _lockDictionary.Add(_instanceType, _synchronizer);
                    }
                }
            }
        }
        public override void OnEntry(MethodExecutionArgs args)
        {
            if (!Monitor.TryEnter(_synchronizer, AquireTimeout))
            {
                Debug.WriteLine("Lock for " + _instanceType + " could not be aquired!");
                args.FlowBehavior = FlowBehavior.Return;
            }
        }
        public override void OnExit(MethodExecutionArgs args)
        {
            Monitor.Exit(_synchronizer);
        }
    }

}
