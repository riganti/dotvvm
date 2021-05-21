using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests
{
    public sealed class ExpectedExceptionMessageSubstring : ExpectedExceptionBaseAttribute
    {
        private readonly Type expectedExceptionType;
        private readonly string expectedExceptionMessageSubstring;

        public ExpectedExceptionMessageSubstring(Type expectedExceptionType, string expectedExceptionMessageSubstring)
        {
            this.expectedExceptionType = expectedExceptionType
                ?? throw new ArgumentNullException(nameof(expectedExceptionType));

            this.expectedExceptionMessageSubstring = expectedExceptionMessageSubstring
                ?? throw new ArgumentNullException(nameof(expectedExceptionMessageSubstring));
        }

        protected override void Verify(Exception exception)
        {
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, expectedExceptionType, $"Wrong type of exception was thrown. Expected: {expectedExceptionType.FullName} Actual: {exception.GetType().FullName}");

            if (!exception.Message.Contains(expectedExceptionMessageSubstring))
            {
                throw new AssertFailedException($"Exception message did not contain required text. Message:<{exception.Message}> Required text:<{expectedExceptionMessageSubstring}>.");
            }
        }
    }
}
