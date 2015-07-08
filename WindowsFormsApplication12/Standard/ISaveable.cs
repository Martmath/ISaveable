using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using Debug_;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
namespace SaveableClass
{
    public interface ISaveable
    {
        /*//Insert in class realize interface
         private ExtISaveable.TempSave SCache;         
         public ExtISaveable.TempSave GetCache()
         {
             if (SCache == null) SCache = new ExtISaveable.TempSave(this);
             return SCache;
         }*/
        ExtISaveable.TempSave GetCache();          
    }
    
    public static class ExtISaveable
    {

        public static class StatD
        {
            public const string DM1 = ":";
            public const string DM2 = "#";
            public const string DM3 = ".";
            public const string DM22 = "##";
            public const string DM23 = "###";
            public const string FI = "FirstIteration";
            public const int MaxU = Int32.MaxValue;// 4294967295;

            public enum Politic
            {
                SkipAllIfCanntSet,
                DontChange,
                GenerateError
            }
            public enum GeneralType
            {
                Dictionary,
                Array,
                List,
                DataTable,
                DataSet,
                Tuple,
                AnonymousType,
                Standard,
                NotSerializable
            }
            public enum WhatMustDo
            {
                IgnoreDifferentType,
                TryCreateNewifDifferentType
            }
        }
        public class P_O
        {
            public PropertyInfo P;
            public object O;
            public object Parent;
            public P_O(object parent,PropertyInfo p,object o) { P = p; O = o; Parent = parent; }
            public P_O() { P = null; O = null; Parent = null; }
        }
        public class I_O
        {
            public int  NextN;
            public object O;
            public I_O(int NN, object o) { NextN = NN; O = o; }
            public I_O(){}
            
        }     
        public static Type GType(this Type T,string S)
        {
            return Type.GetType(S)??AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(S))
                       .Where(type => type != null).FirstOrDefault();
        }
        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }     
        private static class Functions
        {
            //public delegate bool selector(PropertyInfo, object);            
            public static Dictionary<StatD.GeneralType, Func<PropertyInfo, object, bool>> GTSelection;
            public static Dictionary<StatD.GeneralType, Action<ISaveable, object, PropertyInfo, object, string, char, int>> GTGetActions;
            public static Dictionary<Func<PropertyInfo, object, bool>, Action<ISaveable, object, PropertyInfo, object, string, char, int>> GTGetCombine = new Dictionary<Func<PropertyInfo, object, bool>, Action<ISaveable, object, PropertyInfo, object, string, char, int>>();
            public static Dictionary<StatD.GeneralType, Func<ISaveable, object, string, int, bool, I_O>> GTSetActions;
            public static Dictionary<Func<PropertyInfo, object, bool>, Func<ISaveable, object, string, int, bool, I_O>> GTSetCombine = new Dictionary<Func<PropertyInfo, object, bool>, Func<ISaveable, object, string, int, bool, I_O>>();
            public static Dictionary<StatD.GeneralType, Func<P_O, int, string[], P_O>> GTGetElemActions; 
            static Functions()
            {
                GTSelection = getTypeSelector();
                GTGetActions = InitGetActions();
                GTGetCombine = GTSelection.ToDictionary(x => x.Value, x => GTGetActions[x.Key]);
                GTSetActions = InitSetActions();
                GTSetCombine = GTSelection.ToDictionary(x => x.Value, x => GTSetActions[x.Key]);
                GTGetElemActions = InitGetElement(); 
            }
            public static Dictionary<StatD.GeneralType, Func<PropertyInfo, object, bool>> getTypeSelector()
            {
                var Actions = new Dictionary<StatD.GeneralType, Func<PropertyInfo, object, bool>>();
                Func<PropertyInfo, object, Type> getType = (x, o) => ((o == null) ? x.PropertyType : o.GetType());
                Func<PropertyInfo, object, string> getTypeName = (x, o) => (getType(x, o).Name);
                Func<PropertyInfo, object, Type, bool> getInterface = ((x, o, T) => (T.IsAssignableFrom(getType(x, o))) ? true : false); ;
                Func<PropertyInfo, object, bool> WhenUse = null;

                WhenUse = new Func<PropertyInfo, object, bool>((x, o) => getInterface(x, o, typeof(IDictionary)));
                Actions.Add(StatD.GeneralType.Dictionary, WhenUse);

                WhenUse = new Func<PropertyInfo, object, bool>((x, o) => getInterface(x, o, typeof(Array)));
                Actions.Add(StatD.GeneralType.Array, WhenUse);

                WhenUse = new Func<PropertyInfo, object, bool>((x, o) => getInterface(x, o, typeof(IList)));
                Actions.Add(StatD.GeneralType.List, WhenUse);

                WhenUse = new Func<PropertyInfo, object, bool>((x, o) => getTypeName(x, o) == "DataTable" ? true : false);
                Actions.Add(StatD.GeneralType.DataTable, WhenUse);

                WhenUse = new Func<PropertyInfo, object, bool>((x, o) => getTypeName(x, o) == "DataSet" ? true : false);
                Actions.Add(StatD.GeneralType.DataSet, WhenUse);

                WhenUse = new Func<PropertyInfo, object, bool>((x, o) =>
                {
                    string N = getType(x, o).FullName;
                    return (N.Length > 13) && (N.Substring(0, 13) == "System.Tuple`") ? true : false;
                });
                Actions.Add(StatD.GeneralType.Tuple, WhenUse);

                WhenUse = new Func<PropertyInfo, object, bool>((x, o) =>
                {
                    string N = getType(x, o).FullName;
                    return (N.Length > 18) && (N.Substring(0, 18) == "<>f__AnonymousType") ? true : false;
                });
                Actions.Add(StatD.GeneralType.AnonymousType, WhenUse);

                WhenUse = new Func<PropertyInfo, object, bool>((x, o) => (getType(x, o).IsSerializable) ? true : false);
                Actions.Add(StatD.GeneralType.Standard, WhenUse);

                WhenUse = new Func<PropertyInfo, object, bool>((x, o) => true);
                Actions.Add(StatD.GeneralType.NotSerializable, WhenUse);
                return Actions;
            }
            public static Dictionary<StatD.GeneralType, Action<ISaveable, object, PropertyInfo, object, string, char, int>> InitGetActions()
            {
                int Index = 0;
                Dictionary<StatD.GeneralType, Action<ISaveable, object, PropertyInfo, object, string, char, int>> Res =
                new Dictionary<StatD.GeneralType, Action<ISaveable, object, PropertyInfo, object, string, char, int>>();
                Action<ISaveable, object, PropertyInfo, object, string, char, int> WhatDo = null;

                Func<PropertyInfo, object, char, string> AccessLevel = (x, o, u) =>
                {
                    string R = (o.GetType().IsValueType || (o.GetType() == typeof(string))) ? "v" : "r";
                    if (u == 'U') return R.ToUpper(); else if (u == 'L') return R;
                    return (x.CanWrite) ? R.ToUpper() : R;
                };

                Func<ISaveable, object, int, string, int> CheckAndAdd = (IS, o, N, s) =>
                {
                    int i = -1;
                    if (o != null)
                    {
                        i = IS.GetCache().LDependencies.IndexOf(o);
                        if (i == -1) { IS.GetCache().LDependencies.Add(o, N, s); i = IS.GetCache().LDependencies.Count - 1; }
                        else { if (s != null)  IS.GetCache().LDependencies[i].Names.Add(s); if (N != StatD.MaxU)  IS.GetCache().LDependencies[i].Numbers.Add(N); }
                    }
                    return i;
                };
                Func<ISaveable, object, bool> Check = (IS, o) => ((o != null) ? IS.GetCache().LDependencies.IndexOf(o) : -1) != -1;
                Func<object, PropertyInfo, object, object> getObject = (O, x, o) => ((o == null) ? x.GetValue(O, null) : o);
                Func<PropertyInfo, object, Type> getType = (x, o) => ((o == null) ? x.PropertyType : o.GetType());

                //Dictionary========================            
                WhatDo = new Action<ISaveable, object, PropertyInfo, object, string, char, int>((IS, O, x, o, s, u, I) =>
                {
                    IDictionary d = (IDictionary)((o == null) ? x.GetValue(O, null) : o);
                    if (d != null)
                    {
                        string S = AccessLevel(x, d, u);
                        bool ch = (S.ToUpper() == "V") || (!Check(IS, o));
                        string CH = ch ? "o" : "i";
                        List<string> T = new List<string>() { d.GetType().FullName };
                        T.AddRange(d.GetType().GetGenericArguments().Select(xx => xx.FullName).ToList());
                        IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.Dictionary.ToString(), CH, S, CheckAndAdd(IS, d, Index, s),
                        T, new List<string>() { d.Count.ToString() }));
                        Index = Index + 1;
                        if (CH != "i")
                        {
                            int i = 0;
                            foreach (DictionaryEntry p in d)
                            {
                                Functions.GTGetCombine.FirstOrDefault(a => a.Key(x, p.Key)).Value(IS, o, x, p.Key, s + ".0." + i.ToString(), 'L', I + 1);
                                Functions.GTGetCombine.FirstOrDefault(a => a.Key(x, p.Value)).Value(IS, o, x, p.Value, s + ".1." + i.ToString(), 'U', I + 1);
                                i = i + 1;
                            }
                            Index = Index + 1;
                            IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.Dictionary.ToString()));
                        }
                    }
                });
                Res.Add(StatD.GeneralType.Dictionary, WhatDo);

                //Array========================            
                WhatDo = new Action<ISaveable, object, PropertyInfo, object, string, char, int>((IS, O, x, o, s, u, I) =>
                {
                    IList l = (o == null) ? (IList)x.GetValue(O, null) : (IList)o;
                    Array A = (Array)l; List<string> Range = new List<string>();
                    for (int j = 0; j < A.Rank; j++) Range.Add(A.GetLength(j).ToString());
                    Type T = l.GetType().GetElementType();
                    if (T == null)
                    {
                        PropertyInfo TT = l.GetType().GetProperties().Where(X => (X.Name == "Item")).FirstOrDefault();
                        T = (TT == null) ? TypeOfList(l.GetType()) : TT.PropertyType;
                    }
                    IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.Array.ToString(), "o", AccessLevel(x, A, u), CheckAndAdd(IS, o, Index, s),
                        new List<string>() { l.GetType().FullName, T.FullName }, Range));
                    Index = Index + 1;
                    int i = 0;
                    foreach (var p in l)
                    {
                        Functions.GTGetCombine.FirstOrDefault(a => a.Key(x, p)).Value(IS, o, x, p, s + ExtISaveable.StatD.DM3 + i.ToString(), 'U', I + 1);
                        i = i + 1;
                    }
                    Index = Index + 1;
                    IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.Array.ToString()));
                });
                Res.Add(StatD.GeneralType.Array, WhatDo);

                //IList========================            
                WhatDo = new Action<ISaveable, object, PropertyInfo, object, string, char, int>((IS, O, x, o, s, u, I) =>
                {
                    IList l = (o == null) ? (IList)x.GetValue(O, null) : (IList)o;
                    o = l;
                    Type T = l.GetType().GetElementType();
                    if (T == null)
                    {
                        PropertyInfo TT = l.GetType().GetProperties().Where(X => (X.Name == "Item")).FirstOrDefault();
                        T = (TT == null) ? TypeOfList(l.GetType()) : TT.PropertyType;
                    }
                    string S = AccessLevel(x, l, u);
                    bool ch = (S.ToUpper() == "V") || (!Check(IS, o));
                    string CH = ch ? "o" : "i";
                    IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.List.ToString(), CH, S, CheckAndAdd(IS, o, Index, s),
                        new List<string>() { l.GetType().FullName, T.FullName }, new List<string>() { l.Count.ToString() }));
                    Index = Index + 1;
                    if (CH != "i")
                    {
                        int i = 0;
                        foreach (var p in l)
                        {
                            Functions.GTGetCombine.FirstOrDefault(a => a.Key(x, p)).Value(IS, o, x, p, s + ExtISaveable.StatD.DM3 + i.ToString(), 'U', I + 1);
                            i = i + 1;
                        }
                        Index = Index + 1;
                        IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.List.ToString()));
                    }
                });
                Res.Add(StatD.GeneralType.List, WhatDo);

                //DataTable========================           
                WhatDo = new Action<ISaveable, object, PropertyInfo, object, string, char, int>((IS, O, x, o, s, u, I) =>
                {
                    if (o == null) o = x.GetValue(O, null);
                    DataTable d = (DataTable)o;
                    List<string> S = new List<string>() { d.TableName, d.Namespace };
                    for (int j = 0; j < d.Columns.Count; j++) S.Add(d.Columns[j].Caption);
                    IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.DataTable.ToString(), "o", AccessLevel(x, d, u), CheckAndAdd(IS, o, Index, s),
                        d.Columns.Cast<DataColumn>().Select(X => X.DataType.FullName).ToList(), new List<string>() { d.Columns.Count.ToString(), d.Rows.Count.ToString() }, S));
                    Index = Index + 1;
                    for (int i = 0; i < d.Rows.Count; i++)
                    {
                        for (int k = 0; k < d.Rows[i].ItemArray.Length; k++)
                            Functions.GTGetCombine.FirstOrDefault(a => a.Key(x, d.Rows[i].ItemArray[k])).
                                Value(IS, o, x, d.Rows[i].ItemArray[k], s + ExtISaveable.StatD.DM3 + i.ToString() + ExtISaveable.StatD.DM3 + k.ToString(), 'P', I + 1);
                    }
                    Index = Index + 1;
                    IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.DataTable.ToString()));
                });
                Res.Add(StatD.GeneralType.DataTable, WhatDo);

                //DataSet========================            
                WhatDo = new Action<ISaveable, object, PropertyInfo, object, string, char, int>((IS, O, x, o, s, u, I) =>
                {
                    if (o == null) o = x.GetValue(O, null);
                    DataSet d = (DataSet)o;
                    List<string> S = new List<string>() { d.DataSetName, d.Namespace };
                    IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.DataSet.ToString(), "o", AccessLevel(x, d, u), CheckAndAdd(IS, o, Index, s),
                        null, new List<string>() { d.Tables.Count.ToString() }, S));
                    Index = Index + 1;
                    int i = 0;
                    foreach (DataTable dt in d.Tables)
                    {
                        Functions.GTGetCombine.FirstOrDefault(a => a.Key(x, dt)).Value(IS, o, x, dt, s + ExtISaveable.StatD.DM3 + i.ToString(), 'U', I + 1);
                        i = i + 1;
                    }
                    Index = Index + 1;
                    IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.DataSet.ToString()));
                });
                Res.Add(StatD.GeneralType.DataSet, WhatDo);

                //Tuple========================            
                WhatDo = new Action<ISaveable, object, PropertyInfo, object, string, char, int>((IS, O, x, o, s, u, I) =>
                {
                    if (o == null) o = x.GetValue(O, null);
                    string S = AccessLevel(x, o, 'P');
                    bool ch = !Check(IS, o);
                    string CH = ch ? "o" : "i";
                    List<PropertyInfo> PI = o.GetType().GetProperties
                       (BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList();
                    IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.Tuple.ToString(), CH, S, CheckAndAdd(IS, o, Index, s),
                        PI.Select(X => X.PropertyType.FullName).ToList(), new List<string>() { PI.Count.ToString() }));
                    Index = Index + 1;
                    if (ch)
                    {
                        int i = 0;
                        foreach (var p in PI)
                        {
                            Functions.GTGetCombine.FirstOrDefault(a => a.Key(p, null)).Value(IS, o, p, null, s + ExtISaveable.StatD.DM3 + i.ToString(), 'L', I + 1);
                            i = i + 1;
                        }
                        Index = Index + 1;
                        IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.Tuple.ToString()));
                    }
                });
                Res.Add(StatD.GeneralType.Tuple, WhatDo);

                //Anonymous========================            
                WhatDo = new Action<ISaveable, object, PropertyInfo, object, string, char, int>((IS, O, x, o, s, u, I) =>
                {
                    if (o == null) o = x.GetValue(O, null);
                    string S = AccessLevel(x, o, 'P');
                    bool ch = !Check(IS, o);
                    string CH = ch ? "o" : "i";
                    List<PropertyInfo> PI = o.GetType().GetProperties
                       (BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList();
                    List<string> A = PI.Select(m=>m.Name).ToList();
                    List<string> L = new List<string>() { o.GetType().FullName };
                    L.AddRange(PI.Select(X => X.PropertyType.FullName).ToList());
                    IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.AnonymousType.ToString(), CH, S, CheckAndAdd(IS, o, Index, s),
                        L, new List<string>() { PI.Count.ToString() },A));
                    Index = Index + 1;
                    if (ch)
                    {
                        // int i = 0;
                        foreach (var p in PI)
                        {
                            Functions.GTGetCombine.FirstOrDefault(a => a.Key(p, null)).Value(IS, o, p, null, s + ExtISaveable.StatD.DM3 + p.Name, 'L', I + 1);
                            //  i = i + 1;
                        }
                        Index = Index + 1;
                        IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.AnonymousType.ToString()));
                    }
                });
                Res.Add(StatD.GeneralType.AnonymousType, WhatDo);

                //Standard========================            
                WhatDo = new Action<ISaveable, object, PropertyInfo, object, string, char, int>((IS, O, x, o, s, u, I) =>
                {
                    if (o == null) o = x.GetValue(O, null);
                    string S = AccessLevel(x, o, u);
                    bool ch = !Check(IS, o);
                    int N = (S.ToUpper() == "R") ? N = CheckAndAdd(IS, o, Index, s) : -1;
                    if (ch)
                        IS.GetCache().LData.Add(new MData(SerializeObjectBinary(o), I, s, StatD.GeneralType.Standard.ToString(), "n", S, N,
                        new List<string>() { o.GetType().FullName }));
                    else IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.Standard.ToString(), "i", S, N));

                    Index = Index + 1;
                });
                Res.Add(StatD.GeneralType.Standard, WhatDo);

                //Other========================            
                WhatDo = new Action<ISaveable, object, PropertyInfo, object, string, char, int>((IS, O, x, o, s, u, I) =>
                {

                    if (o == null) o = x.GetValue(O, null);
                    string S = AccessLevel(x, o, u);
                    bool ch = (S.ToUpper() == "V") || (!Check(IS, o));
                    string CH = ch ? "o" : "i";
                    Type T = o.GetType();
                    List<PropertyInfo> PI = o.GetType().GetProperties
                       (BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).Where(X => IS.GetCache().ToUse(o, X)).ToList();
                    IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.NotSerializable.ToString(), CH, S, CheckAndAdd(IS, o, Index, s),
                                new List<string>() { o.GetType().FullName }, new List<string>() { PI.Count.ToString() }));
                    Index = Index + 1;
                    if ((PI.Count > 0))
                    {
                        if (ch)
                        {
                            PI.ForEach(X => Functions.GTGetCombine.FirstOrDefault(a => a.Key(X, null)).Value(IS, o, X, null, s + ExtISaveable.StatD.DM3 + X.Name, 'P', I + 1));
                            Index = Index + 1;
                            IS.GetCache().LData.Add(new MData("", I, s, StatD.GeneralType.NotSerializable.ToString()));
                        }

                    }
                });
                Res.Add(StatD.GeneralType.NotSerializable, WhatDo);
                return Res;
            }
            public static Dictionary<StatD.GeneralType, Func<ISaveable, object, string, int, bool, I_O>> InitSetActions()
            {
                Dictionary<StatD.GeneralType, Func<ISaveable, object, string, int, bool, I_O>> Res = new Dictionary<StatD.GeneralType, Func<ISaveable, object, string, int, bool, I_O>>();
                Func<Array, List<int>> AGetRange = X =>
                {
                    List<int> Result = new List<int>();
                    for (int j = 0; j < X.Rank; j++) Result.Add(X.GetLength(j));
                    return Result;
                };

                Func<int, List<int>, List<int>> AGetNumbers = ((Num, Range) =>
                {
                    List<int> Result = new List<int>();
                    for (int j = 1; j < Range.Count; j++)
                    {
                        int Helper = 1;
                        for (int k = j; k < Range.Count; k++) Helper = Helper * Range[k];
                        Result.Add(Num / Helper);
                        Num = Num % Helper;
                    }
                    Result.Add(Num);
                    return Result;
                });

                Func<List<int>, List<int>, bool> ACheckL = (X, Y) =>
                (X.Count != Y.Count) ? false :
                    (X.Where((x, i) => x != Y[i]).FirstOrDefault() == 0) && (X[0] == Y[0]);

                Func<MData, string> ANonGetS = M => M.Info.Addit.Select((X, i) => X + "=(" + M.Info.Types[i+1] + ")default(" + M.Info.Types[i+1]+")")
                    .Aggregate((a, b) => a + "," + b);//  X.Aggregate((a,b)=>a+"=1, "+b)+"=1";
              //  "tyn=(System.UInt16)default(System.UInt16), jj=(System.String)default(System.String)"
                Func<MData, List<Type>> GTypes = ((M) =>
                {
                    List<Type> Result = new List<Type>();
                    M.Info.Types.ForEach(x => Result.Add((null as Type).GType(x)));
                    return Result;
                });
                Func<P_O, StatD.GeneralType> GT = X => Functions.GTSelection.Where(x => x.Value(X.P, X.O)).Select(x => x.Key).FirstOrDefault();
                Func<ISaveable, MData, object> GetMainObj = (IS, M) => IS.GetCache().LDependencies[M.Info.NRef].o;
                Func<MData, int, Type> GTypeM = (M, i) => (null as Type).GType(M.Info.Types[i]);
                Func<P_O, Type> GTypeP = P => P.P.PropertyType;
                Func<MData, int, P_O, Type> GType = (M, i, P) => GTypeM(M, i);//((P!=null)&&(P.P != null)) ? GTypeP(P) : GTypeM(M, i);
                Func<MData, bool> FromRef = M => M.Info.NRef != -1;
                Action<ISaveable, MData, object> SetRef = ((IS, M, O) =>
                {
                    if (FromRef(M) && (GetMainObj(IS, M) == null)) IS.GetCache().LDependencies[M.Info.NRef].o = O;
                });
                Func<ISaveable, MData, P_O, object> InitO = ((IS, M, p_o) =>
                {
                    object O = (FromRef(M) && IS.GetCache().LDependencies[M.Info.NRef].o != null) ? GetMainObj(IS, M) : p_o.O;
                    StatD.GeneralType gt = GTFromString(M.Info.GeneralizedType);
                    if ((O == null) || (GT(new P_O(null, null, O)) != gt)) return null;
                    if ((new List<StatD.GeneralType>() { StatD.GeneralType.Array, StatD.GeneralType.List, StatD.GeneralType.Dictionary, StatD.GeneralType.Standard }.Contains(gt))
                    || ((gt == StatD.GeneralType.NotSerializable) && (IS.GetCache().whatMustDo() == StatD.WhatMustDo.TryCreateNewifDifferentType)))
                    {
                        if (M.Info.Types[0] != O.GetType().FullName) return null;
                    }
                    return O;//if (IS.GetCache().whatMustDo() == WhatMustDo.IgnoreDifferentType) 
                });
                Action<bool, MData, P_O, object> SetVProperty = ((SetandRet, M, p_o, O) =>
                {
                    if (SetandRet && ((M.Info.Access == "R") || (M.Info.Access == "V")) && (p_o.O != O)
                     && (p_o.P != null) && (p_o.P.PropertyType == O.GetType())
                       ) p_o.P.SetValue(p_o.Parent, O, null);
                });
                Func<ISaveable, int, int> GoodNextI = ((IS, i) =>
                {
                    while ((i < IS.GetCache().LData.Count) && (IS.GetCache().LData[i].Info.Status == "c")) i = i + 1;
                    return i;
                });
                Func<ISaveable, int, I_O> BadI_O = (IS, i) =>
                 (i == IS.GetCache().LData.Count) ? new I_O(i, null) :
                 ((IS.GetCache().LCheckRecovers[i]) ? new I_O(i + 1, null) : null);
                Func<ISaveable, int, MData> GetMData = ((IS, i) =>
                {
                    IS.GetCache().LCheckRecovers[i] = true;
                    return IS.GetCache().LData[i];
                });
                Func<ISaveable, int, StatD.GeneralType> GetGType = (IS, i) =>
                GTFromString(IS.GetCache().LData[i].Info.GeneralizedType);
                Func<MData, int, int> GetLength = (M, i) => Convert.ToInt32(M.Info.Dimensions[i]);

                Func<ISaveable, object, string, int, bool, I_O> SetData = null;
                SetData = ((IS, ParentO, ParentPath, i, SetandRet) =>
                {
                    I_O i_o = BadI_O(IS, i);
                    if (i_o == null)
                    {
                        MData M = GetMData(IS, i); object O = null; i = i + 1;
                        P_O p_o = (ParentPath == StatD.FI) ? new P_O(null, null, IS) : GetElement(IS, M.Info);
                        if (M.Info.Status != "i")
                        {
                            O = InitO(IS, M, p_o) ?? NewInstance.GetInstance(GType(M, 0, p_o));
                            //Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(GType(M, 1,p_o), GType(M, 2,p_o)));
                            var DHelper = new Dictionary<object, object>();//Hashtable();
                            I_O R1 = new I_O(); I_O R2 = new I_O(i, null); int L = GetLength(M, 0);
                            for (int j = 0; j < L; j++)
                            {
                                R1 = Res[GetGType(IS, R2.NextN)](IS, null, null, R2.NextN, false);
                                R2 = Res[GetGType(IS, R1.NextN)](IS, null, null, R1.NextN, false);
                                DHelper.Add(R1.O, R2.O);
                            }
                            IDictionary d = (IDictionary)O; d.Clear();
                            DHelper.ToList().ForEach(x => d.Add(x.Key, x.Value));//.Cast<DictionaryEntry>()
                            SetRef(IS, M, O);
                            i = R2.NextN;
                        }
                        else O = GetMainObj(IS, M);
                        SetVProperty(SetandRet, M, p_o, O);
                        i_o = new I_O(GoodNextI(IS, i), O);
                    } return i_o;
                });
                Res.Add(StatD.GeneralType.Dictionary, SetData);

                SetData = ((IS, ParentO, ParentPath, i, SetandRet) =>
                {
                    I_O i_o = BadI_O(IS, i);
                    if (i_o == null)
                    {
                        MData M = GetMData(IS, i); object O = null; i = i + 1;
                        P_O p_o = (ParentPath == StatD.FI) ? new P_O(null, null, IS) : GetElement(IS, M.Info);
                        if (M.Info.Status != "i")
                        {
                            O = InitO(IS, M, p_o); Array A = (Array)O;
                            // List<int> Range = (O == null) ? null : AGetRange(A);
                            List<int> range = M.Info.Dimensions.Select(x => Convert.ToInt32(x)).ToList();
                            if ((A == null) || (!ACheckL(AGetRange(A), range)))
                                A = (Array)Activator.CreateInstance(GType(M, 0, p_o), range.Cast<object>().ToArray());
                            var l = new List<object>();
                            I_O R1 = new I_O(i, null); int L = range.Aggregate((p, x) => p *= x);
                            for (int j = 0; j < L; j++)
                            {
                                R1 = Res[GetGType(IS, R1.NextN)](IS, null, null, R1.NextN, false);
                                l.Add(R1.O);
                            }
                            for (int j = 0; j < L; j++) A.SetValue(l[j], AGetNumbers(j, range).ToArray());
                            O = A;
                            SetRef(IS, M, O);
                            i = R1.NextN;
                        }
                        else O = GetMainObj(IS, M);
                        SetVProperty(SetandRet, M, p_o, O);
                        i_o = new I_O(GoodNextI(IS, i), O);
                    } return i_o;
                });
                Res.Add(StatD.GeneralType.Array, SetData);

                SetData = ((IS, ParentO, ParentPath, i, SetandRet) =>
                {
                    I_O i_o = BadI_O(IS, i);
                    if (i_o == null)
                    {
                        MData M = GetMData(IS, i); object O = null; i = i + 1;
                        P_O p_o = (ParentPath == StatD.FI) ? new P_O(null, null, IS) : GetElement(IS, M.Info);
                        if (M.Info.Status != "i")
                        {
                            O = InitO(IS, M, p_o) ?? NewInstance.GetInstance(GType(M, 0, p_o));
                            var l = new List<object>();// (IList)NewInstance.GetInstance(GType(M, 0, p_o));
                            //Activator.CreateInstance(typeof(List<>).MakeGenericType(GType(M, 1,p_o)));                        
                            //var l = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(GType(M, 1,p_o)));                        
                            I_O R1 = new I_O(i, null); int L = GetLength(M, 0);
                            for (int j = 0; j < L; j++)
                            {
                                R1 = Res[GetGType(IS, R1.NextN)](IS, null, null, R1.NextN, false);
                                l.Add(R1.O);
                            }
                            IList LO = (IList)O; LO.Clear();
                            foreach (var x in l) LO.Add(x);
                            SetRef(IS, M, O);
                            i = R1.NextN;
                        }
                        else O = GetMainObj(IS, M);
                        SetVProperty(SetandRet, M, p_o, O);
                        i_o = new I_O(GoodNextI(IS, i), O);
                    } return i_o;
                });
                Res.Add(StatD.GeneralType.List, SetData);

                SetData = ((IS, ParentO, ParentPath, i, SetandRet) =>
                {
                    I_O i_o = BadI_O(IS, i);
                    if (i_o == null)
                    {
                        MData M = GetMData(IS, i); object O = null; i = i + 1;
                        P_O p_o = (ParentPath == StatD.FI) ? new P_O(null, null, IS) : GetElement(IS, M.Info);
                        if (M.Info.Status != "i")
                        {
                            O = InitO(IS, M, p_o) ?? new DataTable();//Activator.CreateInstance(typeof(DataTable));
                            DataTable d = (DataTable)O;
                            var S = GTypes(M).Select((x, j) => new { T = x, S = M.Info.Addit[j + 2] }).ToList();
                            d.TableName = M.Info.Addit[0]; d.Namespace = M.Info.Addit[1];
                            int Cc = GetLength(M, 0); int Rc = GetLength(M, 1);
                            DataTable D = d.Clone();
                            D.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList().
                            ForEach(X => D.Columns.Remove(X));
                            for (int j = 0; j < Cc; j++) D.Columns.Add(S[j].S, S[j].T);
                            for (int j = 0; j < Rc; j++) D.Rows.Add(D.NewRow());
                            I_O R1 = new I_O(i, null);
                            for (int j = 0; j < Rc; j++)
                            {
                                for (int k = 0; k < Cc; k++)
                                {
                                    R1 = Res[GetGType(IS, R1.NextN)](IS, null, null, R1.NextN, false);
                                    D.Rows[j][k] = R1.O;
                                }
                            }
                            O = D;
                            SetRef(IS, M, O);
                            i = R1.NextN;
                        }
                        else O = GetMainObj(IS, M);
                        SetVProperty(SetandRet, M, p_o, O);
                        i_o = new I_O(GoodNextI(IS, i), O);
                    } return i_o;
                });
                Res.Add(StatD.GeneralType.DataTable, SetData);

                SetData = ((IS, ParentO, ParentPath, i, SetandRet) =>
                {
                    I_O i_o = BadI_O(IS, i);
                    if (i_o == null)
                    {
                        MData M = GetMData(IS, i); object O = null; i = i + 1;
                        P_O p_o = (ParentPath == StatD.FI) ? new P_O(null, null, IS) : GetElement(IS, M.Info);
                        if (M.Info.Status != "i")
                        {
                            O = InitO(IS, M, p_o) ?? new DataSet();//NewInstance.GetInstance(GType(M, 0,p_o));
                            DataSet d = (DataSet)O;
                            d.DataSetName = M.Info.Addit[0];
                            d.Namespace = M.Info.Addit[1];
                            var l = new List<object>();
                            I_O R1 = new I_O(i, null); int L = GetLength(M, 0);
                            for (int j = 0; j < L; j++)
                            {
                                R1 = Res[GetGType(IS, R1.NextN)](IS, null, null, R1.NextN, false);
                                l.Add(R1.O);
                            }
                            d.Tables.Clear();
                            l.ForEach(x => d.Tables.Add((DataTable)x));
                            SetRef(IS, M, O);
                            i = R1.NextN;
                        }
                        else O = GetMainObj(IS, M);
                        SetVProperty(SetandRet, M, p_o, O);
                        i_o = new I_O(GoodNextI(IS, i), O);
                    } return i_o;
                });
                Res.Add(StatD.GeneralType.DataSet, SetData);

                SetData = ((IS, ParentO, ParentPath, i, SetandRet) =>
                {
                    I_O i_o = BadI_O(IS, i);
                    if (i_o == null)
                    {
                        MData M = GetMData(IS, i); object O = null; i = i + 1;
                        P_O p_o = (ParentPath == StatD.FI) ? new P_O(null, null, IS) : GetElement(IS, M.Info);
                        if (M.Info.Status != "i")
                        {
                            List<object> l = new List<object>();
                            I_O R1 = new I_O(i, null); int L = GetLength(M, 0);
                            for (int j = 0; j < L; j++)
                            {
                                R1 = Res[GetGType(IS, R1.NextN)](IS, null, null, R1.NextN, false);
                                l.Add(R1.O);
                            }
                            MethodInfo MI = typeof(Tuple).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x => x.ReturnType.Name == "Tuple`" + L.ToString());
                            if (MI != null)
                            {
                                MethodInfo GMI = MI.MakeGenericMethod(GTypes(M).ToArray());
                                O = GMI.Invoke(null, l.ToArray());
                            }
                            else O = null;
                            SetRef(IS, M, O);
                            i = R1.NextN;
                        }
                        else O = GetMainObj(IS, M);
                        SetVProperty(SetandRet, M, p_o, O);
                        i_o = new I_O(GoodNextI(IS, i), O);
                    } return i_o;
                });
                Res.Add(StatD.GeneralType.Tuple, SetData);

                SetData = ((IS, ParentO, ParentPath, i, SetandRet) =>
                {
                    I_O i_o = BadI_O(IS, i);
                    {
                        MData M = GetMData(IS, i); object O = null; i = i + 1;
                        P_O p_o = (ParentPath == StatD.FI) ? new P_O(null, null, IS) : GetElement(IS, M.Info);
                        if (M.Info.Status != "i")
                        {
                            List<object> l = new List<object>();
                            I_O R1 = new I_O(i, null); int L = GetLength(M, 0);
                            for (int j = 0; j < L; j++)
                            {
                                R1 = Res[GetGType(IS, R1.NextN)](IS, null, null, R1.NextN, false);
                                l.Add(R1.O);
                            }
                            //   var werbu= GetAnonymType("rnh=(System.UInt16)default(System.UInt16),rttr=(System.Windows.Forms.Button)default(System.Windows.Forms.Button)");
                            Type MI = null;
                            string f = string.Join("|", M.Info.Types.GetRange(1, M.Info.Addit.Count)) + "|" + string.Join("|", M.Info.Addit.ToArray());
                            DAn.TryGetValue(f, out MI);
                            if (MI == null)
                            {
                                MI = (Type)GetAnonymType(ANonGetS(M)).Invoke(null, null);
                                DAn.Add(f, MI);
                            }
                            O = Activator.CreateInstance(MI, l.ToArray());
                            SetRef(IS, M, O);
                            i = R1.NextN;
                        }
                        else O = GetMainObj(IS, M);
                        SetVProperty(SetandRet, M, p_o, O);
                        i_o = new I_O(GoodNextI(IS, i), O);
                    } return i_o;
                });
                Res.Add(StatD.GeneralType.AnonymousType, SetData);

                SetData = ((IS, ParentO, ParentPath, i, SetandRet) =>
                {
                    I_O i_o = BadI_O(IS, i);
                    if (i_o == null)
                    {
                        MData M = GetMData(IS, i); object O = null; i = i + 1;
                        P_O p_o = (ParentPath == StatD.FI) ? new P_O(null, null, IS) : GetElement(IS, ParentO, ParentPath, M.Info);                    
                        if (M.Info.Status != "i")
                        {
                            O = DeserializeObjectBinary(M.Data, GType(M, 0, null));
                            SetRef(IS, M, O);
                        }
                        else O = GetMainObj(IS, M);
                        SetVProperty(SetandRet, M, p_o, O);
                        i_o = new I_O(GoodNextI(IS, i), O);
                    } return i_o;
                });
                Res.Add(StatD.GeneralType.Standard, SetData);             

                SetData = ((IS, ParentO, ParentPath, i, SetandRet) =>
                {
                    I_O i_o = BadI_O(IS, i);
                    if (i_o == null)
                    {
                        MData M = GetMData(IS, i); object O = null; i = i + 1;
                        P_O p_o = (ParentPath == StatD.FI) ? new P_O(null, null, IS) : GetElement(IS, ParentO, ParentPath, M.Info);  
                        if (M.Info.Status != "i")
                        {
                            O = InitO(IS, M, p_o) ?? NewInstance.GetInstance(GType(M, 0, p_o));//((p_o!=null)&&(p_o.P != null)) ? GTypeP(p_o) : GTypeM(M, i));  //      GType(M, 0, p_o));
                            SetRef(IS, M, O);
                            I_O R1 = new I_O(i, null); int L = GetLength(M, 0);
                            for (int j = 0; j < L; j++)
                                R1 = Res[GetGType(IS, R1.NextN)](IS, O, M.Info.PropertiesName, R1.NextN, true);
                            i = R1.NextN;
                        }
                        else O = GetMainObj(IS, M);
                        SetVProperty(SetandRet, M, p_o, O);
                        i_o = new I_O(GoodNextI(IS, i), O);
                    } return i_o;
                });
                Res.Add(StatD.GeneralType.NotSerializable, SetData);
                return Res;
            }
            public static Dictionary<StatD.GeneralType, Func<P_O, int, string[], P_O>> InitGetElement()
            {
                Func<P_O, StatD.GeneralType> GT = X => Functions.GTSelection.Where(x => x.Value(X.P, X.O)).Select(x => x.Key).FirstOrDefault();
                Func<PropertyInfo, object, StatD.GeneralType> gt = (X, M) => Functions.GTSelection.Where(x => x.Value(X, M)).Select(x => x.Key).FirstOrDefault();
                Dictionary<StatD.GeneralType, Func<P_O, int, string[], P_O>> Res = new Dictionary<StatD.GeneralType, Func<P_O, int, string[], P_O>>();
                Func<P_O, int, string[], P_O> GetData = null;
                GetData = ((Parent, i, SS) =>
                {
                    P_O Result = new P_O(); Result.Parent = Parent.O;
                    if (Parent.O == null) return Result;
                    IDictionary d = (IDictionary)Parent.O;
                    int N = Convert.ToInt32(SS[i + 1]);
                    if (d.Count <= N) return Result;
                    if (SS[i] == "0")
                        Result.O = d.Keys.Cast<object>().ElementAt(N);
                    else
                        Result.O = d.Values.Cast<object>().ElementAt(N);
                    if (Result.O == null) return Result;
                    if ((i + 2) != SS.Length)
                    {
                        StatD.GeneralType T = GT(Result);
                        return Res.Where(x => x.Key == T).Select(x => x.Value(Result, i + 2, SS)).FirstOrDefault();
                    }
                    else return Result;
                });
                Res.Add(StatD.GeneralType.Dictionary, GetData);

                GetData = ((Parent, i, SS) =>
                {
                    P_O Result = new P_O(); Result.Parent = Parent.O;
                    if (Parent.O == null) return Result;
                    Array d = (Array)Parent.O;
                    int N = Convert.ToInt32(SS[i]);
                    if (d.Cast<object>().Count() <= N) return Result;
                    Result.O = d.Cast<object>().ElementAt(N);
                    if (Result.O == null) return Result;
                    if ((i + 1) != SS.Length)
                    {
                        StatD.GeneralType T = GT(Result);
                        return Res.Where(x => x.Key == T).Select(x => x.Value(Result, i + 1, SS)).FirstOrDefault();
                    }
                    else return Result;
                });
                Res.Add(StatD.GeneralType.Array, GetData);

                GetData = ((Parent, i, SS) =>
                {
                    P_O Result = new P_O(); Result.Parent = Parent.O;
                    if (Parent.O == null) return Result;
                    IList d = (IList)Parent.O;
                    int N = Convert.ToInt32(SS[i]);
                    if (d.Count <= N) return Result;
                    Result.O = d[N];
                    if (Result.O == null) return Result;
                    if ((i + 1) != SS.Length)
                    {
                        StatD.GeneralType T = GT(Result);
                        return Res.Where(x => x.Key == T).Select(x => x.Value(Result, i + 1, SS)).FirstOrDefault();
                    }
                    else return Result;
                });
                Res.Add(StatD.GeneralType.List, GetData);

                GetData = ((Parent, i, SS) =>
                {
                    P_O Result = new P_O(); Result.Parent = Parent.O;
                    if (Parent.O == null) return Result;
                    DataTable d = (DataTable)Parent.O;
                    int Rw = Convert.ToInt32(SS[i]);
                    if (d.Rows.Count <= Rw) return Result;
                    int Cn = Convert.ToInt32(SS[i + 1]);
                    if (d.Columns.Count <= Cn) return Result;
                    Result.O = d.Rows[Rw][Cn];
                    if (Result.O == null) return Result;
                    if ((i + 2) != SS.Length)
                    {
                        StatD.GeneralType T = GT(Result);
                        return Res.Where(x => x.Key == T).Select(x => x.Value(Result, i + 2, SS)).FirstOrDefault();
                    }
                    else return Result;
                });
                Res.Add(StatD.GeneralType.DataTable, GetData);

                GetData = ((Parent, i, SS) =>
                {
                    P_O Result = new P_O(); Result.Parent = Parent.O;
                    if (Parent.O == null) return Result;
                    DataSet d = (DataSet)Parent.O;
                    int Dt = Convert.ToInt32(SS[i]);
                    if (d.Tables.Count <= Dt) return Result;
                    /*int Rw = Convert.ToInt32(SS[i + 1]);
                    int Cn = Convert.ToInt32(SS[i + 2]);
                    Result.O = d.Tables[Dt].Rows[Rw][Cn];
                    if ((i + 3) != SS.Length)
                    {
                        GeneralType T = GT(Result);
                        return D.Where(x => x.Key == T).Select(x => x.Value(Result, i + 3, SS)).FirstOrDefault();
                    }*/
                    Result.O = d.Tables[Dt];
                    if (Result.O == null) return Result;
                    if ((i + 1) != SS.Length)
                    {
                        StatD.GeneralType T = GT(Result);
                        return Res.Where(x => x.Key == T).Select(x => x.Value(Result, i + 1, SS)).FirstOrDefault();
                    }
                    else return Result;
                });
                Res.Add(StatD.GeneralType.DataSet, GetData);

                GetData = ((Parent, i, SS) =>
                {
                    P_O Result = new P_O(); Result.Parent = Parent.O;
                    if (Parent.O == null) return Result;
                    List<PropertyInfo> PI = Parent.O.GetType().GetProperties
                       (BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList();
                    int N = Convert.ToInt32(SS[i]);
                    if (PI.Count <= N) return Result;
                    Result.O = PI[N].GetValue(Parent.O, null);
                    if (Result.O == null) return Result;
                    if ((i + 1) != SS.Length)
                    {
                        StatD.GeneralType T = GT(Result);
                        return Res.Where(x => x.Key == T).Select(x => x.Value(Result, i + 1, SS)).FirstOrDefault();
                    }
                    else return Result;
                });
                Res.Add(StatD.GeneralType.Tuple, GetData);

                GetData = ((Parent, i, SS) =>
                {
                    P_O Result = new P_O(); Result.Parent = Parent.O;
                    if (Parent.O == null) return Result;
                    Result.P = Parent.O.GetType().GetProperty(SS[i]);
                    if (Result.P == null) return Result;
                    Result.O = Result.P.GetValue(Parent.O, null);
                    if (Result.O == null) return Result;
                    if ((i + 1) != SS.Length)
                    {
                        StatD.GeneralType T = GT(Result);
                        return Res.Where(x => x.Key == T).Select(x => x.Value(Result, i + 1, SS)).FirstOrDefault();
                    }
                    else return Result;
                });
                Res.Add(StatD.GeneralType.AnonymousType, GetData);

                GetData = ((Parent, i, SS) =>
                {
                    P_O Result = new P_O(); Result.Parent = Parent.O;
                    Result.P = Parent.O.GetType().GetProperty(SS[i]);
                    if (Result.P == null) return Result;
                    Result.O = Result.P.GetValue(Parent.O, null);
                    return Result;
                });
                Res.Add(StatD.GeneralType.Standard, GetData);

                GetData = ((Parent, i, SS) =>
                {
                    P_O Result = new P_O(); Result.Parent = Parent.O;
                    if (Parent.O == null) return Result;
                    Result.P = Parent.O.GetType().GetProperty(SS[i]);
                    if (Result.P == null) return Result;
                    Result.O = Result.P.GetValue(Parent.O, null);
                    if (Result.O == null) return Result;
                    if ((i + 1) != SS.Length)
                    {
                        StatD.GeneralType T = GT(Result);
                        return Res.Where(x => x.Key == T).Select(x => x.Value(Result, i + 1, SS)).FirstOrDefault();
                    }
                    else return Result;
                });
                Res.Add(StatD.GeneralType.NotSerializable, GetData);
                return Res;
            }
            
            public static P_O GetElement(object IS, object O, string Name, MetaData ME)
            {
              //if (IS == O)  return new P_O(null, null, O);                    
                return ((O != null) && (Name != null) && (Name != "")) ? GetElement(O, Name, ME) : GetElement(IS, ME);
            }
            public static P_O GetElement(object IS, string Name, MetaData ME)
            {
                var MD = new MetaData();
                MD.PropertiesName = ME.PropertiesName.Substring(Name.Length + 1);
                return GetElement(IS, MD);
            }
            public static P_O GetElement(object IS, MetaData ME)
            {
                string[] S = ME.PropertiesName.Split(new string[] { ExtISaveable.StatD.DM3 }, StringSplitOptions.RemoveEmptyEntries);
                Func<P_O, StatD.GeneralType> GT = X => Functions.GTSelection.Where(x => x.Value(X.P, X.O)).Select(x => x.Key).FirstOrDefault();
                P_O R = new P_O(); R.O = IS; StatD.GeneralType t = GT(R);
                return Functions.GTGetElemActions.Where(x => x.Key == t).Select(x => x.Value(R, 0, S)).FirstOrDefault();
            }

            public static Type TypeOfList(Type type)
            {
                ///http://stackoverflow.com/questions/1043755/c-sharp-generic-list-t-how-to-get-the-type-of-t
                var interfaceTest = new Func<Type, Type>(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>) ? i.GetGenericArguments().Single() : null);
                Type innerType = interfaceTest(type);
                if (innerType != null) return innerType;
                foreach (var i in type.GetInterfaces())
                {
                    innerType = interfaceTest(i);
                    if (innerType != null) return innerType;
                }
                return innerType;
            }
            public static StatD.GeneralType GTFromString(string s)
            {
                return (StatD.GeneralType)Enum.Parse(typeof(StatD.GeneralType), s);
            }

            public static string cd = "public static class MF{public static System.Type Run(){return new {0}.GetType();}}";
            public static List<string> ErrLL = new List<string>() { 
               "Microsoft.VisualStudio.HostingProcess.Utilities", 
               "Microsoft.VisualStudio.Debugger",
               "System.Windows.Forms.resources.dll", 
               "mscorlib.resources.dll",
               "CrystalDecisions"
               };
            public static Dictionary<string, Type> DAn = new Dictionary<string, Type>();
            public static CompilerParameters CompilerParam = GetCompilerParameters();
            public static CompilerParameters GetCompilerParameters()
            {
                List<string> loadedLib = new List<string>();
                AppDomain.CurrentDomain.GetAssemblies().ToList().ForEach(a =>
                {
                    try
                    {
                        if ((a.Location != "") && (ErrLL.FirstOrDefault(x => a.Location.Contains(x)) == null))
                            loadedLib.Add(Path.GetFileName(a.Location));
                    }
                    catch { }
                });
                var parameters = new CompilerParameters();
                loadedLib.ForEach(X => parameters.ReferencedAssemblies.Add(X));
                parameters.GenerateInMemory = true;
                return parameters;
            }
            public static MethodInfo GetAnonymType(string S)
            {
                return (new CSharpCodeProvider()).CompileAssemblyFromSource(CompilerParam, cd.Replace("0", S))
                     .CompiledAssembly.GetType("MF").GetMethod("Run");
            }
        
        }
        public class TempSave
        { 
            private ISaveable me;
            public ISaveable This
            {
                get
                {
                    return me;
                }
                set
                {
                    me = value;
                    ObjectLabel = new OL(objectlabel); 
                }
            }

            public TempSave(ISaveable THIS)
            { This = THIS; }
            public TempSave(MapList ME, List<MData> MD)
            {
                this.LDependencies = ME;
                this.LData = MD;
            }           
            public TempSave(string S)
            {
                FromSerializableString(S);
            }
            public TempSave() { }

            public List<MData> LData = new List<MData>();
            public static string SerializeLData(List<MData> L)
            {
                string ff = "";
                L.ForEach(x => ff = ff + (x.Data + ExtISaveable.StatD.DM22 + x.Info.ToString() + ExtISaveable.StatD.DM23));
                return ff.Substring(0, ff.Length - 3);
            }
            public static void DeserealizeLData(List<MData> L, string Data)
            {
                string[] uu = Data.Split(new string[] { ExtISaveable.StatD.DM23 }, StringSplitOptions.RemoveEmptyEntries);
                L.Clear();
                uu.ToList().ForEach(u =>
                {
                    string[] rr = u.Split(new string[] { ExtISaveable.StatD.DM22 }, StringSplitOptions.None);
                    MetaData tt = new MetaData();
                    tt.FromString(rr[1]);
                    L.Add(new MData(rr[0], tt));
                });
            }            
            public MapList LDependencies = new MapList();
            public List<bool> LCheckRecovers;//no need for reading
            public void InitLCheckRecovers()
            {
                LCheckRecovers = Enumerable.Repeat(false, LData.Count).ToList();
            }
            public void InitMainObjForLDependencies()
            {

                foreach (MapElem d in LDependencies)
                {
                    int N = d.Numbers[IndexOfBaseObject(d)];
                    d.o = ExtISaveable.Functions.GetElement(This, LData[N].Info).O;
                    if (d.o != null) LCheckRecovers[N] = true;
                }
            }
             
            public void Init()
            {
                Functions.GTGetCombine.FirstOrDefault(a => a.Key(null, this.This)).Value(this.This, this.This, null, this.This, "", 'L', 0);                
               /* this.This.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                        .Where(x => this.This.GetCache().ToUse(this.This, x)).ToList()
                        .ForEach(X => Functions.GTGetCombine.FirstOrDefault(a => a.Key(X, null)).Value(this.This, this.This, X, null, X.Name, 'P', 0));
                //Not universal
                */
            }
            public void Recover()
            {
                if ((this.LData != null) && (this.LData.Count != 0))
                {
                    this.This.GetCache().InitLCheckRecovers();
                    this.This.GetCache().LDependencies.MainObjectReset();
                    //this.This.GetCache().InitMainObjForLDependencies();
                    ExtISaveable.I_O R = new ExtISaveable.I_O(0, null);
                    while (R.NextN < this.This.GetCache().LData.Count)
                        R = ExtISaveable.Functions.GTSetActions[ExtISaveable.Functions.GTFromString(this.This.GetCache().LData[R.NextN].Info.GeneralizedType)]
                            (this.This, this.This, StatD.FI, R.NextN, true);//.FirstOrDefault(x => x.Key(null, GetElement(IS, IS.GetCache().LData[0].Info).O)).Value(IS, i);
                }
            }
            public void SetCache(ExtISaveable.TempSave s, ISaveable IS)
            {
                TempSave S = s.ReturnClone();
                TempSave Me = IS.GetCache();
                Me.This = IS;
                Me.From(S);
            }
            public TempSave ReturnClone()
            {
                TempSave ww1 = (TempSave)this;
                TempSave we = new TempSave();
                string qwe = ww1.LDependencies.Serialize();
                string qwe2 = SerializeLData(ww1.LData);
                DeserealizeLData(we.LData, qwe2);
                we.LDependencies.Deserealize(qwe);
                return we;
            }
            public void From(TempSave T)
            {
                this.LData = T.LData;
                this.LDependencies = T.LDependencies;
            }
            public string ToSerializableString()
            {
                ForSerialize fs = new ForSerialize(this.LDependencies,this.LData);
                return fs.ToString();
            }
            public string ToString()
            {
                int i = 0; string S = "";
                string l="\r\n".ToString();                
                foreach (var e in this.LData)
                {
                    S=S+i.ToString() + "_" + e.Info.ToString() + "  " + e.Data+l;
                    i = i + 1;
                }
               // return S.Substring(0, S.Length - 2);
                return S+ this.LDependencies.ToString();
            }
            public void FromSerializableString(string S)
            {
                ForSerialize FS = new ForSerialize(S);
                this.LDependencies = FS.GetMapList();
                this.LData = FS.GetLData();
                this.This.GetCache().Recover();
            }
            #region Default external methods
            public delegate string OL();            
            public OL ObjectLabel;
            private string objectlabel()
            {
                return GetObjectHash(This);
            }
            public static string GetObjectHash(ISaveable IS)
            {
                PropertyInfo PI = IS.GetType().GetProperty("Name");
                if (PI != null)
                {
                    return PI.GetValue(IS, null).GetHashCode().ToString("X8");
                }
                else return IS.GetType().Name.ToString().GetHashCode().ToString("X8");
                //return Check(() => This);   
            }
            /* static string Check<T>(Expression<Func<T>> expr)
             {            
                 var body = ((MemberExpression)expr.Body);
                 return body.ToString(); //((FieldInfo)body.Member).Name;         
                 return body.Member.Name+"  "+   ((FieldInfo)body.Member).GetValue(((ConstantExpression)body.Expression).Value);
             }*/
            public delegate int IBO(MapElem M);
            public IBO IndexOfBaseObject = new IBO(indexOfBaseObject);            
            public static int indexOfBaseObject(MapElem s)
            {
                //s.Numbers
                return 0;
            }

            public delegate StatD.WhatMustDo WMD();
            public WMD whatMustDo = new WMD(wmd);
            private static StatD.WhatMustDo wmd()
            {
                return StatD.WhatMustDo.TryCreateNewifDifferentType;//IgnoreDifferentType;
            }

            public delegate bool TU(object O, PropertyInfo x);
            public TU ToUse = new TU(touse);
            private static bool touse(object O, PropertyInfo x)
            {        
                if (x.Name.Contains("arent")) return false;
                if (x.Name.Contains("wner")) return false;           
                Type T = x.PropertyType;
                if (T.IsAbstract) return false;
                if (x.GetIndexParameters().Length > 0) return false;
                if (x.Name == "DataBindings") return false;
                if (x.Name == "ActiveControl") return false;
                if (x.Name == "TopLevelControl") return false;
                if (x.GetValue(O, null) == null) return false;
                if ((T == typeof(IntPtr)) || (T == typeof(UIntPtr))) return false;
                if ((!x.CanWrite) && T.IsSerializable) return false;
                return true;
            }
            #endregion
        }
             
        public class ForSerialize// public for Serialize
        {
            public class ShortMapElem
            {
                public List<int> Numbers;
                public List<string> Names;
                public ShortMapElem(MapElem M)
                {
                    Names = M.Names;
                    Numbers = M.Numbers;
                }
                public  ShortMapElem() { }
            } // public for Serialize
            public List<ShortMapElem> mapElem;
            public List<MData> LData;
            public MapList GetMapList()
            {
                if (mapElem == null) return null;
                MapList R = new MapList();
                mapElem.ForEach(x=>R.Add(new MapElem(x.Names, x.Numbers)));
                return R;
            }
            public List<MData> GetLData()
            {
                return LData;
            }
            public ForSerialize(MapList ME,List<MData> MD){
                mapElem = ME.Select(x => new ShortMapElem(x)).ToList();
                LData = MD;
            }
            public ForSerialize(string S)
            {
                FromString(S);             
            }
            public ForSerialize() { }

            public string ToString() {
                return SerializeObject(this);
            }
            public void FromString(string S)
            {
                ExtISaveable.ForSerialize fs = DeserializeObject<ExtISaveable.ForSerialize>(S);
                this.mapElem=fs.mapElem; 
                this.LData=fs.LData; 
            }
            public static T DeserializeObject<T>(string toDeserialize)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                StringReader textReader = new StringReader(toDeserialize);
                return (T)xmlSerializer.Deserialize(textReader);
            }
            public string SerializeObject<T>(T toSerialize)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
                using (StringWriter textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, toSerialize);
                    string d=((char)34).ToString();                    
                    return textWriter.ToString().
                        Replace("encoding="+d+"UTF-16","encoding="+d +"UTF-8").
                        Replace("encoding="+d+"utf-16","encoding="+d +"utf-8");
                }
            }

        }
        public class MapElem
        {
            public object o;
            
            public List<string> Names;
            public List<int> Numbers;
            public MapElem(object o, int N = StatD.MaxU, string s = null)
            {
                this.o = o;
                Names = new List<string>();
                Numbers = new List<int>();
                if (s != null) Names.Add(s);
                if (N != StatD.MaxU) Numbers.Add(N);
            }
            public MapElem(List<string> NM, List<int>NB)
            {
                Names = NM;
                Numbers = NB;
            }   
        }
        public class MapList : List<MapElem>
        {  
            public MapElem this[int i]
            {
                get
                { return base[i]; }
                set
                { base[i] = value; }
                
            }
            public MapElem this[object o]
            {
                get
                {
                    return this.Find(x => x.o == o);
                    //  return this.Where(x => x.o == o).FirstOrDefault();
                }
                set
                {
                    int i = this.FindIndex(x => x.o == o);
                    if (i > -1) this[i] = value; else this.Add(value);
                }
            }
            public MapElem this[string s]
            {
                get
                {
                    return this.Find(x => x.Names.Contains(s)); 
                    // return this.Where(x => x.Names.Contains(s)).FirstOrDefault(); 
                }
                set
                {
                    int i = this.FindIndex(x => x.Names.Contains(s));
                    if (i > -1) this[i] = value; else this.Add(value);
                }       
            }
            public MapElem this[int N,string s]
            {                
                get
                {
                    return this.Find(x => x.Numbers.Contains(N));                    
                }
                set
                {
                    int i = this.FindIndex(x => x.Numbers.Contains(N));
                    if (i > -1) this[i] = value; else this.Add(value);
                }
            }
            public int IndexOf(string s)
            { 
                return this.FindIndex(x => x.Names.Contains(s));                    
            }
            public int IndexOf(object o)
            {
                return this.FindIndex(x => x.o == o);
            }

            public void Add(object o, int N = StatD.MaxU, string s = null)
            {
                base.Add(new MapElem(o,N,s));
            }
            public string  Serialize()
            {
                var rrt = this.Select(x => new KeyValuePair<List<string>, List<int>>(x.Names, x.Numbers)).ToList();
                return SerializeObjectBinary(this.Select(x => new KeyValuePair<List<string>, List<int>>(x.Names, x.Numbers)).ToList());
            }
            public void Deserealize(string Data) {
                var qw = (List<KeyValuePair<List<string>, List<int>>>)DeserializeObjectBinary(Data, typeof(List<KeyValuePair<List<string>, List<int>>>));               
                this.Clear();
                this.AddRange(qw.Select(x => new MapElem(x.Key, x.Value)));
            }
            public void InitMainObject(string Data)
            {
                var qw = (List<KeyValuePair<List<string>, List<int>>>)DeserializeObjectBinary(Data, typeof(List<KeyValuePair<List<string>, List<int>>>));
                this.Clear();
                this.AddRange(qw.Select(x => new MapElem(x.Key, x.Value)));
            }
            public void MainObjectReset()
            {
                foreach (MapElem M in this) M.o = null;
            }
            public string ToString()
            {          
                string S="";string l="\r\n".ToString();  
                this.ForEach(a=>S=S+ string.Join(ExtISaveable.StatD.DM1, a.Names.ToArray())+ExtISaveable.StatD.DM22+
                    string.Join(ExtISaveable.StatD.DM1, a.Numbers.Select(x=>x.ToString()).ToArray())+l);
                return S.Substring(0,S.Length-2);
            }
        }
        public class MetaData
        {            
            public int Level;
            public string PropertiesName;
            public string GeneralizedType;
            public string Status;
            public string Access;//i think about 2 letter - so not char
            public int NRef;
            public List<string> Types;
            public List<string> Dimensions;
            public List<string> Addit;
            public override string ToString() {
                object o = this;
                return string.Join(ExtISaveable.StatD.DM1, this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Select(X => (X.GetValue(o) ?? "null")).
                   Select(x => (x is List<string>) ? string.Join(ExtISaveable.StatD.DM2, (List<string>)x) : x.ToString()));           
            }
            public void FromString(string s)
            {
                object o = this;
                string[] S = s.Split(new string[] { ExtISaveable.StatD.DM1 }, StringSplitOptions.None);
                FieldInfo[] F = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < F.Length; i++)
                {
                    if (S[i] != "null")
                    {
                        if (F[i].FieldType == typeof(List<string>))
                            F[i].SetValue(this, S[i].Split(new string[] { ExtISaveable.StatD.DM2 }, StringSplitOptions.None).ToList());
                        else
                            F[i].SetValue(this, Convert.ChangeType(S[i], F[i].FieldType));
                    }
                    else F[i].SetValue(this, null);
                }
            }
            public MetaData Clone() {
                return (MetaData)this.MemberwiseClone();
            }
        }
        public class MData
        {            
            public MetaData Info = new MetaData();
            public string Data;
            public MData()
            { }
            public MData(string data, MetaData I)
            {
                Data = data;
                Info = I.Clone(); 
            }
            public MData(string data, int Level, string PropertiesName, string GeneralizedType, string OpenCloseNeutral="c", string Access="", int NRef=-1, List<string> Types=null, List<string> Dimensions=null, List<string> Addit=null)
            {
                Data = data;
                Info.Level = Level;
                Info.PropertiesName = PropertiesName;
                Info.GeneralizedType = GeneralizedType;
                Info.Status = OpenCloseNeutral;
                Info.Access = Access;
                Info.NRef = NRef;
                Info.Types = Types;
                Info.Dimensions = Dimensions;
                Info.Addit = Addit;
            }
        }
        public class SProperty
        {
            public string Name;
            public string Value;
        }
        private static class WFolder
        {

            public static string GetSaveSettingsFolder()
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            }

            public static string GetAppHash()
            {
                return Assembly.GetExecutingAssembly().Location.GetHashCode().ToString("X8");
            }

            public static string GetDomain()
            {
                string S = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
                return ((S != null) && (S != "")) ? S : "Martmath";
            }


            public static string GetFolderPath()
            {
                return GetSaveSettingsFolder() + "\\" + GetDomain() + "\\" + GetAppHash();
            }

            public static string GetFullPath(ISaveable IS, string label = null)
            {
                label = label ?? IS.GetCache().ObjectLabel();
                return GetFolderPath() + "\\" + label + ".xml";
            }
        }
        private static string SerializeObjectBinary(object o)
        {
            if (o.GetType().Equals(typeof(string))) return o.ToString();
            if (o.GetType().IsPrimitive) return o.ToString();
            if (o.GetType().IsEnum) return ((int)o).ToString() + "|" + o.ToString();
            using (MemoryStream stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, o);
                return Convert.ToBase64String(stream.ToArray());
            }
        }
        private static object DeserializeObjectBinary(string str, Type T)
        {
            //if (T == null) return Convert.ToInt32(str.Substring(0, str.IndexOf('|')));            
            if (T.IsPrimitive) return Convert.ChangeType(str, T);
            if (T.IsEnum)
            {
                return Convert.ToInt32(str.Substring(0, str.IndexOf('|')));// Convert.ChangeType(str.Substring(0, str.IndexOf('|')), T);
            }
            if (T.Equals(typeof(string))) return str;
            byte[] bytes = Convert.FromBase64String(str);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return new BinaryFormatter().Deserialize(stream);
            }
        }
       
        public static string Save(this ISaveable IS,string label=null)
        {
            string P = WFolder.GetFolderPath(); 
            if (!Directory.Exists(P))
            {
                string p = WFolder.GetSaveSettingsFolder();
                string[] S = P.Substring(p.Length+1).Split('\\');                
                for (int i =0; i<S.Length;  i++)
                {
                    p = p + "\\" + S[i];
                    if (!Directory.Exists(p))
                    {
                        Directory.CreateDirectory(p);
                        for (int j = i + 1; j < S.Length; j++)
                        {
                            p = p + "\\" + S[j];
                            Directory.CreateDirectory(p);
                        }
                        break;
                    }
                }
            }
            using (StreamWriter objWriter = new StreamWriter(WFolder.GetFullPath(IS, label)))
            {
                IS.GetCache().Init();
                objWriter.Write(IS.GetCache().ToSerializableString()); 
            }
            return IS.GetCache().ObjectLabel();
        }
        public static void LoadData(this ISaveable IS,string label=null)
        {
                TempSave p = IS.GetCache();
                string S = "";
                string P = WFolder.GetFullPath(IS, label);
                if (File.Exists(P))
                {   
                    using (StreamReader objReader = new StreamReader(P))
                    {
                        S = objReader.ReadToEnd();
                    }
                    IS.GetCache().FromSerializableString(S);
                    IS.GetCache().Recover();                  
                }
           
        }
        
        
       
             
    }
}
