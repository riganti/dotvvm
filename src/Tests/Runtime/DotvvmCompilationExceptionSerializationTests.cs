using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class DotvvmCompilationExceptionSerializationTests
    {
        [TestMethod]
        public void DotvvmCompilationException_SerializationAndDeserialization_WorksCorrectly()
        {
            var compilationException =
                new DotvvmCompilationException("Compilation error", new Exception("inner exception"));

            var settings = DefaultSerializerSettingsProvider.Instance.Settings;
            var serializedObject = JsonSerializer.Serialize(compilationException, new JsonSerializerOptions(settings) { WriteIndented = true });

            var deserializedObject = JsonSerializer.Deserialize<DotvvmCompilationException>(serializedObject, settings);
        }
    }
}
