using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Storage
{
    public class FileSize
    {
        public long Bytes { get; set; }

        public string FormattedText => TextUtils.FormatSize(Bytes);

        public override string ToString()
        {
            return FormattedText;
        }
    }
}