using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedControl : ResolvedContentNode, IAbstractControl
    {
        public Dictionary<DotvvmProperty, ResolvedPropertySetter> Properties { get; set; } = new Dictionary<DotvvmProperty, ResolvedPropertySetter>();
        
        public object[] ConstructorParameters { get; set; }

        IEnumerable<IPropertyDescriptor> IAbstractControl.PropertyNames => Properties.Keys;

        public ResolvedControl(ControlResolverMetadata metadata, DothtmlNode node, DataContextStack dataContext)
            : base(metadata, node, dataContext) { }

        public void SetProperty(ResolvedPropertySetter value, bool replace = false)
        {
            string error;
            if (!SetProperty(value, replace, out error)) throw new Exception(error);
        }

        public bool SetProperty(ResolvedPropertySetter value, bool replace, out string error)
        {
            error = null;
            ResolvedPropertySetter oldValue;
            if (!Properties.TryGetValue(value.Property, out oldValue) || replace)
            {
                Properties[value.Property] = value;
            }
            else
            {
                if (!value.Property.MarkupOptions.AllowValueMerging) error = $"Property '{value.Property}' is already set and it's value can't be merged.";
                var merger = (IAttributeValueMerger)Activator.CreateInstance(value.Property.MarkupOptions.AttributeValueMerger);
                var mergedValue = (ResolvedPropertySetter)merger.MergeValues(oldValue, value, out error);
                if (error != null)
                {
                    error = $"Could not merge values using {value.Property.MarkupOptions.AttributeValueMerger.Name}: {error}";
                    return false;
                }
                Properties[mergedValue.Property] = mergedValue;
            }
            value.Parent = this;
            return true;
        }
        
        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitControl(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
            foreach (var prop in Properties.Values)
            {
                prop.Accept(visitor);
            }
            
            base.AcceptChildren(visitor);
        }


        public bool TryGetProperty(IPropertyDescriptor property, out IAbstractPropertySetter value)
        {
            ResolvedPropertySetter result;
            value = null;
            if (!Properties.TryGetValue((DotvvmProperty)property, out result)) return false;
            value = result;
            return true;
        }
    }
}
