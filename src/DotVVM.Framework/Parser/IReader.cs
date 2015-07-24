using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser
{
    public interface IReader : IDisposable
    {


        int Position { get; }

        /// <summary>
        /// Returns the char at the cursor, or Char.Zero, if we are on the end of file.
        /// </summary>
        char Peek();

        /// <summary>
        /// Returns the char at the cursor and advances to the next char, or returns Char.Zero, if we are on the end of file.
        /// </summary>
        char Read();

    }
}
