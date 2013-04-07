using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System.Diagnostics;
using System.Threading;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    /// <summary>
    /// Attribute for implementing Performance Coutner and running them.
    /// </summary>
    [Serializable]
    [ProvideAspectRole(StandardRoles.PerformanceInstrumentation)]
    [AttributeUsageAttribute(AttributeTargets.Method | AttributeTargets.Constructor,
        AllowMultiple = true, Inherited = true)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Threading)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.DataBinding)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.ExceptionHandling)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.TransactionHandling)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Persistence)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.EventBroker)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Tracing)]
    [PostSharp.Aspects.Dependencies.ProvideAspectRole(PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.PerformanceInstrumentation)]
    [AspectRoleDependency(AspectDependencyAction.Commute, PostSharp.Aspects.Dependencies.StandardRoles.Tracing)]
    [StringIntern()]
#if(!DEBUG)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public abstract class PerformanceCounterBaseAttribute : MethodInterceptionAspect
    {
        /// <summary>
        /// a special instance performance counter
        /// to increment always (a summary of all instances)
        /// </summary>
        protected const string OverallInstance = "_Overall";
        private readonly string _categoryName;
        private readonly string _counterName;

        private static readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private static readonly IDictionary<string, IDictionary<string, PerformanceCounter>>
        categoryDict = new Dictionary<string, IDictionary<string, PerformanceCounter>>();
        private static readonly IList<string> _disabledCounters = new List<string>();

        private static readonly object Sync = new object();
        /// <summary>
        /// Type of the Performance counter
        /// </summary>
        protected readonly PerformanceCounterType _counterType;

        /// <summary>
        /// properties for Performance Counter instance
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="counterName"></param>
        /// <param name="counterType"></param>
        public PerformanceCounterBaseAttribute(string categoryName, string counterName, PerformanceCounterType counterType)
        {
            _counterType = counterType;
            _counterName = string.Intern(counterName);
            _categoryName = string.Intern(categoryName);
        }
        /// <summary>
        /// Returns the performance counter matching the
        /// specification of <see cref="CategoryName"/>
        /// and <see cref="CounterName"/>
        /// </summary>
        /// <returns>If not found returns <see langword="null"/>;otherwise the counter instance</returns>
        protected PerformanceCounter GetCounter()
        {
            return GetCounter(CategoryName, CounterName, _counterType);
        }

        /// <summary>
        /// Gets the counter.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="counterName">Name of the counter.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private PerformanceCounter GetCounter(string category,
                                              string counterName,
                                              PerformanceCounterType type)
        {
            if (string.IsNullOrEmpty(category)
               || string.IsNullOrEmpty(counterName))
                return null;
            try
            {
                PerformanceCounter pc = this.EnsureWithCategory(category, counterName, type, false);
                if (pc == null)
                    return null;
                pc.InstanceName = AppDomain.CurrentDomain.FriendlyName;
                return pc;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Get counter failed! " + ex, "EnterpriseLoggingService.GetCounter");
            }

            return null;
        }
        /// <summary>
        /// Ensures the with category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="counterName">Name of the counter.</param>
        /// <param name="type">The type.</param>
        /// <param name="deep">if set to <see langword="true"/> [deep].</param>
        /// <returns></returns>
        private PerformanceCounter EnsureWithCategory(string category,
                                                      string counterName,
                                                      PerformanceCounterType type,
                                                      bool deep)
        {
            cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (categoryDict.ContainsKey(category))
                {
                    IDictionary<string, PerformanceCounter> perf = categoryDict[category];
                    return EnsureCounter(category, counterName, type, perf);
                }
                else
                {
                    if (deep)
                        return null;
                    return EnsureCounter(category, counterName, type, null);
                }
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Ensures the counter.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="counterName">Name of the counter.</param>
        /// <param name="type">The type.</param>
        /// <param name="perf">The perf.</param>
        /// <returns></returns>
        private static PerformanceCounter EnsureCounter(string category,
                                                        string counterName,
                                                        PerformanceCounterType type,
                                                        IDictionary<string, PerformanceCounter> perf)
        {
            PerformanceCounter counter = null;
            string counterkey = category + "||" + counterName;
            if (_disabledCounters.Contains(counterkey))
            {
                Trace.WriteLine(string.Format("The counter {0} in category {1} could not be accessed and therefore disabled", counterName, category), typeof(PerformanceCounterBaseAttribute).FullName);
                return null;
            }
            if (perf != null
               && perf.ContainsKey(counterName))
                counter = perf[counterName];
            else
            {
                cacheLock.EnterWriteLock();
                try
                {
                    Dictionary<string, PerformanceCounter> counterDict;
                    var creation = new CounterCreationDataCollection();
                    var counterKeys = new List<string>();
                    if (PerformanceCounterCategory.Exists(category))
                    {
                        var pc = new PerformanceCounterCategory(category);
                        PerformanceCounter[] counters;
                        if (pc.CategoryType
                           == PerformanceCounterCategoryType.MultiInstance)
                        {
                            string[] instances = pc.GetInstanceNames();
                            if (instances.Length > 0)
                            {
                                foreach (string instance in instances)
                                {
                                    counters = pc.GetCounters(instance);
                                    LoopCounterCollection(creation, counters, counterKeys);
                                }
                            }
                            else
                            {
                                counters = pc.GetCounters();
                                LoopCounterCollection(creation, counters, counterKeys);
                            }
                        }
                        else
                        {
                            counters = pc.GetCounters();
                            LoopCounterCollection(creation, counters, counterKeys);
                        }

                        if (perf != null)
                        {
                            foreach (PerformanceCounter p in perf.Values)
                            {
                                p.Dispose();
                            }
                            perf.Clear();
                            perf = null;
                        }
                        //PerformanceCounterCategory.Delete(category);
                    }
                    else
                    {
                        CheckAndAddCounter(creation, counterKeys, counterName, type);
                        if (type == PerformanceCounterType.AverageTimer32)
                            creation.Add(new CounterCreationData(counterName + "Base",
                                                                 string.Empty,
                                                                 PerformanceCounterType.AverageBase));
                        PerformanceCounterCategory.Create(category,
                                                          string.Empty,
                                                          PerformanceCounterCategoryType.MultiInstance,
                                                          creation);
                    }
                    counterDict = new Dictionary<string, PerformanceCounter>(creation.Count);
                    foreach (CounterCreationData ccd in creation)
                    {
                        counter = new PerformanceCounter(category,
                                                         ccd.CounterName,
                                                         AppDomain.CurrentDomain.FriendlyName,
                                                         false);
                        counterDict.Add(ccd.CounterName, counter);
                    }
                    if (counterDict.TryGetValue(counterName, out counter))
                    {
                        counter = counterDict[counterName];
                        perf = counterDict;
                        if (categoryDict.ContainsKey(category))
                            categoryDict[category] = counterDict;
                        else
                            categoryDict.Add(category, counterDict);
                    }
                    else
                    {
                        Trace.WriteLine("Counter does not exists in group. Counter will be disabled", typeof(PerformanceCounterBaseAttribute).FullName);
                        lock (_disabledCounters)
                        {
                            if (!_disabledCounters.Contains(counterkey))
                                _disabledCounters.Add(counterkey);
                        }
                        counter = null;
                    }
                }
                catch (System.Security.SecurityException sex)
                {
                    Trace.WriteLine("Failed to create counter -> counter will be deactivated!" + sex, typeof(PerformanceCounterBaseAttribute).FullName);
                    lock (_disabledCounters)
                    {
                        if (!_disabledCounters.Contains(counterkey))
                            _disabledCounters.Add(counterkey);
                    }
                    counter = null;
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
            }
            return counter;
        }
        /// <summary>
        /// Checks the and add counter.
        /// </summary>
        /// <param name="creation">The creation.</param>
        /// <param name="counterKeys">The counter keys.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        private static void CheckAndAddCounter(CounterCreationDataCollection creation,
                                               List<string> counterKeys,
                                               string name,
                                               PerformanceCounterType type)
        {
            if (!counterKeys.Contains(name))
            {
                counterKeys.Add(name);
                creation.Add(new CounterCreationData(name, string.Empty, type));
            }
        }
        /// <summary>
        /// Loops the counter collection.
        /// </summary>
        /// <param name="creation">The creation.</param>
        /// <param name="counters">The counters.</param>
        /// <param name="counterKeys">The counter keys.</param>
        private static void LoopCounterCollection(CounterCreationDataCollection creation,
                                                  PerformanceCounter[] counters,
                                                  List<string> counterKeys)
        {
            foreach (PerformanceCounter c in counters)
            {
                CheckAndAddCounter(creation, counterKeys, c.CounterName, c.CounterType);
            }
        }
        /// <summary>
        /// Category name for the counter (group)
        /// </summary>
        public string CategoryName
        {
            get
            {
                return _categoryName;
            }
        }
        /// <summary>
        /// Name of the perfomance counter 
        /// </summary>
        public string CounterName
        {
            get
            {
                return _counterName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        public override void RuntimeInitialize(System.Reflection.MethodBase method)
        {
            //Try loade the counter

            //type of counter checks ...


            //when fail, disable for comple processing
            base.RuntimeInitialize(method);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public override bool CompileTimeValidate(System.Reflection.MethodBase method)
        {
            if (string.IsNullOrEmpty(CategoryName))
            {
                PostSharp.Extensibility.Message.Write(method, PostSharp.Extensibility.SeverityType.Error, ValidationHelper.PerformanceCounterAttributeCategoryNameMissing, ValidationMessages.ResourceManager.GetString(ValidationHelper.PerformanceCounterAttributeCategoryNameMissing));
            }
            if (string.IsNullOrEmpty(CategoryName))
            {
                PostSharp.Extensibility.Message.Write(method, PostSharp.Extensibility.SeverityType.Error, ValidationHelper.PerformanceCounterAttributeCounterNameMissing, ValidationMessages.ResourceManager.GetString(ValidationHelper.PerformanceCounterAttributeCounterNameMissing));
            }
            return base.CompileTimeValidate(method);
        }
    }
}
