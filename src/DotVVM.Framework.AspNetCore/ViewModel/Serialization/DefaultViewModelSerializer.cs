using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Commands;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Security;
using Microsoft.AspNetCore.Http.Extensions;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DefaultViewModelSerializer : ViewModelSerializerBase
    {
        public DefaultViewModelSerializer(DotvvmConfiguration configuration, IViewModelProtector protector, IViewModelSerializationMapper serializationMapper) : base(configuration, protector, serializationMapper)
        {
        }

        protected override string GetDisplayUrl(IDotvvmRequestContext context)
        {
            return context.HttpContext.Request.GetDisplayUrl();
        }
    }
}
