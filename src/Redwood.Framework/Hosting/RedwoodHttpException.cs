using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Hosting
{
    public class RedwoodHttpException : Exception
    {

        public RedwoodHttpException(string message)
            : base(message)
        {

        }

        public RedwoodHttpException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
