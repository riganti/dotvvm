/****************************** Module Header ******************************\
 * Module Name:  DotvvmPropertyPagePropertyStore.cs
 * Project:      CSVSXProjectSubType
 * Copyright (c) Microsoft Corporation.
 * 
 * The DotvvmPropertyPagePropertyStore Class implements the IPropertyStore 
 * Interface. It is used to store the Properties of a PropertyPage object.
 * 
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
using DotVVM.VS2015Extension.ProjectExtensions.PropertyPageBase;
using Microsoft.VisualStudio.Shell.Interop;

namespace DotVVM.VS2015Extension.ProjectExtensions.PropertyPages
{
	public class DotvvmPropertyPagePropertyStore : IDisposable, IPropertyStore
	{
        private bool disposed = false;

		private List<DotvvmPropertyPageProjectFlavorCfg> configs = 
            new List<DotvvmPropertyPageProjectFlavorCfg>();

        public event StoreChangedDelegate StoreChanged;

		#region IPropertyStore Members
		/// <summary>
		/// Use the data passed in to initialize the Properties. 
		/// </summary>
		/// <param name="dataObject">
        /// This is normally only one our configuration object, which means that 
        /// there will be only one elements in configs.
        /// If it is null, we should release it.
        /// </param>
		public void Initialize(object[] dataObjects)
		{
			// If we are editing multiple configuration at once, we may get multiple objects.
			foreach (object dataObject in dataObjects)
			{
				if (dataObject is IVsCfg)
				{
					// This should be our configuration object, so retrive the specific
                    // class so we can access its properties.
                    DotvvmPropertyPageProjectFlavorCfg config = DotvvmPropertyPageProjectFlavorCfg
                        .GetCustomPropertyPageProjectFlavorCfgFromIVsCfg((IVsCfg)dataObject);
                  
                    if (!configs.Contains(config))
                    {
                        configs.Add(config);
                    }						
				}
			}
		}

		/// <summary>
		/// Set the value of the specified property in storage.
		/// </summary>
		/// <param name="propertyName">Name of the property to set.</param>
		/// <param name="propertyValue">Value to set the property to.</param>
		public void Persist(string propertyName, string propertyValue)
		{
			// If the value is null, make it empty.
            if (propertyValue == null)
            {
                propertyValue = String.Empty;
            }

			foreach(DotvvmPropertyPageProjectFlavorCfg config in configs)
			{
				// Set the property
				config[propertyName] = propertyValue;
			}
            if (StoreChanged != null)
            {
                StoreChanged();
            }
		}

		/// <summary>
		/// Retreive the value of the specified property from storage
		/// </summary>
		/// <param name="propertyName">Name of the property to retrieve</param>
		/// <returns></returns>
		public string PropertyValue(string propertyName)
		{
			string value = null;
			if (configs.Count > 0)
				value = configs[0][propertyName];
			foreach (DotvvmPropertyPageProjectFlavorCfg config in configs)
			{
				if (config[propertyName] != value)
				{
					// multiple config with different value for the property
					value = String.Empty;
					break;
				}
			}

			return value;
		}

		#endregion

		#region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Protect from being called multiple times.
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                configs.Clear();
            }
            disposed = true;
        }
		#endregion
	}
}
