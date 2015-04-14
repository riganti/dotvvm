using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Classification
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = RwHtmlClassificationTypes.BindingContent)]
    [Name(RwHtmlClassificationTypes.BindingContent)]
    [UserVisible(true)]
    [Order(After = Priority.Default)]
    internal sealed class RwHtmlBindingContentDefinition : ClassificationFormatDefinition
    {
        [ImportingConstructor]
        public RwHtmlBindingContentDefinition(ClassificationColorManager colorManager)
        {
            var style = colorManager.GetColor<RwHtmlBindingContentDefinition>();
            BackgroundColor = style.BackgroundColor;
            ForegroundColor = style.ForegroundColor;
            DisplayName = "RWHTML Binding Content";
        }
    }
}