using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
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
                view.Properties.ContainsKey(ReplaceWithProperty))
                view.DothtmlNode.AddError(
                    "Styles.Wrappers, Styles.Append, Styles.Prepend and Styles.ReplaceWith properties can not be applied to the root control.");

            ProcessControlList(view.Content, view);
            base.VisitControl(view);
        }
        public override void VisitControl(ResolvedControl control)
        {
            ProcessControlList(control.Content, control);
            base.VisitControl(control);
        }

        public override void VisitPropertyControl(ResolvedPropertyControl propertyControl)
        {
            var control = propertyControl.Control;
            if (control.Properties.ContainsKey(AppendProperty) ||
                control.Properties.ContainsKey(PrependProperty))
                throw new Exception(
                    $"Styles.Append and Styles.Prepend properties can not be applied to a control in property {propertyControl.Property}.");
            propertyControl.Control = ProcessWrapping(ProcessReplacement(control));
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
                list[i] = ProcessReplacement(list[i]);
                list[i] = ProcessWrapping(list[i]);
            }
            ProcessAppendAndPrepend(list);
            SetParent(list, parent);
        }

        void ProcessAppendAndPrepend(List<ResolvedControl> list)
        {
            var controls = list.ToArray();
            list.Clear();
            foreach (var c in controls)
            {
                if (!c.Properties.TryGetValue(PrependProperty, out var prependSetter))
                {
                    var prepend = ((ResolvedPropertyControlCollection)prependSetter).Controls.ToArray();
                    list.AddRange(prepend);
                }
                list.Add(c);
                if (!c.Properties.TryGetValue(AppendProperty, out var appendSetter))
                {
                    var append = ((ResolvedPropertyControlCollection)appendSetter).Controls.ToArray();
                    list.AddRange(append);
                }
            }
        }
        void SetParent(IEnumerable<ResolvedControl> controls, ResolvedTreeNode parent)
        {
            foreach (var c in controls)
                c.Parent ??= parent;
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
            control.Properties.Remove(ReplaceWithProperty);
            var newControl = ((ResolvedPropertyControl)setter).Control;
            // TODO: copy properties

            // recursively replace if the new control also has the ReplaceWithProperty
            return ProcessReplacement(newControl);
        }
    }
}
