using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Classification
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = RwHtmlClassificationTypes.BindingBrace)]
    [Name(RwHtmlClassificationTypes.BindingBrace)]
    [UserVisible(true)] 
    [Order(After = Priority.Default)]
    internal sealed class RwHtmlBindingBraceDefinition : ClassificationFormatDefinition
    {
        [ImportingConstructor]
        public RwHtmlBindingBraceDefinition(ClassificationColorManager colorManager)
        {
            var style = colorManager.GetColor<RwHtmlBindingBraceDefinition>();
            BackgroundColor = style.BackgroundColor;
            ForegroundColor = style.ForegroundColor;
            DisplayName = "RWHTML Binding Brace";
        }
    }
}