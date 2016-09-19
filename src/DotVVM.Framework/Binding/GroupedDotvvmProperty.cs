using DotVVM.Framework.Compilation.ControlTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    public class GroupedDotvvmProperty : DotvvmProperty
    {
        public PropertyGroupDescriptor PropertyGroup { get; private set; }
        public string GroupMemberName { get; private set; }

        private static object registerLock = new object();
        public static GroupedDotvvmProperty Register(PropertyGroupDescriptor group, string name)
        {
            GroupedDotvvmProperty prop;
            lock (registerLock)
            {
                var propname = group.PropertyName + ":" + name;
                if ((prop = DotvvmProperty.ResolveProperty(group.DeclaringType.Name + "." + propname) as GroupedDotvvmProperty) == null)
                {
                    prop = new GroupedDotvvmProperty { PropertyGroup = group, GroupMemberName = name };
                    if (group.PropertyGroupMode == PropertyGroupMode.ValueCollection) prop.IsVirtual = true;
                    DotvvmProperty.Register(propname, group.PropertyType, group.DeclaringType, group.DefaultValue, false, prop, (MemberInfo)group.DescriptorField ?? group.PropertyInfo);
                }
            }
            return prop;
        }
    }
}
