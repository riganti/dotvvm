using DotVVM.Framework.Compilation.Javascript.Ast;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MatchAntiCycle = System.Collections.Generic.HashSet<(DotVVM.Framework.Compilation.Javascript.IJsTypeInfo, DotVVM.Framework.Compilation.Javascript.IJsTypeInfo)>;
using System.Collections;
using System.Collections.Immutable;

namespace DotVVM.Framework.Compilation.Javascript
{
    public interface IJsTypeInfo
    {
        bool CanBeNull { get; }
        string Typeof { get; }
        IJsTypeInfo WithCanBeNull(bool val);
        IJsTypeInfo AddAnnotation(object annotation);
        T Annotation<T>();
    }

    public interface IJsMemberAccessType
    {
        JsPropertyInfo GetProperty(string name);
        IEnumerable<JsPropertyInfo> ListProperties();
    }

    public interface IJsInvocableType
    {
        IEnumerable<JsMethodSignature> GetSignatures();
    }

    public interface IJsIndexerAccess
    {
        IJsTypeInfo ElementType { get; }
        bool IntegerOnly { get; }
    }

    public static class JsTypeInfo
    {
        //private static Dictionary<Type, Func<IJsTypeInfo, IJsTypeInfo, bool>> interfaceMatchers = new Dictionary<Type, Func<IJsTypeInfo, IJsTypeInfo, bool>>
        //{
        //    [typeof(IJsMemberAccessType)] = (a, b) =>
        //    {
        //    }
        //};
        public static bool MatchType(this IJsTypeInfo a, IJsTypeInfo b, MatchAntiCycle antiCycle = null)
        {
            if (a == null | b == null) return false;
            if (a == b) return true;

            if (a.Typeof != b.Typeof || a.Typeof == "object" && b.Typeof == "function") return false;

            antiCycle = antiCycle ?? new MatchAntiCycle();
            antiCycle.Add((a, b));
            if (!a.CanBeNull && b.CanBeNull) return false;

            if (a is IJsMemberAccessType aMember && 
                (!(b is IJsMemberAccessType bMember) ||
                    !aMember.ListProperties().All(
                        p => bMember.GetProperty(p.Name) is var p2 && p2.Exists && p.Type.MatchType(p2.Type, antiCycle))))
                return false;

            if (a is IJsInvocableType aInvokable &&
                (!(b is IJsInvocableType bInvokable) ||
                    !(bInvokable.GetSignatures().ToArray() is var bSignatures &&
                    aInvokable.GetSignatures().All(sig =>
                        bSignatures.Any(sigB => sigB.MatchSig(sig, antiCycle))))))
                return false;

            return true;

            //foreach (var im in interfaceMatchers)
            //{
            //    var (ai, bi) = (im.Key.IsInstanceOfType(a), im.Key.IsInstanceOfType(b));
            //    if (ai && !bi) return false;
            //}
        }

        public static class LazyEnumerable
        {
            public static LazyEnumerable<T> Create<T>(Func<IEnumerable<T>> c) => new LazyEnumerable<T>(c);
        }
        public class  LazyEnumerable<T> :IEnumerable<T>
        {
            private Lazy<IEnumerable<T>> c;

            public LazyEnumerable(Func<IEnumerable<T>> c) { this.c = new Lazy<IEnumerable<T>>(c); }

            public IEnumerator<T> GetEnumerator() => c.Value.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => c.Value.GetEnumerator();
        }

        public static readonly JsObjectType Boolean = new JsObjectType(LazyEnumerable.Create(() => new JsPropertyInfo[] { }), "boolean");
        public static readonly JsObjectType Number = new JsObjectType(LazyEnumerable.Create(() => new[]
        {
            new JsPropertyInfo("toFixed", new JsFunctionType(new[]{ new JsMethodSignature(new[] { Number.WithCanBeNull(true) }, Number) }))
        }), "number");
        public static readonly JsObjectType String = new JsObjectType(new[]{
            new JsPropertyInfo("length", Number)
        }, "string");
        public static JsAnyType Any => JsAnyType.Instance;
        public static IJsTypeInfo Error = JsAnyType.Instance.AddAnnotation(new ErrorType());
        class ErrorType { }
    }

    public class JsPropertyInfo
    {
        public string Name { get; }
        public IJsTypeInfo Type { get; }
        public bool Exists => Type != null;

        public JsPropertyInfo(string name, IJsTypeInfo type)
        {
            this.Name = name;
            this.Type = type;
        }
    }

    public class JsMethodSignature
    {
        public IJsTypeInfo[] ArgumentTypes { get; }
        public IJsTypeInfo MoreArgsType { get; }
        public IJsTypeInfo ResultType { get; }

        public JsMethodSignature(IJsTypeInfo[] arguments, IJsTypeInfo resultType, IJsTypeInfo moreArgs = null)
        {
            this.ArgumentTypes = arguments;
            this.ResultType = resultType;
            this.MoreArgsType = moreArgs;
        }

        public bool MatchSig(JsMethodSignature signature, MatchAntiCycle antiCycle = null)
        {
            if (signature == this) return true;

            antiCycle = antiCycle ?? new MatchAntiCycle();

            if (this.MoreArgsType == null && (signature.MoreArgsType != null || signature.ArgumentTypes.Length > this.ArgumentTypes.Length)) return false;

            if (!ArgumentTypes.Zip(signature.ArgumentTypes, (a, b) => a.MatchType(b, antiCycle)).All(_ => _)) return false;

            if (!signature.ArgumentTypes.Skip(this.ArgumentTypes.Length).All(a => this.MoreArgsType.MatchType(a, antiCycle))) return false;

            if (!this.ArgumentTypes.Skip(signature.ArgumentTypes.Length).All(a => a.MatchType(signature.MoreArgsType, antiCycle))) return false;

            return true;
        }

        public bool MatchArgs(IJsTypeInfo[] arguments, MatchAntiCycle antiCycle = null)
        {
            antiCycle = antiCycle ?? new MatchAntiCycle();
            if (ArgumentTypes.Length < arguments.Length && MoreArgsType == null) return false;
            int i = 0;
            for (; i < arguments.Length; i++)
            {
                if (!(ArgumentTypes.Length >= i ? MoreArgsType : ArgumentTypes[i]).MatchType(arguments[i], antiCycle)) return false;
            }
            for (; i < this.ArgumentTypes.Length; i++)
            {
                if (!this.ArgumentTypes[i].CanBeNull) return false;
            }
            return true;
        }

        public static readonly JsMethodSignature Any = new JsMethodSignature(new IJsTypeInfo[0], JsAnyType.Instance, moreArgs: JsAnyType.Instance);
    }

    public abstract class JsTypeBase : IJsTypeInfo
    {
        private ImmutableArray<object> annotations;
        public bool CanBeNull { get; private set; }

        public abstract string Typeof { get; }

        public JsTypeBase() { }
        public JsTypeBase(bool canBeNull)
        {
            CanBeNull = canBeNull;
        }

        public IJsTypeInfo WithCanBeNull(bool val)
        {
            var r = (JsTypeBase)MemberwiseClone();
            r.CanBeNull = val;
            return r;
        }

        public IJsTypeInfo AddAnnotation(object annotation)
        {
            var clone = (JsTypeBase)MemberwiseClone();
            if (clone.annotations == null) clone.annotations = ImmutableArray.Create(annotation);
            else clone.annotations = clone.annotations.Add(annotation);
            return clone;
        }

        public T Annotation<T>()
        {
            foreach (var item in this.annotations)
            {
                if (item is T) return (T)item;
            }
            return default(T);
        }
    }

    public sealed class JsAnyType : JsTypeBase, IJsMemberAccessType, IJsInvocableType
    {
        public override string Typeof => null;

        public JsPropertyInfo GetProperty(string name)
        {
            return new JsPropertyInfo(name, Instance);
        }

        public IEnumerable<JsMethodSignature> GetSignatures()
        {
            yield return JsMethodSignature.Any;
        }

        public IEnumerable<JsPropertyInfo> ListProperties()
        {
            yield break;
        }

        private JsAnyType() :base(true) { }

        public static readonly JsAnyType Instance = new JsAnyType();
    }

    public sealed class NullOrUndefinedJsType : JsTypeBase
    {
        public readonly bool IsUndefined;
        public bool IsNull => !IsUndefined;

        public override string Typeof => IsUndefined ? "undefined" : "null";

        private NullOrUndefinedJsType(bool undefined) :base(true) { this.IsUndefined = undefined; }

        public override string ToString()
        {
            return Typeof;
        }

        public static readonly NullOrUndefinedJsType Null = new NullOrUndefinedJsType(false);
        public static readonly NullOrUndefinedJsType Undefined = new NullOrUndefinedJsType(true);
    }

    public class JsObjectType : JsTypeBase, IJsMemberAccessType
    {
        public override string Typeof { get; }

        Lazy<Dictionary<string, JsPropertyInfo>> properties;
        public JsObjectType(IEnumerable<JsPropertyInfo> properties, string type = "object")
        {
            this.properties = new Lazy<Dictionary<string, JsPropertyInfo>>(() => properties?.ToDictionary(k => k.Name) ?? new Dictionary<string, JsPropertyInfo>());
            this.Typeof = type;
        }

        public JsPropertyInfo GetProperty(string name)
        {
            properties.Value.TryGetValue(name, out var r);
            return r;
        }

        public IEnumerable<JsPropertyInfo> ListProperties() => properties.Value.Values;
    }

    public class JsFunctionType : JsObjectType, IJsInvocableType
    {
        private readonly JsMethodSignature[] signatures;

        public JsFunctionType(IEnumerable<JsMethodSignature> signatures, IEnumerable<JsPropertyInfo> properties = null)
            :base(properties, "function")
        {
            this.signatures = signatures.ToArray();
        }

        public static IEnumerable<JsPropertyInfo> GetFunctionProperties(IEnumerable<JsMethodSignature> signatures)
        {
            yield return new JsPropertyInfo("name", JsTypeInfo.String);
            yield return new JsPropertyInfo("toSource", ToSourceFunction);
            yield return new JsPropertyInfo("apply",
                new JsFunctionType(
                    signatures.Select(s => new JsMethodSignature(new IJsTypeInfo[] { JsAnyType.Instance, JsIndexableType.CreateArray(JsAnyType.Instance) }, s.ResultType))));
            yield return new JsPropertyInfo("call",
                new JsFunctionType(
                    signatures.Select(s => new JsMethodSignature(new[] { JsAnyType.Instance }.Concat(s.ArgumentTypes).ToArray(), s.ResultType))));
        }

        public IEnumerable<JsMethodSignature> GetSignatures() => signatures;

        public static readonly JsFunctionType ToSourceFunction = new JsFunctionType(new[] { new JsMethodSignature(new IJsTypeInfo[0], JsTypeInfo.String) });
        public static readonly JsFunctionType ToStringFunction = new JsFunctionType(new[] { new JsMethodSignature(new IJsTypeInfo[0], JsTypeInfo.String) });
    }

    public class JsIndexableType : JsObjectType, IJsIndexerAccess
    {
        public JsIndexableType(IJsTypeInfo elementType, bool integerOnly, IEnumerable<JsPropertyInfo> properties = null, string type = "object") : base(properties, type)
        {
            this.ElementType = elementType;
            this.IntegerOnly = integerOnly;
        }

        public IJsTypeInfo ElementType { get; }
        public bool IntegerOnly { get; }

        public static JsIndexableType CreateArray(IJsTypeInfo elementType, params JsPropertyInfo[] properties) => new JsIndexableType(elementType, true, properties);
    }
}
