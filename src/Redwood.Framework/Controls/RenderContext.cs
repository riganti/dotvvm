using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Holds context information that are used during the page rendering phase.
    /// </summary>
    public class RenderContext
    {

        /// <summary>
        /// Gets or sets the serialized view model.
        /// </summary>
        public string SerializedViewModel { get; set; }

        /// <summary>
        /// Gets or sets current request context.
        /// </summary>
        public RedwoodRequestContext RedwoodRequestContext { get; set; }


        /// <summary>
        /// Gets or sets the name of the current page area. 
        /// In future this will be used when there are more than one viewmodel on the page. 
        /// </summary>
        public string CurrentPageArea { get; set; }


        /// <summary>
        /// Gets or sets the stack of path fragments that represent current position in the viewmodel. 
        /// This is needed to render postBack function calls properly.
        /// </summary>
        public Stack<string> PathFragments { get; set; }


        /// <summary>
        /// Gets the instance of the <see cref="RwResourceManager"/> that contains all resources that the page requires.
        /// </summary>
        public RwResourceManager ResourceManager
        {
            get { return RedwoodRequestContext.ResourceManager; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RenderContext"/> class.
        /// </summary>
        public RenderContext(RedwoodRequestContext request)
        {
            CurrentPageArea = "root";
            RedwoodRequestContext = request;
            PathFragments = new Stack<string>();
        }
    }
}