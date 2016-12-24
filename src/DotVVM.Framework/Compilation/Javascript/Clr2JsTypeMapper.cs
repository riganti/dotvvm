using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class Clr2JsTypeMapper
    {
        // TODO(exyi): cache cleanup
        ConcurrentDictionary<Type, IJsTypeInfo> typeCache = new ConcurrentDictionary<Type, IJsTypeInfo>();

        public IJsTypeInfo GetJsType(Type clrType)
        {
            return typeCache.GetOrAdd(clrType, t => {

                if (clrType == typeof(string)) return JsTypeInfo.String;
                else if (clrType.IsNumericType()) return JsTypeInfo.Number;
                else if (clrType == typeof(bool)) return JsTypeInfo.Boolean;
                else if (clrType.IsDelegate()) {
                    var method = clrType.GetMethod("Invoke");
                    return new JsFunctionType(new[] { GetJsMethod(method) }).AddAnnotation(clrType);
                } else {
                    var methods = t.GetMethods().GroupBy(m => m.Name)
                        .Select(g => new JsPropertyInfo(g.Key, new JsFunctionType(g.Select(GetJsMethod)).AddAnnotation(g.ToArray())));
                    var properties = t.GetProperties().Select(p => new JsPropertyInfo(p.Name, GetJsType(p.PropertyType)));
                    var fields = t.GetFields().Select(p => new JsPropertyInfo(p.Name, GetJsType(p.FieldType)));
                    return new JsObjectType(methods.Concat(properties).Concat(fields)).AddAnnotation(clrType);
                }
            });
        }

        protected JsMethodSignature GetJsMethod(MethodInfo method)
        {
            return new JsMethodSignature(method.GetParameters().Select(p => GetJsType(p.ParameterType)).ToArray(), GetJsType(method.ReturnType));
        }
    }
}
