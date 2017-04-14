using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;

namespace DotVVM.Framework.Controls
{
    public abstract class DataSource : DotvvmControl
    {
        public static readonly DotvvmProperty DataSourcesProperty =
            DotvvmProperty.Register<ImmutableDictionary<string, Func<IList<IDictionary<string, object>>>>, DataSource>("DataSources",
                ImmutableDictionary<string, Func<IList<IDictionary<string, object>>>>.Empty);

        public class IndexableDS
        {
            private ImmutableDictionary<string, Func<IList<IDictionary<string, object>>>> dict;

            public IndexableDS(DotvvmBindableObject control)
            {
                this.dict = control.GetRoot().GetValue(DataSourcesProperty).CastTo<ImmutableDictionary<string, Func<IList<IDictionary<string, object>>>>>();
            }

            public IList<IDictionary<string, object>> this[string name] => dict[name]();
        }
    }

    [ControlMarkupOptions(AllowContent = false)]
    public class SqlDataSource : DataSource
    {
        [MarkupOptions]
        public string SelectCommand
        {
            get { return (string)GetValue(SelectCommandProperty); }
            set { SetValue(SelectCommandProperty, value); }
        }

        public static readonly DotvvmProperty SelectCommandProperty =
            DotvvmProperty.Register<string, SqlDataSource>(c => c.SelectCommand);

        [MarkupOptions]
        public string ConnectionString
        {
            get { return (string)GetValue(ConnectionStringProperty); }
            set { SetValue(ConnectionStringProperty, value); }
        }

        public static readonly DotvvmProperty ConnectionStringProperty =
            DotvvmProperty.Register<string, SqlDataSource>(c => c.ConnectionString);

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            base.OnLoad(context);
            this.GetRoot().SetValue(DataSourcesProperty, this.GetRoot().GetValue(DataSourcesProperty).CastTo<ImmutableDictionary<string, Func<IList<IDictionary<string, object>>>>>().Add(ID, () => ((IEnumerable<IDictionary<string, object>>)new SqlConnection(ConnectionString).Query(SelectCommand)).ToList()));
        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            var json = JsonConvert.SerializeObject(this.GetRoot().GetValue(DataSourcesProperty).CastTo<ImmutableDictionary<string, Func<IList<IDictionary<string, object>>>>>()[ID](), typeof(IList<IDictionary<string, object>>), new JsonSerializerSettings());
            var js = "+function () { dotvvm.dataSources = dotvvm.dataSources || {}; dotvvm.dataSources[" + JsonConvert.ToString(ID) + "] = dotvvm.dataSources[" + JsonConvert.ToString(ID) + "] || ko.observable(); dotvvm.serialization.deserialize("+json+", dotvvm.dataSources[" + JsonConvert.ToString(ID) + "]); }();";
            context.ResourceManager.AddStartupScript(Guid.NewGuid().ToString(), js, "dotvvm");
            base.OnPreRender(context);
        }

        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
        }
    }
}
