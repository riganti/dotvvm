#nullable enable

using System.Collections.Generic;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    /// <summary>  </summary>
	[ContainsDotvvmProperties]
    public class Styles
    {
        /// <summary> List of controls that will placed as wrappers to this component (the last will be top-most) </summary> 
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public static CompileTimeOnlyDotvvmProperty WrappersProperty =
            CompileTimeOnlyDotvvmProperty.Register<Styles, IEnumerable<DotvvmBindableObject>>("Wrappers");
        /// <summary> List of controls that will be placed after this control (if possible) </summary> 
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public static CompileTimeOnlyDotvvmProperty AppendProperty =
            CompileTimeOnlyDotvvmProperty.Register<Styles, IEnumerable<DotvvmBindableObject>>("Append");
        /// <summary> List of controls that will be placed before this control in reverse order (if possible) </summary> 
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public static CompileTimeOnlyDotvvmProperty PrependProperty =
            CompileTimeOnlyDotvvmProperty.Register<Styles, IEnumerable<DotvvmBindableObject>>("Prepend");
        /// <summary> A control which will be used instead of the specified control. Properties will be translated to the properties of the new control when possible or added as attached properties when not directly translatable. </summary> 
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public static CompileTimeOnlyDotvvmProperty ReplaceWithProperty =
            CompileTimeOnlyDotvvmProperty.Register<Styles, DotvvmBindableObject>("ReplaceWith");
        /// <summary> No Server Side Styles will be applied to this control. When set by a style, this will be the last style to be applied to this control (although when the style contains multiple applicators, all of them will be applied) </summary> 
        public static CompileTimeOnlyDotvvmProperty ExcludeProperty =
            CompileTimeOnlyDotvvmProperty.Register<Styles, bool>("Exclude");
        /// <summary> A tag which can be used to selectively style certain controls. </summary> 
        public static CompileTimeOnlyDotvvmProperty TagProperty =
            CompileTimeOnlyDotvvmProperty.Register<Styles, string[]>("Tag");

        /// <summary> Append to this property to add a required resource somewhere to the page. </summary> 
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public static CompileTimeOnlyDotvvmProperty RequiredResourcesProperty =
            CompileTimeOnlyDotvvmProperty.Register<Styles, string[]>("RequiredResources");
    }
}
