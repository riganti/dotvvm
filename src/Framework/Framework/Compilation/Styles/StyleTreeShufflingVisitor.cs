using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using static DotVVM.Framework.Controls.Styles;

namespace DotVVM.Framework.Compilation.Styles
{
    public class StyleTreeShufflingVisitor: ResolvedControlTreeVisitor
    {
        IControlResolver controlResolver;

        public StyleTreeShufflingVisitor(IControlResolver controlResolver)
        {
            this.controlResolver = controlResolver;
        }

        public override void VisitView(ResolvedTreeRoot view)
        {
            if (view.Properties.ContainsKey(WrappersProperty) ||
                view.Properties.ContainsKey(AppendProperty) ||
                view.Properties.ContainsKey(PrependProperty) ||
                view.Properties.ContainsKey(ReplaceWithProperty) ||
                view.Properties.ContainsKey(RemoveProperty))
                view.DothtmlNode.AddError(
                    "Styles.Wrappers, Styles.Append, Styles.Prepend, Styles.ReplaceWith, and Styles.Remove properties cannot be applied to the root control.");

            ProcessControlList(view.Content, view);
            base.VisitControl(view);
        }
        public override void VisitControl(ResolvedControl control)
        {
            Debug.Assert(control.Parent != null);
            ProcessControlList(control.Content, control);
            base.VisitControl(control);
            Debug.Assert(control.Parent != null);

            foreach (var (prop, setter) in control.Properties.ToArray())
            {
                // remove empty properties

                var isEmpty = setter switch {
                    ResolvedPropertyControl { Control: var c } => c is null || ShouldRemove(c),
                    ResolvedPropertyControlCollection { Controls: var c } => c.Count == 0,
                    ResolvedPropertyTemplate { Content: var c } => c.Count == 0,
                    _ => false
                };

                if (isEmpty)
                    control.Properties.Remove(prop);
            }
        }

        public override void VisitPropertyControl(ResolvedPropertyControl propertyControl)
        {
            var control = propertyControl.Control;
            if (control is null)
                return;
            if (control.Properties.ContainsKey(AppendProperty) ||
                control.Properties.ContainsKey(PrependProperty))
                throw new Exception(
                    $"Styles.Append and Styles.Prepend properties cannot be applied to a control in property {propertyControl.Property}.");
            propertyControl.Control = ProcessWrapping(ProcessReplacement(control));
            propertyControl.Control.Parent = propertyControl;
            base.VisitPropertyControl(propertyControl);
        }

        public override void VisitPropertyControlCollection(ResolvedPropertyControlCollection propertyControlCollection)
        {
            ProcessControlList(propertyControlCollection.Controls, propertyControlCollection);
            base.VisitPropertyControlCollection(propertyControlCollection);
        }

        public override void VisitPropertyTemplate(ResolvedPropertyTemplate propertyTemplate)
        {
            ProcessControlList(propertyTemplate.Content, propertyTemplate);
            base.VisitPropertyTemplate(propertyTemplate);
        }

        void ProcessControlList(List<ResolvedControl> list, ResolvedTreeNode parent)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = ProcessWrapping(list[i]);
                list[i] = ProcessReplacement(list[i]);
            }
            ProcessAppendAndPrepend(list, parent);
            SetParent(list, parent);
        }

        void ProcessAppendAndPrepend(List<ResolvedControl> list, ResolvedTreeNode parent)
        {
            var controls = list.ToArray();
            list.Clear();
            foreach (var c in controls)
            {
                if (c.Properties.TryGetValue(PrependProperty, out var prependSetter))
                {
                    c.Properties.Remove(PrependProperty);
                    var prepend = ((ResolvedPropertyControlCollection)prependSetter).Controls.ToList();
                    ProcessControlList(prepend, parent);
                    list.AddRange(prepend);
                }
                if (!ShouldRemove(c))
                {
                    list.Add(c);
                }
                if (c.Properties.TryGetValue(AppendProperty, out var appendSetter))
                {
                    c.Properties.Remove(AppendProperty);
                    var append = ((ResolvedPropertyControlCollection)appendSetter).Controls.ToList();
                    ProcessControlList(append, parent);
                    list.AddRange(append);
                }
            }
        }
        void SetParent(IEnumerable<ResolvedControl> controls, ResolvedTreeNode parent)
        {
            foreach (var c in controls)
                c.Parent = parent;
        }

        ResolvedControl ProcessWrapping(ResolvedControl control)
        {
            if (!control.Properties.TryGetValue(WrappersProperty, out var setter))
                return control;
            control.Properties.Remove(WrappersProperty);

            var wrappers = ((ResolvedPropertyControlCollection)setter).Controls.ToArray();
            foreach (var w in wrappers)
            {
                control = WrapControl(control, w);
            }
            return control;
        }

        ResolvedControl WrapControl(ResolvedControl innerControl, ResolvedControl wrapperControl)
        {
            wrapperControl.Parent = innerControl.Parent;
            innerControl.Parent = wrapperControl;
            ResolvedControlHelper.SetContent(wrapperControl, new [] { innerControl }, StyleOverrideOptions.Append);

            // Wrap the wrapper, if it should be wrapped
            return ProcessWrapping(wrapperControl);
        }

        ResolvedControl ProcessReplacement(ResolvedControl control)
        {
            if (!control.Properties.TryGetValue(ReplaceWithProperty, out var setter))
                return control;

            if (ShouldRemove(control))
                return control;

            control.Properties.Remove(ReplaceWithProperty);
            var newControl = ((ResolvedPropertyControl)setter).Control.NotNull();

            // Copy content
            ResolvedControlHelper.SetContent(newControl, control.Content.ToArray(), StyleOverrideOptions.Append);

            // copy properties
            foreach (var p in control.Properties.Values
#if !DotNetCore
                .ToArray()
#endif
                )
            {
                control.Properties.Remove(p.Property);

                // if it was an attached property, we won't translate by name
                if (p.Property.DeclaringType.IsAssignableFrom(control.Metadata.Type))
                {
                    newControl.SetProperty(p, StyleOverrideOptions.Ignore, out _);
                    continue;
                }

                if (p.Property is GroupedDotvvmProperty gProp)
                {
                    var group2 =
                        DotvvmPropertyGroup.GetPropertyGroups(newControl.Metadata.Type)
                        .SingleOrDefault(g => g.Name == gProp.PropertyGroup.Name) ??
                        DotvvmPropertyGroup.GetPropertyGroups(newControl.Metadata.Type)
                        .SingleOrDefault(g => g.Prefixes.Intersect(gProp.PropertyGroup.Prefixes).Any());
                    if (group2 is object)
                    {
                        var prop2 = group2.GetDotvvmProperty(gProp.GroupMemberName);
                        newControl.SetProperty(
                            ResolvedControlHelper.TranslateProperty(prop2, p.GetValue(), p.GetDataContextStack(parentControl: control), null));
                        continue;
                    }
                }
                else
                {
                    var prop2 = DotvvmProperty.ResolveProperty(newControl.Metadata.Type, p.Property.Name);
                    if (prop2 is object)
                    {
                        newControl.SetProperty(
                            ResolvedControlHelper.TranslateProperty(prop2, p.GetValue(), p.GetDataContextStack(parentControl: control), null));
                        continue;
                    }
                }

                // no corresponding property found, we leave it there as an attached property
                newControl.SetProperty(p);
            }

            // recursively replace if the new control also has the ReplaceWithProperty
            return ProcessReplacement(newControl);
        }

        static bool ShouldRemove(ResolvedControl control)
        {
            if (!control.TryGetProperty(RemoveProperty, out var setter))
                return false;

            if (setter is not ResolvedPropertyValue { Value: bool value })
                throw new Exception($"The value of the {RemoveProperty} property must be a constant boolean. No bindings are allowed. Got: {setter}");
            
            return value;
        }
    }
}
