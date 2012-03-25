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
    public abstract class PerformanceCounterBaseAspectAttribute : MethodInterceptionAspect
    {
        protected const string OverallInstance = "_Overall";

        private static readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private static readonly IDictionary<string, IDictionary<string, PerformanceCounter>>
            categoryDict;

        private static readonly object Sync = new object();
        private bool _runtimeInitError;
        protected PerformanceCounterType _counterType;
        protected PerformanceCounter GetCounter()
        {
            if (_runtimeInitError)
                return null;
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
            PerformanceCounter counter;
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
                        PerformanceCounterCategory.Delete(category);
                    }
                    CheckAndAddCounter(creation, counterKeys, counterName, type);
                    if (type == PerformanceCounterType.AverageTimer32)
                        creation.Add(new CounterCreationData(counterName + "Base",
                                                             "",
                                                             PerformanceCounterType.AverageBase));
                    PerformanceCounterCategory.Create(category,
                                                      "",
                                                      PerformanceCounterCategoryType.MultiInstance,
                                                      creation);

                    counterDict = new Dictionary<string, PerformanceCounter>(creation.Count);
                    foreach (CounterCreationData ccd in creation)
                    {
                        counter = new PerformanceCounter(category,
                                                         ccd.CounterName,
                                                         AppDomain.CurrentDomain.FriendlyName,
                                                         false);
                        counterDict.Add(ccd.CounterName, counter);
                    }
                    counter = counterDict[counterName];
                    perf = counterDict;
                    if (categoryDict.ContainsKey(category))
                        categoryDict[category] = counterDict;
                    else
                        categoryDict.Add(category, counterDict);
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
                creation.Add(new CounterCreationData(name, "", type));
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
        public string CategoryName { get; set; }
        public string CounterName { get; set; }


        public override void RuntimeInitialize(System.Reflection.MethodBase method)
        {
            //Try loade the counter

            //type of counter checks ...


            //when fail, disable for comple processing
            base.RuntimeInitialize(method);
        }

        public override bool CompileTimeValidate(System.Reflection.MethodBase method)
        {
            if (string.IsNullOrEmpty(CategoryName))
            {
                PostSharp.Extensibility.Message.Write(PostSharp.Extensibility.SeverityType.Error, ValidationHelper.PerformanceCounterAttributeCategoryNameMissing, ValidationMessages.ResourceManager.GetString(ValidationHelper.PerformanceCounterAttributeCategoryNameMissing));
            }
            if (string.IsNullOrEmpty(CategoryName))
            {
                PostSharp.Extensibility.Message.Write(PostSharp.Extensibility.SeverityType.Error, ValidationHelper.PerformanceCounterAttributeCounterNameMissing, ValidationMessages.ResourceManager.GetString(ValidationHelper.PerformanceCounterAttributeCounterNameMissing));
            }
            return base.CompileTimeValidate(method);
        }
    }
}
