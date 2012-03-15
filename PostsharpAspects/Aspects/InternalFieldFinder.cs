using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    [ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.MemberAccess, RestrictedMemberAccess = true)]
    public sealed class InternalFieldFinder
    {
        private static readonly InternalFieldFinder _instance = new InternalFieldFinder();
        private readonly Dictionary<string, object> _resultCache = new Dictionary<string, object>();
        private readonly Dictionary<string, FieldInfo> _fieldcache = new Dictionary<string, FieldInfo>();

        public static InternalFieldFinder Instance
        {
            get { return _instance; }
        }
        public void Reset()
        {
            lock (_instance)
            {
                _resultCache.Clear();
                _fieldcache.Clear();
            }
        }

        public T GetInstance<T>(MethodBase method, object instance)
        {
            if (ReferenceEquals(method, null)) throw new ArgumentNullException("method");
            // cache based on type
            object result = null;
            Type reflectedType = method.ReflectedType;
            string typeName = reflectedType.FullName + "|" + typeof(T).FullName;

            FieldInfo f;
            if (!_fieldcache.TryGetValue(typeName, out f))
            {
                lock (_instance)
                {
                    if (!_fieldcache.TryGetValue(typeName, out f))
                    {
                        var fields = reflectedType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                        for (int i = 0; i < fields.Length; i++)
                        {
                            f = fields[i];
                            if (f.FieldType.IsAssignableFrom(typeof(T)))
                            {
                                //we have found the right field
                                _fieldcache.Add(typeName, f);
                                break;
                            }
                            f = null;
                        }
                    }
                    if (f != null)
                        result = TryGetFieldValue(typeName, f, instance);
                }
            }
            else
            {
                result = TryGetFieldValueWithLock(typeName, f, instance);
            }
            return (T)result;
        }

        private object TryGetFieldValueWithLock(string typeName, FieldInfo fieldInfo, object instance)
        {
            lock (_instance)
            {
                return TryGetFieldValue(typeName, fieldInfo, instance);
            }
        }

        private object TryGetFieldValue(string typeName, FieldInfo fieldInfo, object instance)
        {
            object result = null;
            if (fieldInfo.IsStatic)
            {
                if (!_resultCache.TryGetValue(typeName, out result))
                {
                    result = fieldInfo.GetValue(null);
                    if (result != null)
                        _resultCache.Add(typeName, result);
                }
            }
            else
            {
                if (instance != null)
                {
                    object tmp;
                    WeakReference reference =null;
                    if (_resultCache.TryGetValue(typeName, out tmp))
                    {
                        var specific = (Dictionary<WeakReference, object>)tmp;
                        foreach (var o in specific)
                        {
                            if(ReferenceEquals( o.Key.Target, instance))
                            {
                                result = o.Value;
                                reference = o.Key;
                                break;
                            }
                        }
                        if(reference==null)
                        {
                            reference = new WeakReference(instance);
                            result = GetInstanceResult(fieldInfo, typeName, instance, reference, specific);
                        }
                        else
                        {
                            if (!reference.IsAlive)
                            {
                                specific.Remove(reference);
                                result = GetInstanceResult(fieldInfo, typeName, instance, reference, specific);

                            }
                        }
                    }
                    else
                    {
                        reference = new WeakReference(instance);
                        var specific = new Dictionary<WeakReference, object>();
                        result = GetInstanceResult(fieldInfo, typeName, instance, reference, specific);
                    }
                }
            }
            return result;
        }

        private object GetInstanceResult(FieldInfo fieldInfo, string typeName, object instance, WeakReference reference, Dictionary<WeakReference, object> specific)
        {
            object result;
            result = fieldInfo.GetValue(instance);
            if (result != null)
            {
                specific.Add(reference, result);
                if (!_resultCache.ContainsKey(typeName)) _resultCache.Add(typeName, specific);
            }
            return result;
        }

    }
}
