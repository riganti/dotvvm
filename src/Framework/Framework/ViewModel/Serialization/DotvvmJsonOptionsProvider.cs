using System;
using System.Text.Json;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.ViewModel.Serialization;

/// <summary> Creates and provides System.Text.Json serialization options for ViewModel serialization </summary>
public interface IDotvvmJsonOptionsProvider
{
    /// <summary> Options used for view model serialization, includes the <see cref="ViewModelJsonConverter" /> </summary>
    JsonSerializerOptions ViewModelJsonOptions { get; }
    /// <summary> Options used for serialization of other objects like the ModelState in the invalid VM response. </summary>
    JsonSerializerOptions PlainJsonOptions { get; }

    /// <summary> The the main converter used for viewmodel serialization and deserialization (in initial requests and commands) </summary>
    IDotvvmJsonConverter GetRootViewModelConverter(Type type);
}


public class DotvvmJsonOptionsProvider : IDotvvmJsonOptionsProvider
{
    private Lazy<JsonSerializerOptions> _viewModelOptions;
    public JsonSerializerOptions ViewModelJsonOptions => _viewModelOptions.Value;
    private Lazy<JsonSerializerOptions> _plainJsonOptions;
    public JsonSerializerOptions PlainJsonOptions => _plainJsonOptions.Value;

    private Lazy<ViewModelJsonConverter> _viewModelConverter;

    public DotvvmJsonOptionsProvider(DotvvmConfiguration configuration)
    {
        var debug = configuration.Debug;
        _viewModelConverter = new Lazy<ViewModelJsonConverter>(() => configuration.ServiceProvider.GetRequiredService<ViewModelJsonConverter>());
        _viewModelOptions = new Lazy<JsonSerializerOptions>(() => 
            new JsonSerializerOptions(DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe) {
                Converters = { _viewModelConverter.Value },
                WriteIndented = debug
            }
        );
        _plainJsonOptions = new Lazy<JsonSerializerOptions>(() =>
            !debug ? DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe
                   : new JsonSerializerOptions(DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe) { WriteIndented = true }
        );
    }

    public IDotvvmJsonConverter GetRootViewModelConverter(Type type) => _viewModelConverter.Value.GetDotvvmConverter(type);
}
