using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Utils;

namespace DotVVM.TypeScript.Compiler.Translators
{
    public class TranslatorsEvidence
    {

        public TranslatorsEvidence()
        {
            TranslatorFactories = new Dictionary<Type, Func<object>>();
        }

        private IDictionary<Type, Func<object>> TranslatorFactories { get; }
        
        public void RegisterTranslator<TInput>(Func<ITranslator<TInput>> translatorFactory)
        {
            if (TranslatorFactories.ContainsKey(typeof(TInput)))
            {
                throw new InvalidOperationException($"Translator for {typeof(TInput).Name} was already registered.");
            }
            TranslatorFactories.Add(typeof(TInput), translatorFactory);
        }

        public ITranslator<TInput> ResolveTranslator<TInput>(TInput input)
        {
            return TranslatorFactories[typeof(TInput)].Invoke() as ITranslator<TInput>;
        }

    }
}
