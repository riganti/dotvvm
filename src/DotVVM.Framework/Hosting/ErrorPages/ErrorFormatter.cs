using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Runtime.Commands;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class ErrorFormatter
    {
#if DotNetCore
        public ExceptionModel LoadDemystifiedException(Exception exception)
        {
            return LoadException(exception, stackFrameGetter: ex => {
                var rawStackTrace = new StackTrace(ex, true).GetFrames();
                if (rawStackTrace == null) return null; // demystifier throws in these cases
                try
                {
                    return new EnhancedStackTrace(ex).GetFrames();
                }
                catch(Exception bensException)
                {
                    return rawStackTrace;
                }
            });
        }
#endif
        public ExceptionModel LoadException(Exception exception, StackFrameModel[] existingTrace = null, Func<Exception, StackFrame[]> stackFrameGetter = null)
        {
            stackFrameGetter = stackFrameGetter ?? (ex => new StackTrace(ex, true).GetFrames());

            var m = new ExceptionModel();
            m.Message = exception.Message;
            m.OriginalException = exception;
            m.TypeName = exception.GetType().FullName;
            var frames = stackFrameGetter(exception) ?? new StackFrame[0];
            var stack = new List<StackFrameModel>();
            bool skipping = existingTrace != null;
            for (int i = frames.Length - 1; i >= 0; i--)
            {
                var f = frames[i];
                if (skipping && existingTrace.Length > i && f.GetMethod() == existingTrace[i].Method) continue;
                skipping = false;

#if DotNetCore
                var niceMethod = (f as EnhancedStackFrame)?.MethodInfo;
#endif

                stack.Add(AddMoreInfo(new StackFrameModel
                {
                    Method = f.GetMethod(),
#if DotNetCore
                    FormattedMethod = niceMethod?.ToString(),
#endif
                    At = LoadSourcePiece(f.GetFileName(), f.GetFileLineNumber(),
                        errorColumn: f.GetFileColumnNumber())
                }));

                //Adding additional information to ExceptionModel from InfoLoaders and InfoCollectionLoader
                m.AdditionalInfo = InfoLoaders.Select(info => info(exception))
                    .Where(info => info != null && info.Objects != null).ToArray()
                    .Union(InfoCollectionLoader.Select(infoCollection => infoCollection(exception))
                        .Where(infoCollection => infoCollection != null)
                        .SelectMany(infoCollection => infoCollection)
                        .Where(info => info != null && info.Objects != null).ToArray())
                    .ToArray();
            }
            stack.Reverse();
            m.Stack = stack.ToArray();
            if (exception.InnerException != null) m.InnerException = LoadException(exception.InnerException, m.Stack, stackFrameGetter);
            return m;
        }

        private StackFrameModel AddMoreInfo(StackFrameModel frame)
        {
            try
            {
                frame.MoreInfo = FrameInfoLoaders.Select(f => f(frame)).Where(f => f != null).ToArray();
            }
            catch
            {
                frame.MoreInfo = new IFrameMoreInfo[0];
            }

            return frame;
        }

        public List<Func<StackFrameModel, IFrameMoreInfo>> FrameInfoLoaders =
            new List<Func<StackFrameModel, IFrameMoreInfo>>()
            {
                CreateDotvvmDocsLink,
                CreateReferenceSourceLink,
                CreateGithubLink
            };

        protected static IFrameMoreInfo CreateDotvvmDocsLink(StackFrameModel frame)
        {
            const string DotvvmThumb = "https://dotvvm.com/Content/assets/ico/favicon.png";
            var type = frame.Method?.DeclaringType;
            if (type == null) return null;
            while (type.DeclaringType != null) type = type.DeclaringType;
            if (type.Namespace == "DotVVM.Framework.Controls")
            {
                const string BuildinControlsDocs = "https://dotvvm.com/docs/controls/builtin/";
                var url = BuildinControlsDocs + type.Name;
                return FrameMoreInfo.CreateThumbLink(url, DotvvmThumb);
            }
            return null;
        }

        protected static IFrameMoreInfo CreateGithubLink(StackFrameModel frame)
        {
            const string GithubUrl = @"https://github.com/riganti/dotvvm/blob/master/src/";
            const string Octocat = @"https://assets-cdn.github.com/favicon.ico";
            if (frame.Method?.DeclaringType?.GetTypeInfo()?.Assembly == typeof(ErrorFormatter).GetTypeInfo().Assembly)
            {
                // dotvvm github
                if (!String.IsNullOrEmpty(frame.At?.FileName))
                {
                    var fileName =
                        frame.At.FileName.Substring(
                            frame.At.FileName.LastIndexOf("DotVVM.Framework", StringComparison.Ordinal));
                    var url = GithubUrl + fileName.Replace('\\', '/').TrimStart('/') + "#L" + frame.At.LineNumber;
                    return FrameMoreInfo.CreateThumbLink(url, Octocat);
                }
                else
                {
                    // guess by method name
                    var fileName = frame.Method.DeclaringType.FullName.Replace("DotVVM.Framework", "")
                        .Replace('.', '/');
                    if (fileName.Contains("+"))
                        fileName = fileName.Remove(fileName.IndexOf('+')); // remove nested class
                    var url = GithubUrl + "DotVVM.Framework" + fileName + ".cs";
                    return FrameMoreInfo.CreateThumbLink(url, Octocat);
                }
            }
            return null;
        }

        static HashSet<string> ReferenceSourceAssemblies = new HashSet<string>
        {
            "mscorlib",
            "PresentationFramework",
            "System.Web",
            "System",
            "System.Windows.Forms",
            "PresentationCore",
            "System.ServiceModel",
            "System.Data",
            "System.Data.Entity",
            "System.Core",
            "System.Xml",
            "System.Activities",
            "WindowsBase",
            "System.Activities.Presentation",
            "System.Drawing",
            "Microsoft.VisualBasic",
            "System.IdentityModel",
            "System.Web.Extensions",
            "System.Runtime.Serialization",
            "System.Workflow.ComponentModel",
            "System.Data.SqlXml",
            "System.Data.Linq",
            "UIAutomationClientsideProviders",
            "PresentationBuildTasks",
            "System.Configuration",
            "System.Management",
            "System.Data.Services",
            "System.Workflow.Activities",
            "System.Management.Automation",
            "Microsoft.CSharp",
            "System.Web.Mobile",
            "System.Web.Services",
            "System.Security",
            "System.Data.Services.Client",
            "System.ServiceModel.Web",
            "System.ServiceModel.Activities",
            "System.Workflow.Runtime",
            "System.ComponentModel.DataAnnotations",
            "System.Activities.Core.Presentation",
            "System.Net.Http",
            "System.Design",
            "Microsoft.Build.Tasks.v4.0",
            "UIAutomationClient",
            "System.Runtime.Remoting",
            "Microsoft.JSc",
            "System.Private.CoreLib"
        };

        protected static IFrameMoreInfo CreateReferenceSourceLink(StackFrameModel frame)
        {
            const string DotNetIcon = "http://referencesource.microsoft.com/favicon.ico";
            const string SourceUrl = "http://referencesource.microsoft.com/";
            if (frame.Method?.DeclaringType?.GetTypeInfo()?.Assembly != null &&
                ReferenceSourceAssemblies.Contains(frame.Method.DeclaringType.GetTypeInfo().Assembly.GetName().Name))
            {
                if (!String.IsNullOrEmpty(frame.At?.FileName))
                {
                    throw new NotImplementedException();
                }
                else
                {
                    if (frame.Method.DeclaringType.GetTypeInfo().IsGenericType)
                    {
                        var url = SourceUrl + "#q=" +
                                  WebUtility.HtmlEncode(
                                      GetGenericFullName(frame.Method.DeclaringType).Replace('+', '.'));
                        return FrameMoreInfo.CreateThumbLink(url, DotNetIcon);
                    }
                    else
                    {
                        var url = SourceUrl + "#q=" +
                                  WebUtility.HtmlEncode(frame.Method.DeclaringType.FullName.Replace('+', '.') + "." +
                                                        frame.Method.Name);
                        return FrameMoreInfo.CreateThumbLink(url, DotNetIcon);
                    }
                }
            }
            return null;
        }

        protected static string GetGenericFullName(Type type)
        {
            if (!type.GetTypeInfo().IsGenericType) return type.FullName;

            var name = type.FullName;
            name = name.Remove(name.IndexOf("`", StringComparison.Ordinal));
            var typeInfo = type.GetTypeInfo();

            StringBuilder sb = new StringBuilder(name);
            sb.Append("<");
            for (int i = 0; i < typeInfo.GenericTypeParameters.Length; i++)
            {
                sb.Append(typeInfo.GenericTypeParameters[i].Name);
                if (i != typeInfo.GenericTypeParameters.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(">");

            return sb.ToString();
        }

        public List<Func<Exception, IEnumerable<ExceptionAdditionalInfo>>> InfoCollectionLoader =
            new List<Func<Exception, IEnumerable<ExceptionAdditionalInfo>>>();

        public List<Func<Exception, ExceptionAdditionalInfo>> InfoLoaders =
            new List<Func<Exception, ExceptionAdditionalInfo>>();

        public void AddInfoLoader<T>(Func<T, ExceptionAdditionalInfo> func)
            where T : Exception
        {
            InfoLoaders.Add(e =>
            {
                if (e is T) return func((T) e);
                else return null;
            });
        }

        /// <summary>
        /// Adds a function to InfoCollectionLoader that returns a collection of ExceptionAdditionalInfo
        /// </summary>
        /// <typeparam name="T">type of the exception</typeparam>
        /// <param name="func">function that returns a collection of ExceptionAdditionalInfo</param>
        public void AddInfoCollectionLoader<T>(Func<T, IEnumerable<ExceptionAdditionalInfo>> func)
            where T : Exception
        {
            InfoCollectionLoader.Add(e =>
            {
                if (e is T) return func((T) e);
                else return null;
            });
        }

        public static SourceModel LoadSourcePiece(string fileName, int lineNumber,
            int additionalLineCount = 7,
            int errorColumn = 0,
            int errorLength = 0)
        {
            var result = new SourceModel();
            result.FileName = fileName;
            result.LineNumber = lineNumber;
            result.ErrorColumn = errorColumn;
            result.ErrorLength = errorLength;
            if (fileName != null)
            {
                try
                {
                    var lines = File.ReadAllLines(fileName);
                    if (lineNumber >= 0)
                    {
                        result.CurrentLine = lines[Math.Max(0, lineNumber - 1)];
                        result.PreLines = lines.Skip(lineNumber - additionalLineCount)
                            .TakeWhile(l => l != result.CurrentLine).ToArray();
                    }
                    else additionalLineCount = 30;
                    result.PostLines = lines.Skip(lineNumber).Take(additionalLineCount).ToArray();
                    return result;
                }
                catch
                {
                    result.LoadFailed = true;
                }
            }
            return result;
        }

        public List<Func<Exception, IHttpContext, IErrorSectionFormatter>> Formatters =
            new List<Func<Exception, IHttpContext, IErrorSectionFormatter>>();

        public string ErrorHtml(Exception exception, IHttpContext context)
        {
            var template = new ErrorPageTemplate();
            template.Formatters = Formatters.Select(f => f(exception, context))
                .Concat(context.GetEnvironmentTabs()
                    .Select(o => DictionarySection.Create(o.Item1, "env_" + o.Item1.GetHashCode(), o.Item2)))
                .Where(t => t != null).ToArray();
            template.ErrorCode = context.Response.StatusCode;
            template.ErrorDescription = "Unhandled exception occurred";
            template.Summary = exception.GetType().FullName + ": " + LimitLength(exception.Message, 600);

            return template.TransformText();
        }

        public static ErrorFormatter CreateDefault()
        {
            var f = new ErrorFormatter();
            f.Formatters.Add((e, o) => DotvvmMarkupErrorSection.Create(e));
#if DotNetCore
            f.Formatters.Add((e, o) => new ExceptionSectionFormatter(f.LoadDemystifiedException(e)));
#endif
            f.Formatters.Add((e, o) => new ExceptionSectionFormatter(f.LoadException(e), "Raw Stack Trace", "raw_stack_trace"));
            f.Formatters.Add((e, o) => DictionarySection.Create("Cookies", "cookies", o.Request.Cookies));
            f.Formatters.Add((e, o) => DictionarySection.Create("Request Headers", "reqHeaders", o.Request.Headers));
            f.AddInfoLoader<ReflectionTypeLoadException>(e => new ExceptionAdditionalInfo
            {
                Title = "Loader Exceptions",
                Objects = e.LoaderExceptions.Select(lde => lde.GetType().Name + ": " + lde.Message).ToArray(),
                Display = ExceptionAdditionalInfo.DisplayMode.ToString
            });
            f.AddInfoLoader<DotvvmCompilationException>(e =>
            {
                var info = new ExceptionAdditionalInfo()
                {
                    Title = "DotVVM Compiler",
                    Objects = null,
                    Display = ExceptionAdditionalInfo.DisplayMode.ToString
                };
                if (e.Tokens != null && e.Tokens.Any())
                {
                    info.Objects = new object[]
                    {
                        $"Error in '{string.Concat(e.Tokens.Select(t => t.Text))}' at line {e.Tokens.First().LineNumber} in {e.SystemFileName}"
                    };
                }
                return info;
            });

            f.AddInfoCollectionLoader<InvalidCommandInvocationException>(e =>
            {
                if (e.AdditionData == null || !e.AdditionData.Any())
                {
                    return null;
                }
                var infos = new List<ExceptionAdditionalInfo>();
                foreach (var data in e.AdditionData)
                {
                    infos.Add(new ExceptionAdditionalInfo()
                    {
                        Title = data.Key,
                        Objects = data.Value,
                        Display = ExceptionAdditionalInfo.DisplayMode.ToHtmlList
                    });
                }
                return infos;
            });

            return f;
        }

        public string LimitLength(string source, int length, string ending = "...")
        {
            if (length < source.Length)
            {
                return source.Substring(0, length - ending.Length) + ending;
            }
            else
            {
                return source;
            }
        }
    }
}