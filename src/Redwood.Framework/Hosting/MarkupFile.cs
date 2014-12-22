using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Parser;

namespace Redwood.Framework.Hosting
{
    public class MarkupFile
    {

        public const string ViewFileExtension = ".rwhtml";



        public Func<IReader> ContentsReaderFactory { get; set; }

        public string FileName { get; set; }

        public string FullPath { get; set; }



        public override int GetHashCode()
        {
            return FileName != null ? StringComparer.CurrentCultureIgnoreCase.GetHashCode(FileName).GetHashCode() : 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((MarkupFile)obj);
        }

        protected bool Equals(MarkupFile other)
        {
            return string.Equals(FileName, other.FileName, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}