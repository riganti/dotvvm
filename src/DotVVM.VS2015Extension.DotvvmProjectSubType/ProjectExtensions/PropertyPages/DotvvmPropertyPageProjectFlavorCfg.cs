/****************************** Module Header ******************************\
 * Module Name:  DotvvmPropertyPageProjectFlavorCfg.cs
 * Project:      CSVSXProjectSubType
 * Copyright (c) Microsoft Corporation.
 * 
 * The project subtype configuration object implements IVsProjectFlavorCfg to 
 * give the project subtype access to various configuration interfaces.
 * 
 * The base project asks the project subtype to create an IVsProjectFlavorCfg 
 * object corresponding to each of its (project subtype's) configuration objects.
 * The IVsProjectFlavorCfg objects can then, for example, implement IPersistXMLFragment
 * to manage persistence into the project file. The base project system calls 
 * IPersistXMLFragment methods InitNew, Load and Save as appropriate.
 * 
 * The IVsProjectFlavorCfg object can hold and add a referenced pointer to the 
 * IVsCfg object of the base project.
 * 
 * The IPersistXMLFragment is used to persist non-build related data in free-form XML. 
 * The methods provided by IPersistXMLFragment are called by Visual Studio whenever 
 * Visual Studio needs to persist non-build related data in the project file.
 * http://msdn.microsoft.com/en-us/library/bb166204.aspx
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace DotVVM.VS2015Extension.ProjectExtensions.PropertyPages
{
    [ComVisible(false)]
    internal class DotvvmPropertyPageProjectFlavorCfg : IVsProjectFlavorCfg, IPersistXMLFragment
    {

        // This allow the property page to map a IVsCfg object (the baseConfiguration) 
        // to an actual instance of DotvvmPropertyPageProjectFlavorCfg.
        static Dictionary<IVsCfg, DotvvmPropertyPageProjectFlavorCfg> mapIVsCfgToCustomPropertyPageProjectFlavorCfg =
            new Dictionary<IVsCfg, DotvvmPropertyPageProjectFlavorCfg>();

        internal static DotvvmPropertyPageProjectFlavorCfg GetCustomPropertyPageProjectFlavorCfgFromIVsCfg(IVsCfg configuration)
        {
            if (mapIVsCfgToCustomPropertyPageProjectFlavorCfg.ContainsKey(configuration))
            {
                return (DotvvmPropertyPageProjectFlavorCfg)mapIVsCfgToCustomPropertyPageProjectFlavorCfg[configuration];
            }
            else
            {
                throw new ArgumentOutOfRangeException("Cannot find configuration in mapIVsCfgToSpecializedCfg.");
            }
        }

        // Specify whether this is changed.
        private bool isDirty = false;

        // Store all the Properties of this configuration.
        private Dictionary<string, string> propertiesList = new Dictionary<string, string>();

        // Store the DotvvmPropertyPageProjectFlavor object when this instance is
        // initialized.
        // This field is not used to customize the PropertyPage, but it is useful to
        // customize the debug behavior.
        private IVsHierarchy project;

        // The IVsCfg object of the base project.
        private IVsCfg baseConfiguration;

        // The IVsProjectFlavorCfg object of the inner project subtype. 
        private IVsProjectFlavorCfg innerConfiguration;

        /// <summary>
        /// Get or set a Property.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string this[string propertyName]
        {
            get
            {
                if (propertiesList.ContainsKey(propertyName))
                {
                    return propertiesList[propertyName];
                }
                return String.Empty;
            }
            set
            {
                // Don't do anything if there isn't any real change
                if (this[propertyName] == value)
                {
                    return;
                }

                isDirty = true;
                if (propertiesList.ContainsKey(propertyName))
                {
                    propertiesList.Remove(propertyName);
                }
                propertiesList.Add(propertyName, value);
            }
        }

        /// <summary>
        /// Initialize the DotvvmPropertyPageProjectFlavorCfg instance.
        /// </summary>
        public void Initialize(DotvvmPropertyPageProjectFlavor project, IVsCfg baseConfiguration, IVsProjectFlavorCfg innerConfiguration)
        {
            this.project = project;
            this.baseConfiguration = baseConfiguration;
            this.innerConfiguration = innerConfiguration;
            mapIVsCfgToCustomPropertyPageProjectFlavorCfg.Add(baseConfiguration, this);
        }

        #region IVsProjectFlavorCfg Members

        /// <summary>
        /// Provides access to a configuration interfaces such as IVsBuildableProjectCfg2
        /// or IVsDebuggableProjectCfg.
        /// </summary>
        /// <param name="iidCfg">IID of the interface that is being asked</param>
        /// <param name="ppCfg">Object that implement the interface</param>
        /// <returns>HRESULT</returns>
        public int get_CfgType(ref Guid iidCfg, out IntPtr ppCfg)
        {
            ppCfg = IntPtr.Zero;
            if (this.innerConfiguration != null)
            {
                return this.innerConfiguration.get_CfgType(ref iidCfg, out ppCfg);
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Closes the IVsProjectFlavorCfg object.
        /// </summary>
        /// <returns></returns>
        public int Close()
        {

            mapIVsCfgToCustomPropertyPageProjectFlavorCfg.Remove(this.baseConfiguration);
            int hr = this.innerConfiguration.Close();

            if (this.project != null)
            {
                this.project = null;
            }

            if (this.baseConfiguration != null)
            {
                if (Marshal.IsComObject(this.baseConfiguration))
                {
                    Marshal.ReleaseComObject(this.baseConfiguration);
                }
                this.baseConfiguration = null;
            }

            if (this.innerConfiguration != null)
            {
                if (Marshal.IsComObject(this.innerConfiguration))
                {
                    Marshal.ReleaseComObject(this.innerConfiguration);
                }
                this.innerConfiguration = null;
            }
            return hr;
        }

        #endregion

        #region IPersistXMLFragment Members

        /// <summary>
        /// Implement the InitNew method to initialize the project extension properties
        /// and other build-independent data. This method is called if there is no XML
        /// configuration data present in the project file.
        /// </summary>
        /// <param name="guidFlavor">
        /// GUID of the project subtype.
        /// </param>
        /// <param name="storage"></param>
        /// <returns></returns>
        public int InitNew(ref Guid guidFlavor, uint storage)
        {
            //Return,if it is our guid.
            if (IsMyFlavorGuid(ref guidFlavor))
            {
                return VSConstants.S_OK;
            }

            //Forward the call to inner flavor(s).
            if (this.innerConfiguration != null && this.innerConfiguration is IPersistXMLFragment)
            {
                return ((IPersistXMLFragment)this.innerConfiguration)
                    .InitNew(ref guidFlavor, storage);
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Implement the IsFragmentDirty method to determine whether an XML fragment has 
        /// changed since it was last saved to its current file.
        /// </summary>
        /// <param name="storage">
        /// Storage type of the file in which the XML is persisted. Values are taken
        /// from _PersistStorageType enumeration.
        /// </param>
        /// <param name="pfDirty"></param>
        /// <returns></returns>
        public int IsFragmentDirty(uint storage, out int pfDirty)
        {
            pfDirty = 0;
            switch (storage)
            {
                // Specifies storage file type to project file.
                case (uint)_PersistStorageType.PST_PROJECT_FILE:
                    if (isDirty)
                    {
                        pfDirty |= 1;
                    }
                    break;

                // Specifies storage file type to user file.
                case (uint)_PersistStorageType.PST_USER_FILE:
                    // Do not store anything in the user file.
                    break;
            }

            // Forward the call to inner flavor(s) 
            if (pfDirty == 0 && this.innerConfiguration != null
                && this.innerConfiguration is IPersistXMLFragment)
            {
                return ((IPersistXMLFragment)this.innerConfiguration)
                    .IsFragmentDirty(storage, out pfDirty);
            }
            return VSConstants.S_OK;

        }

        /// <summary>
        /// Implement the Load method to load the XML data from the project file.
        /// </summary>
        /// <param name="guidFlavor">
        /// GUID of the project subtype.
        /// </param>
        /// <param name="storage">
        /// Storage type of the file in which the XML is persisted. Values are taken
        /// from _PersistStorageType enumeration.
        /// </param>
        /// <param name="pszXMLFragment">
        /// String containing the XML fragment.
        /// </param>
        public int Load(ref Guid guidFlavor, uint storage, string pszXMLFragment)
        {
            if (IsMyFlavorGuid(ref guidFlavor))
            {
                switch (storage)
                {
                    case (uint)_PersistStorageType.PST_PROJECT_FILE:
                        // Load our data from the XML fragment.
                        XmlDocument doc = new XmlDocument();
                        XmlNode node = doc.CreateElement(this.GetType().Name);
                        node.InnerXml = pszXMLFragment;
                        if (node == null || node.FirstChild == null)
                            break;

                        // Load all the properties
                        foreach (XmlNode child in node.FirstChild.ChildNodes)
                        {
                            propertiesList.Add(child.Name, child.InnerText);
                        }
                        break;
                    case (uint)_PersistStorageType.PST_USER_FILE:
                        // Do not store anything in the user file.
                        break;
                }
            }

            // Forward the call to inner flavor(s)
            if (this.innerConfiguration != null && this.innerConfiguration is IPersistXMLFragment)
            {
                return ((IPersistXMLFragment)this.innerConfiguration)
                    .Load(ref guidFlavor, storage, pszXMLFragment);
            }

            return VSConstants.S_OK;

        }


        /// <summary>
        /// Implement the Save method to save the XML data in the project file.
        /// </summary>
        /// <param name="guidFlavor">
        /// GUID of the project subtype.
        /// </param>
        /// <param name="storage">
        /// Storage type of the file in which the XML is persisted. Values are taken
        /// from _PersistStorageType enumeration.
        /// </param>
        /// <param name="pszXMLFragment">
        /// String containing the XML fragment.
        /// </param>
        /// <param name="fClearDirty">
        /// Indicates whether to clear the dirty flag after the save is complete. 
        /// If true, the flag should be cleared. If false, the flag should be left 
        /// unchanged.
        /// </param>
        /// <returns></returns>
        public int Save(ref Guid guidFlavor, uint storage, out string pbstrXMLFragment, int fClearDirty)
        {
            pbstrXMLFragment = null;

            if (IsMyFlavorGuid(ref guidFlavor))
            {
                switch (storage)
                {
                    case (uint)_PersistStorageType.PST_PROJECT_FILE:
                        // Create XML for our data (a string and a bool).
                        XmlDocument doc = new XmlDocument();
                        XmlNode root = doc.CreateElement(this.GetType().Name);

                        foreach (KeyValuePair<string, string> property in propertiesList)
                        {
                            XmlNode node = doc.CreateElement(property.Key);
                            node.AppendChild(doc.CreateTextNode(property.Value));
                            root.AppendChild(node);
                        }

                        doc.AppendChild(root);
                        // Get XML fragment representing our data
                        pbstrXMLFragment = doc.InnerXml;

                        if (fClearDirty != 0)
                            isDirty = false;
                        break;
                    case (uint)_PersistStorageType.PST_USER_FILE:
                        // Do not store anything in the user file.
                        break;
                }
            }

            // Forward the call to inner flavor(s)
            if (this.innerConfiguration != null
                && this.innerConfiguration is IPersistXMLFragment)
            {
                return ((IPersistXMLFragment)this.innerConfiguration)
                    .Save(ref guidFlavor, storage, out pbstrXMLFragment, fClearDirty);
            }

            return VSConstants.S_OK;
        }

        #endregion

        private bool IsMyFlavorGuid(ref Guid guidFlavor)
        {
            return guidFlavor.Equals(GuidList.DotvvmPropertyPageProjectFactoryGuid);
        }

    }
}
