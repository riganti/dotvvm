using System;
using DotVVM.Framework.Compilation;

namespace DotVVM.Framework.Configuration
{
    public sealed class ViewCompilationConfiguration
    {
        private bool isFrozen = false;
        private ViewCompilationMode mode;
        public ViewCompilationMode Mode
        {
            get => mode;
            set {
                ThrowIfFrozen();
                mode = value;
            }
        }

        private TimeSpan? backgroundCompilationDelay;
        public TimeSpan? BackgroundCompilationDelay
        {
            get => backgroundCompilationDelay;
            set
            {
                ThrowIfFrozen();
                backgroundCompilationDelay = value;
            }
        }

        public void Validate()
        {
            if (BackgroundCompilationDelay.HasValue && Mode == ViewCompilationMode.Lazy)
            {
                throw new Exception($"{nameof(BackgroundCompilationDelay)} is not supported in {nameof(ViewCompilationMode.Lazy)} {nameof(Mode)}.");
            }
        }
        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error(nameof(DotvvmMarkupConfiguration));
        }
    }
}
