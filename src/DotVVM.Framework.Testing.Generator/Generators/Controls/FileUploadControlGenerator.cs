using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.Generator;

namespace DotVVM.Framework.Testing.Generator.Generators.Controls
{
    public class FileUploadControlGenerator : SeleniumGenerator<FileUpload>
    {
        private static readonly DotvvmProperty[] nameProperties = { FileUpload.UploadCompletedProperty, FileUpload.UploadButtonTextProperty, FileUpload.UploadedFilesProperty };
        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "FileUploadProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}
