using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Runtime.Tracing;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Testing
{
    public class TestDotvvmRequestContext : IDotvvmRequestContext
    {
        public IHttpContext HttpContext { get; set; }
        public string CsrfToken { get; set; }
        public JObject ReceivedViewModelJson { get; set; }
        public object ViewModel { get; set; }
        public JObject ViewModelJson { get; set; }
        public DotvvmConfiguration Configuration { get; set; }
        public IDotvvmPresenter Presenter { get; set; }
        public RouteBase Route { get; set; }
        public bool IsPostBack { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public ResourceManager ResourceManager { get; set; }
        public ModelState ModelState { get; set; }
        public IQueryCollection Query { get; set; }
        public bool IsCommandExceptionHandled { get; set; }
        public bool IsPageExceptionHandled { get; set; }
        public Exception CommandException { get; set; }
        public bool IsSpaRequest { get; set; }
        public bool IsInPartialRenderingMode { get; set; }
        public string ApplicationHostPath { get; set; }
        public string ResultIdFragment { get; set; }
        public DotvvmView View { get; set; }

        private IServiceProvider _services;

        public IServiceProvider Services
        {
            get => _services ?? Configuration?.ServiceProvider ?? throw new NotSupportedException();
            set => _services = value;
        }

        IHttpContext IDotvvmRequestContext.HttpContext => throw new NotImplementedException();

        string IDotvvmRequestContext.CsrfToken { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        JObject IDotvvmRequestContext.ReceivedViewModelJson { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        object IDotvvmRequestContext.ViewModel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        JObject IDotvvmRequestContext.ViewModelJson { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        DotvvmView IDotvvmRequestContext.View { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        DotvvmConfiguration IDotvvmRequestContext.Configuration => throw new NotImplementedException();

        IDotvvmPresenter IDotvvmRequestContext.Presenter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        RouteBase IDotvvmRequestContext.Route { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IDotvvmRequestContext.IsPostBack { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IDictionary<string, object> IDotvvmRequestContext.Parameters { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        ResourceManager IDotvvmRequestContext.ResourceManager => throw new NotImplementedException();

        ModelState IDotvvmRequestContext.ModelState => throw new NotImplementedException();

        IQueryCollection IDotvvmRequestContext.Query => throw new NotImplementedException();

        bool IDotvvmRequestContext.IsCommandExceptionHandled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IDotvvmRequestContext.IsPageExceptionHandled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Exception IDotvvmRequestContext.CommandException { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        bool IDotvvmRequestContext.IsSpaRequest => throw new NotImplementedException();

        bool IDotvvmRequestContext.IsInPartialRenderingMode => throw new NotImplementedException();

        string IDotvvmRequestContext.ResultIdFragment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IServiceProvider IDotvvmRequestContext.Services => throw new NotImplementedException();

        public CustomResponsePropertiesManager CustomResponseProperties { get; set; } = new CustomResponsePropertiesManager();
    }
}
