using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotVVM.Framework.Binding
{
    public class ViewModelPathComparer
    {
        /// <summary>
        /// Compares two viewModel paths.
        /// </summary>
        public static bool AreEqual(string[] path, string[] otherPath)
        {
            if (path.Length != otherPath.Length) return false;
            for (var i = 0; i < path.Length; i++)
            {
                if (path[i] != otherPath[i]) return false;
            }
            return true;
        }
    }
}