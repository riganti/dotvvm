using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Routing
{
    public class DefaultRouteParameterTypes : Dictionary<string, IRouteParameterType>
    {
        public DefaultRouteParameterTypes()
        {
            Add("int", new GenericRouteParameterType("-?[0-9]*?", s => int.Parse(s)));
            Add("posint", new GenericRouteParameterType("[0-9]*?", s => int.Parse(s)));
            Add("float", new GenericRouteParameterType("-?[0-9.e]*?", s => float.Parse(s)));
            Add("guid", new GenericRouteParameterType("[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}", s => Guid.Parse(s)));
        }

        /// <summary>
        /// Merge mew parameter types with current.
        /// </summary>
        /// <param name="parameterTypes">new parameter types</param>
        /// <param name="override">If is true, then the new parameter types will override current</param>
        public void Merge(Dictionary<string, IRouteParameterType> parameterTypes, bool @override = true)
        {
            foreach (var parameterType in parameterTypes)
            {
                if (@override)
                    Remove(parameterType.Key);

                if(! ContainsKey(parameterType.Key))
                    Add(parameterType.Key, parameterType.Value);
            }
        }
    }
}