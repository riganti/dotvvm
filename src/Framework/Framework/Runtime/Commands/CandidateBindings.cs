using System.Collections.Generic;
using System.Linq;
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

        public string[] BindingsToString() =>
            Bindings.Select(b => $"[{(string.IsNullOrWhiteSpace(b.Key) ? "" : $"{b.Key}, ")}{b.Value}]").ToArray();

        public override string ToString() =>
            string.Join("; ", Bindings.Select(b => $"[{(string.IsNullOrWhiteSpace(b.Key) ? "" : $"{b.Key}, ")}{b.Value}]"));
    }
}
