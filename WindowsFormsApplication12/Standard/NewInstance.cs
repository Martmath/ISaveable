using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SaveableClass
{
   public class NewInstance
    {
        public static Type WithoutRef(Type T)
        {
            if (T.Name[T.Name.Length - 1] == '&') T = Type.GetType(T.FullName.Substring(0, T.FullName.Length - 1));
            return T;
        }

        public static object GetInstance(Type T)
        {
            object Result = null;
            if (T == null) return null;
            T = WithoutRef(T);

            if (T.IsValueType)
            {
                Result = Activator.CreateInstance(T);
            }
            else if (T.Name == "String")
                Result = (object)"";
            else
            {
                ConstructorInfo C = T.GetConstructor
                (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (C != null) Result = C.Invoke(null);
                else
                {
                    ConstructorInfo[] CC = T.GetConstructors(); int i = 0;
                    while ((i < CC.Length) && (!ItGoodConstructor(CC[i]))) { i = i + 1; }
                    if (i < CC.Length) Result = ObjectFromConstructor(CC[i]);

                    if ((Result == null) && (CC.Length > 0))
                    {
                        ParameterInfo[] ps = CC[0].GetParameters();
                        List<object> o = new List<object>();
                        object O = null;
                        for (i = 0; i < ps.Length; i++)
                        {
                            //  if ((ps[i].DefaultValue != null)&&(!ps[i].ParameterType.IsByRef)) O = ps[i].DefaultValue; else //some time not work(((
                            {
                                O = GetInstance(ps[i].ParameterType);
                                if (O == null) break;
                            }
                            o.Add(O);
                        }
                        if (o.Count == ps.Length) Result = CC[0].Invoke(o.ToArray());
                    }
                }
            }
            return Result;
        }
        public static bool ItGoodConstructor(ConstructorInfo C)
        {
            ParameterInfo[] ps = C.GetParameters();
            var cs = from p in ps
                     let prms = p.ParameterType.GetConstructor
                     (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null)
                     let pms = p.ParameterType.IsValueType
                     where ((prms != null) || pms)
                     select p;
            return (cs.Count() == ps.Length);
        }
        public static object ObjectFromConstructor(ConstructorInfo C)
        {
            ParameterInfo[] ps = C.GetParameters();
            List<object> o = new List<object>();
            for (int i = 0; i < ps.Length; i++)
            {
                // if (ps[i].DefaultValue != null) o.Add(ps[i].DefaultValue); else 
                if (ps[i].ParameterType.IsValueType) o.Add(Activator.CreateInstance(ps[i].ParameterType));
                else
                    o.Add(ps[i].ParameterType.GetConstructor
                         (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null).Invoke(null));
            }
            return C.Invoke(o.ToArray());
        }

    }
}
