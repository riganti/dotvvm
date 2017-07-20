using System.Collections.Generic;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Runtime.Commands
{
    public class CandidateBindings
    {
        public List<KeyValuePair<string, IBinding>> Bindings { get; set; }

        public CandidateBindings()
        {
            Bindings = new List<KeyValuePair<string, IBinding>>();
        }

        public void AddBinding(KeyValuePair<string, IBinding> binding)
        {
            Bindings.Add(binding);
        }

        public override string ToString()
        {
            string result = null;
            foreach (var binding in Bindings)
            {
                result = result == null
                    ? $"[{(string.IsNullOrWhiteSpace(binding.Key) ? "" : $"{binding.Key}, ")}{binding.Value}]"
                    : string.Join(";", result, $"[{binding.Key}, {binding.Value}]");
            }
            return result;
        }
    }
}