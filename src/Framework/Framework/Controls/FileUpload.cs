using System;
using System.Net;
using System.Text;
using System.Text.Json;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a FileUpload control allowing users to upload one or multiple files asynchronously.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class FileUpload : HtmlGenericControl
    {
        public FileUpload()
            : base("div", false)
        {
        }

        /// <summary>
        /// Gets or sets a collection of uploaded files.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false, Required = true)]
        public UploadedFilesCollection? UploadedFiles
        {
            get { return (UploadedFilesCollection?)GetValue(UploadedFilesProperty); }
            set { SetValue(UploadedFilesProperty, value); }
        }

        public static readonly DotvvmProperty UploadedFilesProperty
            = DotvvmProperty.Register<UploadedFilesCollection?, FileUpload>(p => p.UploadedFiles);

        /// <summary>
        /// Gets or sets whether the user can select multiple files at once. It is enabled by default.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool AllowMultipleFiles
        {
            get { return (bool)GetValue(AllowMultipleFilesProperty)!; }
            set { SetValue(AllowMultipleFilesProperty, value); }
        }
        [AttachedProperty(typeof(bool))]
        public static readonly DotvvmProperty AllowMultipleFilesProperty
            = DotvvmProperty.Register<bool, FileUpload>(p => p.AllowMultipleFiles, true);

        /// <summary>
        /// Gets or sets the types of files that the server accepts. It must be a comma-separated list of unique content type
        /// specifiers (e.g. ".jpg,image/png,audio/*"). All file types are allowed by default.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string? AllowedFileTypes
        {
            get { return GetValue(AllowedFileTypesProperty) as string; }
            set { SetValue(AllowedFileTypesProperty, value); }
        }

        [AttachedProperty(typeof(string))]
        public static readonly DotvvmProperty AllowedFileTypesProperty
            = DotvvmProperty.Register<string?, FileUpload>(p => p.AllowedFileTypes);

        /// <summary>
        /// Gets or sets the <see href="https://developer.mozilla.org/en-US/docs/Web/HTML/Attributes/capture">&lt;input capture="..." attribute</see>.
        /// It specifies that, optionally, a new file should be captured, and which device should be used to capture that new media of a type defined by the <see cref="AllowedFileTypes" /> property.
        /// Allowed values are "user" and "environment" to select if front-facing or rear-facing camera/microphone should be used.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string? Capture
        {
            get { return GetValue(CaptureProperty) as string; }
            set { SetValue(CaptureProperty, value); }
        }

        public static readonly DotvvmProperty CaptureProperty
            = DotvvmProperty.Register<string?, FileUpload>(p => p.Capture);

        /// <summary>
        /// Gets or sets the maximum size of files in megabytes (MB). The size is not limited by default.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public int? MaxFileSize
        {
            get { return GetValue(MaxFileSizeProperty) as int?; }
            set { SetValue(MaxFileSizeProperty, value); }
        }

        [AttachedProperty(typeof(int?))]
        public static readonly DotvvmProperty MaxFileSizeProperty
            = DotvvmProperty.Register<int?, FileUpload>(c => c.MaxFileSize);

        /// <summary>
        /// Gets or sets the text on the upload button. The default value is "Upload".
        /// </summary>
        public string UploadButtonText
        {
            get { return (string)GetValue(UploadButtonTextProperty)!; }
            set { SetValue(UploadButtonTextProperty, value ?? throw new ArgumentNullException(nameof(value))); }
        }

        public static readonly DotvvmProperty UploadButtonTextProperty
            = DotvvmProperty.Register<string, FileUpload>(c => c.UploadButtonText, Resources.Controls.FileUpload_UploadButtonText, true);

        /// <summary>
        /// Gets or sets the text on the indicator showing number of files. The default value is "{0} files". The number of files
        /// will be substituted for the "{0}" placeholder.
        /// </summary>
        public string? NumberOfFilesIndicatorText
        {
            get { return (string?)GetValue(NumberOfFilesIndicatorTextProperty); }
            set { SetValue(NumberOfFilesIndicatorTextProperty, value); }
        }

        public static readonly DotvvmProperty NumberOfFilesIndicatorTextProperty
            = DotvvmProperty.Register<string?, FileUpload>(c => c.NumberOfFilesIndicatorText, Resources.Controls.FileUpload_NumberOfFilesText, true);

        /// <summary>
        /// Gets or sets the text that appears when there is an error during the upload.
        /// </summary>
        public string? UploadErrorMessageText
        {
            get { return (string?)GetValue(UploadErrorMessageTextProperty); }
            set { SetValue(UploadErrorMessageTextProperty, value); }
        }

        public static readonly DotvvmProperty UploadErrorMessageTextProperty
            = DotvvmProperty.Register<string?, FileUpload>(c => c.UploadErrorMessageText, Resources.Controls.FileUpload_UploadErrorMessageText, true);

        /// <summary>
        /// Gets or sets the text that appears when all files are uploaded successfully.
        /// </summary>
        public string? SuccessMessageText
        {
            get { return (string?)GetValue(SuccessMessageTextProperty); }
            set { SetValue(SuccessMessageTextProperty, value); }
        }

        public static readonly DotvvmProperty SuccessMessageTextProperty
            = DotvvmProperty.Register<string?, FileUpload>(c => c.SuccessMessageText, Resources.Controls.FileUpload_SuccessMessageText, true);

        /// <summary>
        /// Gets or sets a command that is triggered when the upload is complete.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public Command? UploadCompleted
        {
            get { return (Command?)GetValue(UploadCompletedProperty); }
            set { SetValue(UploadCompletedProperty, value); }
        }
        [AttachedProperty(typeof(Command))]
        public static readonly DotvvmProperty UploadCompletedProperty
            = DotvvmProperty.Register<Command?, FileUpload>(p => p.UploadCompleted);

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            if (!IsPropertySet(UploadedFilesProperty))
            {
                throw new DotvvmControlException(this, "The UploadedFiles property of the FileUpload cannot be null!");
            }
            context.ResourceManager.AddRequiredResource(ResourceConstants.DotvvmFileUploadCssResourceName);
            base.OnPreRender(context);
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var uploadedFiles = GetValueBinding(UploadedFilesProperty);
            if (uploadedFiles is null)
            {
                throw new DotvvmControlException(this, "The UploadedFiles property of the FileUpload control must be bound using a value binding!");
            }
            writer.AddKnockoutDataBind("with", uploadedFiles, this);
            writer.AddAttribute("class", "dotvvm-upload", true);

            RenderUploadCompletedBinding(this, writer);

            base.AddAttributesToRender(writer, context);
        }

        private static void RenderUploadCompletedBinding(DotvvmControl control, IHtmlWriter writer)
        {
            var uploadCompletedBinding = control.GetCommandBinding(UploadCompletedProperty);
            if (uploadCompletedBinding != null)
            {
                writer.AddAttribute("data-dotvvm-upload-completed", KnockoutHelper.GenerateClientPostBackScript(nameof(UploadCompleted), uploadCompletedBinding, control, useWindowSetTimeout: true, returnValue: null));
            }
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            RenderUploadButton(writer);
            RenderInputControl(writer, context);
            RenderUploadedFilesTitle(writer);
            RenderProgressWrapper(writer);
            RenderResultTitle(writer);

            base.RenderContents(writer, context);
        }

        private void RenderResultTitle(IHtmlWriter writer)
        {
            // render result
            writer.AddAttribute("class", "dotvvm-upload-result");
            writer.AddKnockoutDataBind("html", $"Error() ? {KnockoutHelper.MakeStringLiteral(UploadErrorMessageText!)} : {KnockoutHelper.MakeStringLiteral(SuccessMessageText!)}");
            writer.AddKnockoutDataBind("attr", "{ title: Error }");
            writer.AddKnockoutDataBind("css", "{ 'dotvvm-upload-result-success': !Error(), 'dotvvm-upload-result-error': Error }");
            writer.AddKnockoutDataBind("visible", "!IsBusy() && Files().length > 0");
            writer.RenderBeginTag("span");
            writer.RenderEndTag();
        }

        private static void RenderProgressWrapper(IHtmlWriter writer)
        {
            // render progress wrapper
            writer.AddKnockoutDataBind("visible", "IsBusy");
            writer.AddAttribute("class", "dotvvm-upload-progress-wrapper");
            writer.RenderBeginTag("span");
            writer.AddAttribute("class", "dotvvm-upload-progress");
            writer.AddKnockoutDataBind("style", "{ 'width': (Progress() == -1 ? '50' : Progress()) + '%' }");
            writer.RenderBeginTag("span");
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private void RenderUploadedFilesTitle(IHtmlWriter writer)
        {
            // render upload files
            writer.AddAttribute("class", "dotvvm-upload-files");
            writer.AddKnockoutDataBind("html", $"dotvvm.globalize.format({KnockoutHelper.MakeStringLiteral(NumberOfFilesIndicatorText!)}, Files().length)");
            writer.RenderBeginTag("span");
            writer.RenderEndTag();
        }

        private void RenderUploadButton(IHtmlWriter writer)
        {
            // render upload button
            writer.AddAttribute("class", "dotvvm-upload-button");
            writer.AddKnockoutDataBind("visible", "!IsBusy()");
            writer.RenderBeginTag("span");
            writer.AddAttribute("href", "javascript:;");
            writer.AddAttribute("onclick", "dotvvm.fileUpload.showUploadDialog(this); return false;");
            writer.RenderBeginTag("a");
            writer.WriteText(UploadButtonText);
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private void RenderInputControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.AddStyleAttribute("display", "none");
            writer.AddAttribute("type", "file");

            if (AllowMultipleFiles)
            {
                writer.AddAttribute("multiple", "multiple");
            }

            if (!string.IsNullOrWhiteSpace(AllowedFileTypes))
            {
                writer.AddAttribute("accept", AllowedFileTypes);
            }
            if (!string.IsNullOrWhiteSpace(Capture))
            {
                writer.AddAttribute("capture", Capture);
            }

            writer.AddKnockoutDataBind("dotvvm-FileUpload", JsonSerializer.Serialize(new { url = context.TranslateVirtualPath(GetFileUploadHandlerUrl(this)) }, DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe));
            writer.RenderSelfClosingTag("input");
        }

        private static string GetFileUploadHandlerUrl(DotvvmControl control)
        {
            var builder = new StringBuilder("~/");
            builder.Append(HostingConstants.FileUploadHandlerMatchUrl);
            var delimiter = "?";

            if (control.GetValue(AllowMultipleFilesProperty) as bool? == true)
            {
                builder.AppendFormat("{0}multiple=true", delimiter);
                delimiter = "&";
            }

            var allowedFileTypes = control.GetValue(AllowedFileTypesProperty) as string;
            if (!string.IsNullOrWhiteSpace(allowedFileTypes))
            {
                builder.AppendFormat("{0}fileTypes={1}", delimiter, WebUtility.UrlEncode(allowedFileTypes));
                delimiter = "&";
            }

            var maxFileSize = control.GetValue(MaxFileSizeProperty) as int?;
            if (maxFileSize != null)
            {
                builder.AppendFormat("{0}maxSize={1}", delimiter, maxFileSize);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets or sets the UploadedFilesCollection to which files will be uploaded when pasted or dropped on a control (typically TextBox).
        /// This is an attached property.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        [AttachedProperty(typeof(UploadedFilesCollection))]
        public static readonly DotvvmProperty UploadOnPasteOrDropProperty
            = DelegateActionProperty<UploadedFilesCollection>.Register<FileUpload>("UploadOnPasteOrDrop", RenderUploadOnPasteOrDropProperty);

        private static void RenderUploadOnPasteOrDropProperty(IHtmlWriter writer, IDotvvmRequestContext context, DotvvmProperty property, DotvvmControl control)
        {
            RenderUploadCompletedBinding(control, writer);

            var group = new KnockoutBindingGroup()
            {
                { "url", KnockoutHelper.MakeStringLiteral(context.TranslateVirtualPath(GetFileUploadHandlerUrl(control))) },
                { "collection", control, property },
                { "multiple", control, AllowMultipleFilesProperty }
            };
            writer.AddKnockoutDataBind("dotvvm-FileUpload-UploadOnPasteOrDrop", group);
        }
    }
}
