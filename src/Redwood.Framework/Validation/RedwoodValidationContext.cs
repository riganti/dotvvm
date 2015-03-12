using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Validation
{
    public class RedwoodValidationContext
    {
        public ViewModelValidator Validator { get; set; }
        public Stack<string> PathStack { get; set; }
        public Stack<object> ObjectStack { get; set; }
        public List<ViewModelValidationError> Errors { get; set; }
        public HashSet<string> Groups { get; set; }
        public string Path
        {
            get { return PathStack.Peek(); }
        }
        public object Value
        {
            get { return ObjectStack.Peek(); }
        }

        public RedwoodValidationContext(object root, IEnumerable<string> groups)
        {
            PathStack = new Stack<string>();
            ObjectStack = new Stack<object>();
            Errors = new List<ViewModelValidationError>();
            Groups = new HashSet<string>(groups ?? new string[] { "*" });
            PushLevel(root, "");
        }

        /// <summary>
        /// Pushs new nest level to both stack
        /// </summary>
        /// <param name="value">value of property</param>
        /// <param name="name">name of property, collection indices are in square brackets</param>
        public void PushLevel(object value, string name)
        {
            ObjectStack.Push(value);
            if (name.Length > 0 && name[0] == '[')
            {
                // collection index
                PathStack.Push(Path + "()" + name);
            }
            else if (PathStack.Count > 0)
            {
                PathStack.Push(CombinePath(Path, name));
            }
            else PathStack.Push(name);
        }

        public void PushLevel(object value, int index)
        {
            ObjectStack.Push(value);
            PathStack.Push(string.Format("{0}()[{1}]", Path, index));
        }

        public void PopLevel()
        {
            ObjectStack.Pop();
            PathStack.Pop();
        }

        public void AddError(string errorMessage)
        {
            Errors.Add(new ViewModelValidationError
            {
                ErrorMessage = errorMessage,
                PropertyPath = Path
            });
        }

        public bool MatchGroups(string groups)
        {
            if (groups == null) return this.Groups.Contains("*");
            return groups.Split(',').Any(g => this.Groups.Contains(g.Trim()));
        }


        /// <summary>
        /// Combines the path.
        /// </summary>
        private string CombinePath(string prefix, string path)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return path;
            }
            else if (!prefix.EndsWith("]"))
            {
                return prefix + "()." + path;
            }
            else
            {
                return prefix + "." + path;
            }
        }
    }
}
