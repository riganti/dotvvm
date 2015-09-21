using OpenQA.Selenium;
using System;
using System.Runtime.Serialization;

namespace DotVVM.Samples.Tests.Exceptions
{
    [Serializable]
    public class MoreElementsInSequenceException : WebDriverException
    {
        public MoreElementsInSequenceException()
        {
        }

        public MoreElementsInSequenceException(string message) : base(message)
        {
        }

        public MoreElementsInSequenceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MoreElementsInSequenceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}