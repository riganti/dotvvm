using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DotVVM.Framework.Tests.Common.Runtime
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
            var serializedObject = JsonConvert.SerializeObject(compilationException, settings);

            var deserializedObject = JsonConvert.DeserializeObject<DotvvmCompilationException>(serializedObject, settings);
        }
    }
}
