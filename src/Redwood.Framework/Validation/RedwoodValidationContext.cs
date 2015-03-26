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
        public string[] ValidationPath { get; set; }
        public object Root { get; private set; }
        public string[] Path
        {
            get { return PathStack.Reverse().Skip(1).ToArray(); }
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
            this.Root = root;
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
            PathStack.Push(name);
        }

        public void PushLevel(object value, int index)
        {
            ObjectStack.Push(value);
            PathStack.Push(string.Format("[{0}]", index));
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

        public bool ShouldValidate(ValidationRule rule)
        {
            return IsPathPrefix(ValidationPath, Path)
                && MatchGroups(rule.Groups, Groups);
        }

        public static bool MatchGroups(string groupString, ISet<string> activeGroups)
        {
            if (groupString == null) return activeGroups.Contains("*");

            return groupString.Split(',').Any(g => g == "**" || activeGroups.Contains(g.Trim()));
        }

        public static bool IsPathPrefix(string[] prefix, string[] path)
        {
            if (prefix.Length > path.Length)
                return false;

            for (int i = 0; i < prefix.Length; i++)
            {
                if (path[i] != prefix[i]) return false;
            }
            return true;
        }

        public static string CombinePath(string prefix, string path)
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
