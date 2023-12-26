using System;
using System.Collections.Generic;
using System.Reflection;

namespace BattleCity.Extensions
{
    public static class CloneExtension
    {
        /// <summary>
        /// Клонировать объект
        /// </summary>
        public static T Clone<T>(this T obj)
        {
            // если объект унаследован от ICloneable
            if (obj is ICloneable)
            {
                // выполняем клонирование методом, определенным в самом объекте
                return (T)obj.CallClone();
            }

            // выполняем своё клонирование
            return (T)CloneObject(obj);
        }

        /// <summary>
        /// Создать клон объекта
        /// </summary>
        private static object CloneObject(this object obj)
        {
            var newObj = Activator.CreateInstance(obj.GetType(), true);
            foreach (var p in newObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance))
            {
                if (!p.CanWrite) continue;
                var value = p.GetValue(obj);

                if (p.HasMethod("Clone"))
                {
                    value = obj.CallClone();
                    p.SetValue(newObj, value);
                    continue;
                }

                if (p.PropertyType.IsGenericType && value != null)
                {
                    if (p.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Type itemType = p.PropertyType.GetGenericArguments()[0];
                        var t = typeof(List<>).MakeGenericType(itemType);

                        var arrayItems = value.GetType().GetRuntimeMethod("ToArray", new Type[] { }).Invoke(value, null);
                        if (itemType.GetMethod("Clone", BindingFlags.Public | BindingFlags.Instance) != null)
                        {
                            var clonedObj = Activator.CreateInstance(t);
                            IEnumerable<object> collection = (IEnumerable<object>)value;
                            var setter = clonedObj.GetType().GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
                            foreach (var item in collection)
                                setter.Invoke(clonedObj, new object[] { item.CallClone() });

                            value = clonedObj;
                        }
                        else
                        {
                            var clonedObj = Activator.CreateInstance(t, arrayItems);
                            value = clonedObj;
                        }

                    }
                    else
                    {
                        Console.WriteLine("Warning: Generic type is not typeof List<>");
                    }
                }
                else if (p.PropertyType.IsArray && value != null)
                {
                    if (value is Array arr)
                    {
                        var iList = (System.Collections.IList)arr;
                        var clonedObj = (System.Collections.IList)Array.CreateInstance(arr.GetType().GetElementType(), arr.Length);
                        for (int n = 0; n < arr.Length; n++)
                            clonedObj[n] = iList[n];
                        value = clonedObj;
                    }
                }
                else if (value != null && !(p.PropertyType.IsPrimitive || p.PropertyType.IsEnum) && !p.PropertyType.IsValueType && p.PropertyType.Name != null && !p.PropertyType.Namespace.StartsWith("System"))
                {
                    if (value.GetType().GetConstructor(Type.EmptyTypes) != null)
                        value = CloneObject(value);
                }
                p.SetValue(newObj, value);
            }
            return newObj;
        }

        private static bool HasMethod(this object objectToCheck, string methodName)
        {
            var type = objectToCheck.GetType();
            var methodInfo = type.GetMethod(methodName);
            if (methodInfo == null) return false;
            var args = methodInfo.GetGenericArguments();
            return methodInfo.ReturnType == type && args == null || args.Length == 0;
        }

        private static object CallClone(this object obj)
        {
            var type = obj.GetType();
            var methodInfo = type.GetMethod("Clone");
            return methodInfo.Invoke(obj, null);
        }

        #if NET35

        public static object GetValue(this PropertyInfo propertyInfo, object obj)
        {
            return propertyInfo.GetValue(obj, null);
        }

        public static void SetValue(this PropertyInfo propInfo, object obj, object value)
        {
            propInfo.SetValue(obj, value, null);
        }

        public static MethodInfo GetRuntimeMethod(this Type type, string name, Type[] parameters)
        {
            CheckAndThrow(type);
            return type.GetMethod(name, parameters);
        }

        private static void CheckAndThrow(Type t)
        {
            if (t == null) throw new ArgumentNullException("type");
            if (t.GetType().ToString() != "System.RuntimeType") throw new ArgumentException("Argument_MustBeRuntimeType");
        }

        #endif

        //public static T CloneObject<T>(this T obj) where T : ICloneableObject<T>
        //{
        //    var newObj = Activator.CreateInstance(typeof(T));
        //    foreach (var p in newObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance))
        //    {
        //        p.SetValue(newObj, p.GetValue(obj));
        //    }
        //    return (T)newObj;
        //}
    }
}
