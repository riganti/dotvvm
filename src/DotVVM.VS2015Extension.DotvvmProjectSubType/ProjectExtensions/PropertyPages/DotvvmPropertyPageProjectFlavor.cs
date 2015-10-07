/****************************** Module Header ******************************\
 * Module Name:  DotvvmPropertyPageProjectFlavor.cs
 * Project:      CSVSXProjectSubType
 * Copyright (c) Microsoft Corporation.
 * 
 * A Project SubType, or called ProjectFlavor,  is a flavor of an inner project.
 * The default behavior of all methods is to delegate to the inner project. 
 * For any behavior you want to change, simply handle the request yourself,
 * and delegate to the base class any case you don't want to handle.
 * 
 * In this DotvvmPropertyPageProjectFlavor, we demonstrate 2 features
 * 1. Add our custom Property Page.
 * 2. Remove the default Service Property Page.
 * 
 * By overriding GetProperty method and using propId parameter containing one of 
 * the values of the __VSHPROPID2 enumeration, we can filter, add or remove project
 * properties. 
 * 
 * For example, to add a page to the configuration-dependent property pages, we
 * need to filter configuration-dependent property pages and then add a new page 
 * to the existing list. 
 * 
 * This source is subject to the Microsoft Public License.
 * See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
 * All other rights reserved.
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***************************************************************************/

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using DotVVM.VS2015Extension.DotvvmProjectSubType.ProjectExtensions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;

namespace DotVVM.VS2015Extension.ProjectExtensions.PropertyPages
{

    public class DotvvmPropertyPageProjectFlavor : FlavoredProjectBase, IVsProjectFlavorCfgProvider
    {

        private Icon dothtmlIcon;
        private Icon dotmasterIcon;
        private Icon dotcontrolIcon;

        // Internal so that our config can have easy access to it. 
        // With this package, we can get access to VS services.
        internal DotvvmSubTypePackage Package { get; set; }
    
        // The IVsProjectFlavorCfgProvider of the inner project.
        // Because we are flavoring the base project directly, it is always null.
        protected IVsProjectFlavorCfgProvider innerVsProjectFlavorCfgProvider = null;

        public DotvvmPropertyPageProjectFlavor()
        {
            dothtmlIcon = new Icon(typeof(DotvvmPropertyPageProjectFlavor).Assembly.GetManifestResourceStream("DotVVM.VS2015Extension.DotvvmProjectSubType.Resources.dotHtml256.ico"));
            dotmasterIcon = new Icon(typeof(DotvvmPropertyPageProjectFlavor).Assembly.GetManifestResourceStream("DotVVM.VS2015Extension.DotvvmProjectSubType.Resources.dotMaster256.ico"));
            dotcontrolIcon = new Icon(typeof(DotvvmPropertyPageProjectFlavor).Assembly.GetManifestResourceStream("DotVVM.VS2015Extension.DotvvmProjectSubType.Resources.dotControl256.ico"));
        }

        #region Overriden Methods

        /// <summary>
        /// This is were all QI for interface on the inner object should happen. 
        /// Then set the inner project wait for InitializeForOuter to be called to do
        /// the real initialization
        /// </summary>
        /// <param name="innerIUnknown"></param>
        protected override void SetInnerProject(IntPtr innerIUnknown)
        {
            object objectForIUnknown = null;
            objectForIUnknown = Marshal.GetObjectForIUnknown(innerIUnknown);

            if (base.serviceProvider == null)
            {
                base.serviceProvider = this.Package;
            }

            base.SetInnerProject(innerIUnknown);

            this.innerVsProjectFlavorCfgProvider = objectForIUnknown as IVsProjectFlavorCfgProvider;
        }

        /// <summary>
        /// Release the innerVsProjectFlavorCfgProvider when closed.
        /// </summary>
        protected override void Close()
        {
            base.Close();
            if (innerVsProjectFlavorCfgProvider != null)
            {
                if (Marshal.IsComObject(innerVsProjectFlavorCfgProvider))
                {
                    Marshal.ReleaseComObject(innerVsProjectFlavorCfgProvider);
                }
                innerVsProjectFlavorCfgProvider = null;
            }
        }

        /// <summary>
        ///  By overriding GetProperty method and using propId parameter containing one of 
        ///  the values of the __VSHPROPID2 enumeration, we can filter, add or remove project
        ///  properties. 
        ///  
        ///  For example, to add a page to the configuration-dependent property pages, we
        ///  need to filter configuration-dependent property pages and then add a new page 
        ///  to the existing list. 
        /// </summary>
        protected override int GetProperty(uint itemId, int propId, out object property)
        {
            // Provide custom icons for DotVVM files
            if (propId == (int) __VSHPROPID.VSHPROPID_IconIndex)
            {
                object name;
                GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_Name, out name);

                if (name is string)
                {
                    if (((string)name).EndsWith(".dothtml", StringComparison.CurrentCultureIgnoreCase))
                    {
                        property = null;
                        return VSConstants.E_NOTIMPL;
                    }
                    if (((string)name).EndsWith(".dotmaster", StringComparison.CurrentCultureIgnoreCase))
                    {
                        property = null;
                        return VSConstants.E_NOTIMPL;
                    }
                    if (((string)name).EndsWith(".dotcontrol", StringComparison.CurrentCultureIgnoreCase))
                    {
                        property = null;
                        return VSConstants.E_NOTIMPL;
                    }
                }
            }
            if (propId == (int)__VSHPROPID.VSHPROPID_IconHandle)
            {
                object name;
                GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_Name, out name);

                if (name is string)
                {
                    if (((string)name).EndsWith(".dothtml", StringComparison.CurrentCultureIgnoreCase))
                    {
                        property = dothtmlIcon.Handle;
                        return VSConstants.S_OK;
                    }
                    if (((string)name).EndsWith(".dotmaster", StringComparison.CurrentCultureIgnoreCase))
                    {
                        property = dotmasterIcon.Handle;
                        return VSConstants.S_OK;
                    }
                    if (((string)name).EndsWith(".dotcontrol", StringComparison.CurrentCultureIgnoreCase))
                    {
                        property = dotcontrolIcon.Handle;
                        return VSConstants.S_OK;
                    }
                }
            }

            return base.GetProperty(itemId, propId, out property);
        }
        
        #endregion

        #region IVsProjectFlavorCfgProvider Members

        /// <summary>
        /// Allows the base project to ask the project subtype to create an 
        /// IVsProjectFlavorCfg object corresponding to each one of its 
        /// (project subtype's) configuration objects.
        /// </summary>
        /// <param name="pBaseProjectCfg">
        /// The IVsCfg object of the base project.
        /// </param>
        /// <param name="ppFlavorCfg">
        /// The IVsProjectFlavorCfg object of the project subtype.
        /// </param>
        /// <returns></returns>
        public int CreateProjectFlavorCfg(IVsCfg pBaseProjectCfg, out IVsProjectFlavorCfg ppFlavorCfg)
        {
            IVsProjectFlavorCfg cfg = null;

            if (innerVsProjectFlavorCfgProvider != null)
            {
                innerVsProjectFlavorCfgProvider.
                    CreateProjectFlavorCfg(pBaseProjectCfg, out cfg);
            }

            var configuration = new DotvvmPropertyPageProjectFlavorCfg();

            configuration.Initialize(this, pBaseProjectCfg, cfg);
            ppFlavorCfg = (IVsProjectFlavorCfg)configuration;

            return VSConstants.S_OK;
        }

        #endregion
      
    }
}
