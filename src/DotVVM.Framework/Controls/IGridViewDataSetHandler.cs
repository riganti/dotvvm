#nullable enable
using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Controls
{
    public interface IGridViewDataSetHandler
    {
        bool IsCommandSupported(string commandName);

        void SetCommand(string commandName, DotvvmControl control, DotvvmProperty property, Action<object[]>? parameter = null);
    }
}
