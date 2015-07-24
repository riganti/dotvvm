using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification
{
    internal static class DothtmlClassificationTypes
    {
        public const string Name = "Dothtml";
        public const string BindingBrace = "Dothtml_BindingBrace";
        public const string BindingContent = "Dothtml_BindingContent";

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(BindingBrace)]
        internal static ClassificationTypeDefinition DothtmlBindingBraceClassificationTypeDefinition = null;

        [Export(typeof (ClassificationTypeDefinition))] 
        [Name(BindingContent)] 
        internal static ClassificationTypeDefinition DothtmlBindingContentClassificationTypeDefinition = null;
    }
} 
