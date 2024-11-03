using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using DotVVM.Framework.Compilation;

namespace DotVVM.Framework.Configuration
{
    public sealed class ViewCompilationConfiguration
    {
        private bool isFrozen = false;
        private ViewCompilationMode mode = ViewCompilationMode.AfterApplicationStart;
        /// <summary>
        /// Gets or sets the mode under which the view compilation (pages, controls, ... ) is done. By default, view are precompiled asynchronously after the application starts.
        /// </summary>
        [JsonPropertyName("mode")]
        [DefaultValue(ViewCompilationMode.AfterApplicationStart)]
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
        [JsonPropertyName("backgroundCompilationDelay")]
        public TimeSpan? BackgroundCompilationDelay
        {
            get => backgroundCompilationDelay;
            set
            {
                ThrowIfFrozen();
                backgroundCompilationDelay = value;
            }
        }
        private bool compileInParallel = true;
        /// <summary>
        /// Gets or sets whether the view compilation will be performed in parallel or in series.
        /// </summary>
        [JsonPropertyName("compileInParallel")]
        public bool CompileInParallel
        {
            get => compileInParallel;
            set
            {
                ThrowIfFrozen();
                compileInParallel = value;
            }
        }

        private bool precompileEvenInDebug = false;
        /// <summary>
        /// By default, view precompilation is disabled in Debug mode, to make startup time faster. This options controls this behavior.
        /// </summary>
        [JsonPropertyName("precompileEvenInDebug")]
        public bool PrecompileEvenInDebug
        {
            get => precompileEvenInDebug;
            set
            {
                ThrowIfFrozen();
                precompileEvenInDebug = value;
            }
        }

        public void Validate()
        {
            if (BackgroundCompilationDelay.HasValue && (Mode == ViewCompilationMode.Lazy || Mode==ViewCompilationMode.DuringApplicationStart))
            {
                throw new Exception($"{nameof(BackgroundCompilationDelay)} must be null in {nameof(ViewCompilationMode.Lazy)} {nameof(Mode)}.");
            }
        }
        
        public void Freeze()
        {
            Validate();
            this.isFrozen = true;
        }

        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error(nameof(DotvvmMarkupConfiguration));
        }
    }
}
