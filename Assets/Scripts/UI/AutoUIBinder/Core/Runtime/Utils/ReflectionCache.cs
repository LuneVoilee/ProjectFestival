using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace AutoUI
{
    /// <summary>
    /// 反射缓存系统 - 优化反射性能，避免重复计算
    /// </summary>
    public static class ReflectionCache
    {
        #region 缓存字典

        // 类型的方法缓存
        private static readonly Dictionary<Type, MethodInfo[]> _methodCache = new Dictionary<Type, MethodInfo[]>();
        
        // 类型的字段缓存
        private static readonly Dictionary<Type, FieldInfo[]> _fieldCache = new Dictionary<Type, FieldInfo[]>();
        
        // UnityEvent字段缓存
        private static readonly Dictionary<Type, FieldInfo[]> _unityEventFieldCache = new Dictionary<Type, FieldInfo[]>();
        
        // 类型友好名称缓存
        private static readonly Dictionary<Type, string> _friendlyNameCache = new Dictionary<Type, string>();
        
        // 方法参数类型缓存
        private static readonly Dictionary<MethodInfo, Type[]> _methodParameterCache = new Dictionary<MethodInfo, Type[]>();

        #endregion

        #region 公共API

        /// <summary>
        /// 获取类型的所有方法（带缓存）
        /// </summary>
        public static MethodInfo[] GetMethods(Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            if (!_methodCache.TryGetValue(type, out var methods))
            {
                methods = type.GetMethods(flags);
                _methodCache[type] = methods;
            }
            return methods;
        }

        /// <summary>
        /// 获取类型的所有字段（带缓存）
        /// </summary>
        public static FieldInfo[] GetFields(Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (!_fieldCache.TryGetValue(type, out var fields))
            {
                fields = type.GetFields(flags);
                _fieldCache[type] = fields;
            }
            return fields;
        }


        /// <summary>
        /// 获取类型的UnityEvent字段（带缓存）
        /// </summary>
        public static FieldInfo[] GetUnityEventFields(Type type)
        {
            if (!_unityEventFieldCache.TryGetValue(type, out var eventFields))
            {
                var allFields = GetFields(type);
                var eventFieldList = new List<FieldInfo>();
                
                foreach (var field in allFields)
                {
                    if (typeof(UnityEventBase).IsAssignableFrom(field.FieldType))
                    {
                        // 只缓存可序列化的字段
                        bool isSerializable = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;
                        if (isSerializable)
                        {
                            eventFieldList.Add(field);
                        }
                    }
                }
                
                eventFields = eventFieldList.ToArray();
                _unityEventFieldCache[type] = eventFields;
            }
            return eventFields;
        }

        /// <summary>
        /// 获取类型的友好名称（带缓存）
        /// </summary>
        public static string GetFriendlyTypeName(Type type)
        {
            if (type == null) return "void";
            
            if (!_friendlyNameCache.TryGetValue(type, out var friendlyName))
            {
                // 使用简化的类型名称获取逻辑，避免CodeDom依赖
                friendlyName = GetSimplifiedTypeName(type);
                _friendlyNameCache[type] = friendlyName;
            }
            return friendlyName;
        }

        /// <summary>
        /// 获取简化的类型名称
        /// </summary>
        private static string GetSimplifiedTypeName(Type type)
        {
            if (type == null) return "void";

            // 处理泛型类型
            if (type.IsGenericType)
            {
                string typeName = type.Name;
                int backtickIndex = typeName.IndexOf('`');
                if (backtickIndex > 0)
                {
                    typeName = typeName.Substring(0, backtickIndex);
                }

                var genericArgs = type.GetGenericArguments();
                if (genericArgs.Length > 0)
                {
                    var argNames = new string[genericArgs.Length];
                    for (int i = 0; i < genericArgs.Length; i++)
                    {
                        argNames[i] = GetSimplifiedTypeName(genericArgs[i]);
                    }
                    return $"{typeName}<{string.Join(", ", argNames)}>";
                }
                return typeName;
            }

            // 处理数组类型
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var rank = type.GetArrayRank();
                var brackets = rank == 1 ? "[]" : $"[{new string(',', rank - 1)}]";
                return GetSimplifiedTypeName(elementType) + brackets;
            }

            // 处理常见的简化名称
            switch (type.FullName)
            {
                case "System.String": return "string";
                case "System.Int32": return "int";
                case "System.Boolean": return "bool";
                case "System.Single": return "float";
                case "System.Double": return "double";
                case "System.Void": return "void";
                case "System.Object": return "object";
                default:
                    // 移除命名空间，只保留类型名
                    string fullName = type.Name;
                    int lastDot = fullName.LastIndexOf('.');
                    return lastDot >= 0 ? fullName.Substring(lastDot + 1) : fullName;
            }
        }

        /// <summary>
        /// 获取方法的参数类型（带缓存）
        /// </summary>
        public static Type[] GetMethodParameterTypes(MethodInfo method)
        {
            if (!_methodParameterCache.TryGetValue(method, out var paramTypes))
            {
                var parameters = method.GetParameters();
                paramTypes = new Type[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    paramTypes[i] = parameters[i].ParameterType;
                }
                _methodParameterCache[method] = paramTypes;
            }
            return paramTypes;
        }

        /// <summary>
        /// 检查方法是否有特定特性（带缓存）
        /// </summary>
        public static bool HasAttribute<T>(MethodInfo method) where T : Attribute
        {
            return method.GetCustomAttribute<T>() != null;
        }


        #endregion

        #region 缓存管理

        /// <summary>
        /// 清理所有缓存
        /// </summary>
        public static void ClearAllCaches()
        {
            _methodCache.Clear();
            _fieldCache.Clear();
            _unityEventFieldCache.Clear();
            _friendlyNameCache.Clear();
            _methodParameterCache.Clear();
            
            Debug.Log("[AutoUIBinder] 反射缓存已清理");
        }

        /// <summary>
        /// 清理特定类型的缓存
        /// </summary>
        public static void ClearTypeCache(Type type)
        {
            _methodCache.Remove(type);
            _fieldCache.Remove(type);
            _unityEventFieldCache.Remove(type);
            _friendlyNameCache.Remove(type);
            
            // 清理方法参数缓存（需要遍历）
            var keysToRemove = new List<MethodInfo>();
            foreach (var kvp in _methodParameterCache)
            {
                if (kvp.Key.DeclaringType == type)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            foreach (var key in keysToRemove)
            {
                _methodParameterCache.Remove(key);
            }
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        public static string GetCacheStats()
        {
            return $"方法缓存: {_methodCache.Count}, " +
                   $"字段缓存: {_fieldCache.Count}, " +
                   $"事件字段缓存: {_unityEventFieldCache.Count}, " +
                   $"类型名称缓存: {_friendlyNameCache.Count}, " +
                   $"方法参数缓存: {_methodParameterCache.Count}";
        }

        #endregion

        #region Unity生命周期处理

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            // 在编辑器中，当代码重新编译时清理缓存
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ClearAllCaches;
        }
#endif

        #endregion
    }
}