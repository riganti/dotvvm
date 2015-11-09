using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using DotVVM.VS2015Extension.Configuration;
using DotVVM.VS2015Extension.DotvvmPageWizard;
using DotVVM.VS2015Extension.VsPackages;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.JSLS;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Projection
{
    //[Export(typeof(ITextViewModelProvider)),
    //ContentType(ContentTypeDefinitions.DothtmlContentType),
    //TextViewRole(PredefinedTextViewRoles.Document)]
    internal class DothtmlProjectionTextViewModelProvider : ITextViewModelProvider
    {
        private DothtmlProjectionTextViewModel textViewModel;
        private List<ProjectionInfo> projectionInfos;
        private List<ITrackingSpan> allSpans;
        private IComponentModel _componentModel;
        private IVsInvisibleEditorManager _invisibleEditorManager;
        private IVsEditorAdaptersFactoryService _editorAdapter;
        private ITextEditorFactoryService _editorFactoryService;
        private string javascriptFilePath;
        private IVsInvisibleEditor jsEditor;

        [Import]
        public IProjectionBufferFactoryService ProjectionBufferFactoryService { get; set; }

        [Import]
        public IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [ImportMany(typeof(ITaggerProvider))]
        internal ITaggerProvider[] AllTaggerProviders { get; set; }

        public DothtmlProjectionTextViewModelProvider()
        {
            //_componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            //_invisibleEditorManager = (IVsInvisibleEditorManager)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsInvisibleEditorManager));
            //_editorAdapter = _componentModel.GetService<IVsEditorAdaptersFactoryService>();
            //_editorFactoryService = _componentModel.GetService<ITextEditorFactoryService>();
        }

        public ITextViewModel CreateTextViewModel(ITextDataModel dataModel, ITextViewRoleSet roles)
        {
            //var tempFolder = Path.Combine(Path.GetTempPath(), "dotvvmTemp");
            //if (!Directory.Exists(tempFolder))
            //    Directory.CreateDirectory(tempFolder);
            //javascriptFilePath = Path.Combine(tempFolder, Guid.NewGuid().ToString("N") + ".js");
            //File.Create(javascriptFilePath);

            IProjectionBuffer projectionBuffer = ParseDataModelToProjection(dataModel);
            textViewModel = new DothtmlProjectionTextViewModel(dataModel, projectionBuffer);

            return textViewModel;
        }

        private IWpfTextViewHost CreateEditor(string filePath, ITrackingSpan[] spans)
        {
            //IVsInvisibleEditors are in-memory represenations of typical Visual Studio editors.
            //Language services, highlighting and error squiggles are hooked up to these editors
            //for us once we convert them to WpfTextViews.
            var invisibleEditor = GetInvisibleEditor(filePath);

            var docDataPointer = IntPtr.Zero;
            Guid guidIVsTextLines = typeof(IVsTextLines).GUID;

            ErrorHandler.ThrowOnFailure(invisibleEditor.GetDocData(
                fEnsureWritable: 1
                , riid: ref guidIVsTextLines
                , ppDocData: out docDataPointer));

            IVsTextLines docData = (IVsTextLines)Marshal.GetObjectForIUnknown(docDataPointer);

            //Create a code window adapter
            var codeWindow = _editorAdapter.CreateVsCodeWindowAdapter(DotvvmPackage.Instance.OleServiceProvider);
            ErrorHandler.ThrowOnFailure(codeWindow.SetBuffer(docData));

            //Get a text view for our editor which we will then use to get the WPF control for that editor.
            IVsTextView textView;
            ErrorHandler.ThrowOnFailure(codeWindow.GetPrimaryView(out textView));

            //We add our own role to this text view. Later this will allow us to selectively modify
            //this editor without getting in the way of Visual Studio's normal editors.
            var roles = _editorFactoryService.DefaultRoles.Concat(new string[] { "CustomProjectionRole" });

            var vsTextBuffer = docData as IVsTextBuffer;
            var textBuffer = _editorAdapter.GetDataBuffer(vsTextBuffer);

            var guid = VSConstants.VsTextBufferUserDataGuid.VsTextViewRoles_guid;
            ((IVsUserData)codeWindow).SetData(ref guid, _editorFactoryService.CreateTextViewRoleSet(roles).ToString());

            //_currentlyFocusedTextView = textView;
            var textViewHost = _editorAdapter.GetWpfTextViewHost(textView);
            return textViewHost;
        }

        /// <summary>
        /// Creates an invisible editor for a given filePath.
        /// If you're frequently creating projection buffers, it may be worth caching
        /// these editors as they're somewhat expensive to create.
        /// </summary>
        private IVsInvisibleEditor GetInvisibleEditor(string filePath)
        {
            IVsInvisibleEditor invisibleEditor;
            ErrorHandler.ThrowOnFailure(this._invisibleEditorManager.RegisterInvisibleEditor(
                filePath
                , pProject: null
                , dwFlags: (uint)_EDITORREGFLAGS.RIEF_ENABLECACHING
                , pFactory: null
                , ppEditor: out invisibleEditor));

            return invisibleEditor;
        }

        public static List<ProjectionInfo> GetProjectionInfos(ITextSnapshot snapshot)
        {
            List<ProjectionInfo> projections = new List<ProjectionInfo>();
            var scriptRegex = new Regex(@"(?i)(<script[^>]*>(?<JavaScript>[^<]+)</script>)
|(?i)(<dot:InlineScript[^>]*>(?<JavaScript>[^<]+)</dot:InlineScript>)
|(?i)(<dot:InlineTypeScript[^>]*>(?<TypeScript>[^<]+)</dot:InlineTypeScript>)
|(?i)(<dot:InlineStyle[^>]*>(?<css>[^<]+)</dot:InlineStyle>)
|(?i)(<Style[^>]*>(?<css>[^<]+)</Style>)");
            var matches = scriptRegex.Matches(snapshot.GetText());

            foreach (Match match in matches)
            {
                foreach (var groupName in new[] { "JavaScript", "TypeScript", "css" })
                {
                    var group = match.Groups[groupName];
                    if (group.Success)
                    {
                        projections.Add(new ProjectionInfo()
                        {
                            ContentType = groupName,
                            Start = group.Index,
                            End = group.Index + group.Length
                        });
                    }
                }
            }

            return projections;
        }

        private IProjectionBuffer ParseDataModelToProjection(ITextDataModel dataModel)
        {
            dataModel.DataBuffer.Changed += DataBufferOnChanged;

            var projections = new List<ProjectionInfo>();
            var snapshot = dataModel.DataBuffer.CurrentSnapshot;

            projectionInfos = GetProjectionInfos(snapshot);
            projections.AddRange(projectionInfos);

            allSpans = CreateSpansFromProjections(projections, snapshot);
            return CreateProjectionBuffer("projection", allSpans.ToArray());
        }

        private List<ITrackingSpan> CreateSpansFromProjections(List<ProjectionInfo> projections, ITextSnapshot snapshot)
        {
            var spans = new List<ITrackingSpan>();
            var nextLocation = 0;
            foreach (var projectionInfo in projections)
            {
                // generate default projection buffer span
                if (projectionInfo.Start != nextLocation)
                {
                    var preSpan = new Span(nextLocation, projectionInfo.Start - nextLocation);
                    var preTrackingPreSpan = snapshot.CreateTrackingSpan(preSpan, SpanTrackingMode.EdgeExclusive);
                    spans.Add(preTrackingPreSpan);
                }
                // generate separate contentype projection buffer
                var span = new Span(projectionInfo.Start, projectionInfo.End - projectionInfo.Start);
                var trackingSpan = snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);
                var buffer = CreateProjectionBuffer(projectionInfo.ContentType, trackingSpan);
                spans.Add(buffer.CurrentSnapshot.CreateTrackingSpan(new Span(0, buffer.CurrentSnapshot.Length), SpanTrackingMode.EdgeExclusive));
                nextLocation = projectionInfo.End;
            }
            // generate default projection buffer span
            if (nextLocation < snapshot.Length)
            {
                var preSpan = new Span(nextLocation, snapshot.Length - nextLocation);
                spans.Add(snapshot.CreateTrackingSpan(preSpan, SpanTrackingMode.EdgeExclusive));
            }

            return spans;
        }

        private void DataBufferOnChanged(object sender, TextContentChangedEventArgs textContentChangedEventArgs)
        {
#if DEBUG
            var watch = Stopwatch.StartNew();
            try
            {
#endif
                var afterChangeScriptProjectionInfos = GetProjectionInfos(textContentChangedEventArgs.After.TextBuffer.CurrentSnapshot);
                if (projectionInfos.Count == afterChangeScriptProjectionInfos.Count)
                {
                    return;
                }

                projectionInfos = afterChangeScriptProjectionInfos;
                var projectionBuffer = textViewModel.EditBuffer as IProjectionBuffer;
                if (projectionBuffer != null)
                {
                    var newAllSpans = CreateSpansFromProjections(projectionInfos, textViewModel.DataModel.DataBuffer.CurrentSnapshot);

                    for (int i = 0, y = 0; i < allSpans.Count; i++, y++)
                    {
                        var oldSpan = SpanInfo.CreateFromBuffer(allSpans[i]);
                        var newSpan = SpanInfo.CreateFromBuffer(newAllSpans[y]);

                        if (oldSpan.Length > newSpan.Length)
                        {
                            var spans = new List<object> { newSpan.Span };
                            do
                            {
                                newSpan = SpanInfo.CreateFromBuffer(newAllSpans[++y]);
                                spans.Add(newSpan.Span);
                            } while (newSpan.End < oldSpan.End);
                            projectionBuffer.ReplaceSpans(i, 1, spans, EditOptions.None, "ReplaceSpanWithMultipleSpans");
                        }
                        else if (oldSpan.Length < newSpan.Length)
                        {
                            var spans = new List<object> { oldSpan.Span };
                            do
                            {
                                oldSpan = SpanInfo.CreateFromBuffer(allSpans[++i]);
                                spans.Add(oldSpan.Span);
                            } while (newSpan.End > oldSpan.End);
                            projectionBuffer.ReplaceSpans(y, spans.Count, new object[] { newSpan.Span }, EditOptions.None, "ReplaceMultipleSpansWithSpan");
                        }
                    }

                    allSpans = newAllSpans;
                }
#if DEBUG
            }
            finally
            {
                //Console.WriteLine($"{nameof(DataBufferOnChanged)} execution time: {watch.Elapsed}");
                Debug.WriteLine($"{nameof(DataBufferOnChanged)} execution time: {watch.Elapsed}");
            }
#endif
        }

        private IProjectionBuffer CreateProjectionBuffer(string contentType = null, params ITrackingSpan[] spans)
        {
            return contentType != null
                ? ProjectionBufferFactoryService.CreateProjectionBuffer(null,
                    new List<object>(spans),
                    ProjectionBufferOptions.None,
                    ContentTypeRegistryService.GetContentType(contentType))
                : ProjectionBufferFactoryService.CreateProjectionBuffer(null,
                    new List<object>(spans),
                    ProjectionBufferOptions.None);
        }
    }

    internal class ProjectionInfo
    {
        public int Start { get; set; }

        public int End { get; set; }

        public string ContentType { get; set; }
    }

    internal class SpanInfo
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int Length => End - Start;
        public ITrackingSpan Span { get; set; }

        public static SpanInfo CreateFromBuffer(ITrackingSpan span)
        {
            return new SpanInfo()
            {
                Span = span,
                Start = span.GetStartPoint(span.TextBuffer.CurrentSnapshot),
                End = span.GetEndPoint(span.TextBuffer.CurrentSnapshot)
            };
        }
    }
}