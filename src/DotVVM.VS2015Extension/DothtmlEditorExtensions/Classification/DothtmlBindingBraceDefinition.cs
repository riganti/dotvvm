using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = DothtmlClassificationTypes.BindingBrace)]
    [Name(DothtmlClassificationTypes.BindingBrace)]
    [UserVisible(true)] 
    [Order(After = Priority.Default)]
    internal sealed class DothtmlBindingBraceDefinition : ClassificationFormatDefinition
    {
        [ImportingConstructor]
        public DothtmlBindingBraceDefinition(ClassificationColorManager colorManager)
        {
            var style = colorManager.GetColor<DothtmlBindingBraceDefinition>();
            BackgroundColor = style.BackgroundColor;
            ForegroundColor = style.ForegroundColor;
            DisplayName = "Dothtml Binding Brace";
        }
    }
}