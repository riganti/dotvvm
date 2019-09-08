using System;
using System.Linq;

namespace DotVVM.CommandLine.Core.Arguments
{
    public class Arguments
    {
        private string[] args;

        public Arguments(string[] args)
        {
            this.args = args;
        }

        public string this[int index]
        {
            get { return index < args.Length ? args[index] : null; }
        }

        public bool HasOption(params string[] options)
        {
            return GetOptionIndex(options) >= 0;
        }

        public string GetOptionValue(params string[] options)
        {
            var index = GetOptionIndex(options);
            if (index >= 0)
            {
                return this[index + 1];
            }
            return null;
        }

        private int GetOptionIndex(string[] options)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (options.Contains(args[i], StringComparer.CurrentCultureIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Consume(int argsCount)
        {
            args = args.Skip(argsCount).ToArray();
        }
        public bool Contains(string arg)
        {
            return args.Contains(arg);
        }
    }
}
