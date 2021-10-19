﻿using System;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class PageObjectDefinitionImpl: PageObjectDefinition
    {
        public PageObjectDefinitionImpl(string name, string @namespace)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
