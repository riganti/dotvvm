using System;
using DotVVM.Framework.Compilation;

namespace DotVVM.Framework.Configuration
{
    public sealed class ViewCompilationConfiguration
    {
        private bool isFrozen = false;
        private ViewCompilationMode mode;
        /// <summary>
        /// Gets or sets the mode under which the view compilation (pages, controls, ... ) is done.
        /// </summary>
        public ViewCompilationMode Mode
        {
            get => mode;
            set {
                ThrowIfFrozen();
                mode = value;
            }
        }

        private TimeSpan? backgroundCompilationDelay;
        /// <summary>
        /// Gets or sets the delay before view compilation will be done. This compilation delay can be set only in precompilation modes.
        /// </summary>
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
