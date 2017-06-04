using System.Collections;
using System.Collections.Generic;

namespace System.CodeDom.Compiler
{
    public class CompilerErrorCollection : IEnumerable<CompilerError>
    {
        private List<CompilerError> errors = new List<CompilerError>();

        public void Add(CompilerError error)
        {
            errors.Add(error);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<CompilerError> GetEnumerator()
        {
            return errors.GetEnumerator();
        }
    }
}