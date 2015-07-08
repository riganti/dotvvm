using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    public interface IResolvedControlTreeVisitor
    {
        void VisitControl(ResolvedControl control);
        void VisitView(ResolvedView view);
        void VisitPropertyValue(ResolvedPropertyValue propertyValue);
        void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding);
        void VisitPropertyTemplate(ResolvedPropertyTemplate propertyTemplate);
        void VisitPropertyControlCollection(ResolvedPropertyControlCollection propertyControlCollection);
        void VisitPropertyControl(ResolvedPropertyControl propertyControl);
    }
}
