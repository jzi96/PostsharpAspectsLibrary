using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Caching;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Aspects.Dependencies;
using Zieschang.Net.Projects.PostsharpAspects.Utilities;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    /// <summary>
    /// </summary>
    /// <remarks><para>PostSharp Sample copy</para></remarks>
    [Serializable]
    [ProvideAspectRole(StandardRoles.Caching)]
    [AttributeUsageAttribute(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly,
         AllowMultiple = true, Inherited = true)]
#if(RELEASE)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public class CachingAttribute : OnMethodBoundaryAspect
    {
        // Some formatting strings to compose the cache key.
        private MethodFormatStrings _formatStrings;

        // A dictionary that serves as a trivial cache implementation.
        private static readonly System.Web.Caching.Cache Cache = new System.Web.Caching.Cache();
        private readonly DateTime _absolutExpiration;
        private readonly TimeSpan _slidingExpiration;

        public TimeSpan SlidingExpiration
        {
            get
            {
                return _slidingExpiration;
            }
        }
        public DateTime AbsolutExpiration
        {
            get
            {
                return _absolutExpiration;
            }
        }
        public CachingAttribute()
            : this(System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration)
        { }
        public CachingAttribute(DateTime absolutExpiration)
            : this(absolutExpiration, System.Web.Caching.Cache.NoSlidingExpiration)
        { }

        public CachingAttribute(TimeSpan slidingExpiration)
            : this(System.Web.Caching.Cache.NoAbsoluteExpiration, slidingExpiration)
        { }

        private CachingAttribute(DateTime absolutExpiration, TimeSpan slidingExpiration)
        {
            _absolutExpiration = absolutExpiration;
            _slidingExpiration = slidingExpiration;
        }
        // Validate the attribute usage.
        public override bool CompileTimeValidate(MethodBase method)
        {
            // Don't apply to constructors.
            if (method is ConstructorInfo)
            {
                Message.Write(method, SeverityType.Error, "CX0001", "Cannot cache constructors.");
                return false;
            }

            var methodInfo = (MethodInfo)method;

            // Don't apply to void methods.
            if (methodInfo.ReturnType.Name == "Void")
            {
                Message.Write(method, SeverityType.Error, "CX0002", "Cannot cache void methods.");
                return false;
            }

            // Does not support out parameters.
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Any(t => t.IsOut))
            {
                Message.Write(method, SeverityType.Error, "CX0003", "Cannot cache methods with return values.");
                return false;
            }

            return true;
        }


        // At compile time, initialize the format string that will be
        // used to create the cache keys.
        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            _formatStrings = Formatter.GetMethodFormatStrings(method);
        }

        // Executed at runtime, before the method.
        public override void OnEntry(MethodExecutionArgs eventArgs)
        {
            // Compose the cache key.
            string key = _formatStrings.Format(
                eventArgs.Instance, eventArgs.Method, eventArgs.Arguments.ToArray());

            // Test whether the cache contains the current method call.
            var value = Cache.Get(key);
            if (ReferenceEquals(value, null))
            {
                lock (Cache)
                {
                    value = Cache.Get(key);
                    if (ReferenceEquals(value, null))
                    {
                        // If not, we will continue the execution as normally.
                        // We store the key in a state variable to have it in the OnExit method.
                        eventArgs.MethodExecutionTag = key;
                    }
                    else
                    {
                        // If it is in cache, we set the cached value as the return value
                        // and we force the method to return immediately.
                        eventArgs.ReturnValue = value;
                        eventArgs.FlowBehavior = FlowBehavior.Return;
                    }
                }
            }
        }

        // Executed at runtime, after the method.
        public override void OnSuccess(MethodExecutionArgs eventArgs)
        {
            // Retrieve the key that has been computed in OnEntry.
            var key = (string)eventArgs.MethodExecutionTag;

            // Put the return value in the cache.
            lock (Cache)
            {
                Cache.Add(key, eventArgs.ReturnValue, null, _absolutExpiration, _slidingExpiration,
                          CacheItemPriority.Normal, null);
            }
        }
    }

    // This aspect provides no advise at all, but provides a dependency: it cannot
    // be found together on the same method with a caching aspect.
}
