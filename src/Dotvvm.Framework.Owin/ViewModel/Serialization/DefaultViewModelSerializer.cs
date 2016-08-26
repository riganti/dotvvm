
using System;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DefaultViewModelSerializer : ViewModelSerializerBase
    {
        public DefaultViewModelSerializer(DotvvmConfiguration configuration, IViewModelProtector protector, IViewModelSerializationMapper serializationMapper) : base(configuration, protector, serializationMapper)
        {
        }

        protected override string GetDisplayUrl(IDotvvmRequestContext context)
        {
            return context.OwinContext.Request.Uri.PathAndQuery;
        }
    }
}