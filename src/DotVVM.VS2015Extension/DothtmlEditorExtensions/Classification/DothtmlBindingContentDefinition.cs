using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = DothtmlClassificationTypes.BindingContent)]
    [Name(DothtmlClassificationTypes.BindingContent)]
    [UserVisible(true)]
    [Order(After = Priority.Default)]
    internal sealed class DothtmlBindingContentDefinition : ClassificationFormatDefinition
    {
        [ImportingConstructor]
        public DothtmlBindingContentDefinition(ClassificationColorManager colorManager)
        {
            var style = colorManager.GetColor<DothtmlBindingContentDefinition>();
            BackgroundColor = style.BackgroundColor;
            ForegroundColor = style.ForegroundColor;
            DisplayName = "Dothtml Binding Content";
        }
    }
}