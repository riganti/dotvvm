using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Utils;
using DotVVM.TypeScript.Compiler.Utils;
using DotVVM.TypeScript.Compiler.Utils.Logging;

namespace DotVVM.TypeScript.Compiler.Translators
{
    public class TranslatorsEvidence
    {
        private readonly ILogger _logger;

        public TranslatorsEvidence(ILogger _logger)
        {
            this._logger = _logger;
            TranslatorFactories = new Dictionary<Type, Func<object>>();
        }

        private IDictionary<Type, Func<object>> TranslatorFactories { get; }
        
        public void RegisterTranslator<TInput>(Func<ITranslator<TInput>> translatorFactory)
        {
            _logger.LogDebug("Translators Evidence", $"Registered translator for type: {typeof(TInput).Name}");
            if (TranslatorFactories.ContainsKey(typeof(TInput)))
            {
                throw new InvalidOperationException($"Translator for {typeof(TInput).Name} was already registered.");
            }
            TranslatorFactories.Add(typeof(TInput), translatorFactory);
        }

        public ITranslator<TInput> ResolveTranslator<TInput>(TInput input)
        {
            _logger.LogDebug("Translators Evidence", $"Looking for translator for type: {typeof(TInput).Name}");
            return TranslatorFactories[typeof(TInput)].Invoke() as ITranslator<TInput>;
        }

    }
}
