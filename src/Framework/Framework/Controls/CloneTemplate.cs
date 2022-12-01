using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary> DotVVM ITemplate implementation which clones the specified <see cref="Controls" /> into each template instance. </summary>
    public class CloneTemplate : ITemplate
    {
        public CloneTemplate(params DotvvmBindableObject[] controls)
        {
            this.Controls = controls;
        }
        public CloneTemplate(IEnumerable<DotvvmBindableObject> controls)
        {
            this.Controls = controls.ToArray();
        }

        public DotvvmBindableObject[] Controls { get; }
        public void BuildContent(IDotvvmRequestContext context, DotvvmControl container)
        {
            foreach (var x in Controls)
            {
                container.Children.Add((DotvvmControl)x.CloneControl());
            }
        }
    }
}
