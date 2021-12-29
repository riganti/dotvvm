using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class StackFrameModel
    {
        public StackFrameModel(MethodBase? method, string? formattedMethod, SourceModel at, IFrameMoreInfo[]? moreInfo)
        {
            Method = method;
            FormattedMethod = formattedMethod;
            At = at;
            MoreInfo = moreInfo ?? new IFrameMoreInfo[0];
        }

        public MethodBase? Method { get; set; }
        public string? FormattedMethod { get; set; }
        public SourceModel At { get; set; }
        public IFrameMoreInfo[] MoreInfo { get; set; }
    }

    public interface IFrameMoreInfo
    {
        string Link { get; }
        string ContentHtml { get; }
    }

    public class FrameMoreInfo : IFrameMoreInfo
    {
        public string ContentHtml { get; set; }

        public string Link { get; set; }

        public FrameMoreInfo(string link, string content)
        {
            Link = link;
            ContentHtml = content;
        }

        public static FrameMoreInfo CreateThumbLink(string link, string thumbLink)
        {
            return new FrameMoreInfo(link, $"<img width='15px' height='15px' src='{thumbLink}' />");
        }
    }
}
