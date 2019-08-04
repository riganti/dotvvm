using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation;
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

            var serializedObject = JsonConvert.SerializeObject(compilationException);

            var deserializedObject = JsonConvert.DeserializeObject<DotvvmCompilationException>(serializedObject);
        }
    }
}
