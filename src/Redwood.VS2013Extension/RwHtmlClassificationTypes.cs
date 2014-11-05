using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Redwood.VS2013Extension
{
    internal static class RwHtmlClassificationTypes
    {
        public const string Name = "RWHTML";
        public const string BindingBrace = "RwHtml_BindingBrace";
        public const string BindingContent = "RwHtml_BindingContent";

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(BindingBrace)]
        internal static ClassificationTypeDefinition RwHtmlBindingBraceClassificationTypeDefinition = null;

        [Export(typeof (ClassificationTypeDefinition))] 
        [Name(BindingContent)] 
        internal static ClassificationTypeDefinition RwHtmlBindingContentClassificationTypeDefinition2 = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = RwHtmlClassificationTypes.BindingBrace)]
    [Name(RwHtmlClassificationTypes.BindingBrace)]
    [UserVisible(true)] 
    [Order(After = Priority.Default)]
    internal sealed class RwHtmlBindingBraceDefinition : ClassificationFormatDefinition
    {
        public RwHtmlBindingBraceDefinition()
        {
            BackgroundColor = Colors.Yellow;
            ForegroundColor = Colors.Blue;
            DisplayName = "RWHTML Binding Brace";
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = RwHtmlClassificationTypes.BindingContent)]
    [Name(RwHtmlClassificationTypes.BindingContent)]
    [UserVisible(true)]
    [Order(After = Priority.Default)]
    internal sealed class RwHtmlBindingContentDefinition : ClassificationFormatDefinition
    {
        public RwHtmlBindingContentDefinition()
        {
            BackgroundColor = Color.FromRgb(244, 244, 244);
            ForegroundColor = Colors.Blue;
            DisplayName = "RWHTML Binding Content";
        }
    }
}
