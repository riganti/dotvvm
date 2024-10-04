using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.ViewModel.Serialization;

/// <summary> System.Text.Json converter which supports population of existing objects. Implementations of this interface are also expected to implement <see cref="IDotvvmJsonConverter{T}" /> and inherit from <see cref="JsonConverter{T}" /> </summary>
public interface IDotvvmJsonConverter
{
    public object? ReadUntyped(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, DotvvmSerializationState state);
    public object? PopulateUntyped(ref Utf8JsonReader reader, Type typeToConvert, object? value, JsonSerializerOptions options, DotvvmSerializationState state);
    public void WriteUntyped(Utf8JsonWriter writer, object? value, JsonSerializerOptions options, DotvvmSerializationState state, bool requireTypeField = true, bool wrapObject = true);
}

/// <summary> System.Text.Json converter which supports population of existing objects. </summary>
public interface IDotvvmJsonConverter<T>: IDotvvmJsonConverter
{
    public T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, DotvvmSerializationState state);
    public T Populate(ref Utf8JsonReader reader, Type typeToConvert, T value, JsonSerializerOptions options, DotvvmSerializationState state);
    public void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options, DotvvmSerializationState state, bool requireTypeField = true, bool wrapObject = true);
}

