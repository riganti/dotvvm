using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    public class FileUpload : HtmlGenericControl
    {

        /// <summary>
        /// Gets or sets whether the user can select multiple files.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool AllowMultipleFiles
        {
            get { return (bool)GetValue(AllowMultipleFilesProperty); }
            set { SetValue(AllowMultipleFilesProperty, value); }
        }
        public static readonly DotvvmProperty AllowMultipleFilesProperty
            = DotvvmProperty.Register<bool, FileUpload>(p => p.AllowMultipleFiles, true);

        /// <summary>
        /// Gets or sets a collection of uploaded files.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public UploadedFilesCollection UploadedFiles
        {
            get { return (UploadedFilesCollection) GetValue(UploadedFilesProperty); }
            set { SetValue(UploadedFilesProperty, value); }
        }
        public static readonly DotvvmProperty UploadedFilesProperty
            = DotvvmProperty.Register<UploadedFilesCollection, FileUpload>(p => p.UploadedFiles, null);


        /// <summary>
        /// Gets or sets a command that is called when the upload is complete.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public Action UploadCompleted
        {
            get { return (Action)GetValue(UploadCompletedProperty); }
            set { SetValue(UploadCompletedProperty, value); }
        }
        public static readonly DotvvmProperty UploadCompletedProperty
            = DotvvmProperty.Register<Action, FileUpload>(p => p.UploadCompleted, null);




        public FileUpload() : base("div")
        {
        }

        internal override void OnPreRenderComplete(IDotvvmRequestContext context)
        {
            EnsureControlHasId();
            context.ResourceManager.AddRequiredResource(Constants.DotvvmFileUploadResourceName);
            context.ResourceManager.AddRequiredResource(Constants.DotvvmFileUploadCssResourceName);

            base.OnPreRenderComplete(context);
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("with", this, UploadedFilesProperty, () =>
            {
                throw new Exception("The UploadedFiles property of the FileUpload control must be bound!");   // TODO: Exception handling
            });
            writer.AddAttribute("class", "dot-upload", true);

            var uploadCompletedBinding = GetCommandBinding(UploadCompletedProperty);
            if (uploadCompletedBinding != null)
            {
                writer.AddAttribute("data-upload-completed", KnockoutHelper.GenerateClientPostBackScript(uploadCompletedBinding, context, this, true, null));
            }

            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            // render iframe
            writer.AddAttribute("class", "dot-upload-iframe");
            writer.AddAttribute("src", "~/" + Constants.FileUploadHandlerMatchUrl + (AllowMultipleFiles ? "?multiple=true" : ""));
            writer.AddAttribute("id", ID + "_iframe");
            writer.AddAttribute("data-target-control-id", ID);
            writer.RenderBeginTag("iframe");
            writer.RenderEndTag();

            // render upload button
            writer.AddAttribute("class", "dot-upload-button");
            writer.AddKnockoutDataBind("visible", "!IsBusy()");
            writer.RenderBeginTag("span");
            writer.AddAttribute("href", "#");
            writer.AddAttribute("onclick", string.Format("dotvvm.fileUpload.showUploadDialog('{0}_iframe'); return false;", ID));
            writer.RenderBeginTag("a");
            writer.WriteUnencodedText("Upload");     // TODO: localization
            writer.RenderEndTag();
            writer.RenderEndTag();

            // render upload files
            writer.AddAttribute("class", "dot-upload-files");
            writer.AddKnockoutDataBind("html", "dotvvm.format('{0} files', Files().length)");     // TODO: localization
            writer.RenderBeginTag("span");
            writer.RenderEndTag();

            // render progress wrapper
            writer.AddKnockoutDataBind("visible", "IsBusy");
            writer.AddAttribute("class", "dot-upload-progress-wrapper");
            writer.RenderBeginTag("span");
            writer.AddAttribute("class", "dot-upload-progress");
            writer.AddKnockoutDataBind("style", "{ 'width': (Progress() == -1 ? '50' : Progress()) + '%' }");
            writer.RenderBeginTag("span");
            writer.RenderEndTag();
            writer.RenderEndTag();

            // render result
            writer.AddAttribute("class", "dot-upload-result");
            writer.AddKnockoutDataBind("html", "Error() ? 'Error occured.' : 'The files are uploaded.'");       // TODO: localization
            writer.AddKnockoutDataBind("attr", "{ title: Error }");
            writer.AddKnockoutDataBind("css", "{ 'dot-upload-result-success': !Error(), 'dot-upload-result-error': Error }");
            writer.AddKnockoutDataBind("visible", "!IsBusy() && Files().length > 0");
            writer.RenderBeginTag("span");
            writer.RenderEndTag();

            base.RenderContents(writer, context);
        }
    }
}
