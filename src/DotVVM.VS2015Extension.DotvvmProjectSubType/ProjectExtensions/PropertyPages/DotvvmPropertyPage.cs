/****************************** Module Header ******************************\
 * Module Name:  DotvvmPropertyPage.cs
 * Project:      CSVSXProjectSubType
 * Copyright (c) Microsoft Corporation.
 * 
 * The DotvvmPropertyPage Class inherits the PropertyPage Class and overrides 
 * the GetNewPageView and GetNewPropertyStore methods. For more detailed 
 * description, see the PropertyPage Class.
 * 
 * This class will be provided by this Package.
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
using System.Runtime.InteropServices;
using DotVVM.VS2015Extension.ProjectExtensions.PropertyPageBase;

namespace DotVVM.VS2015Extension.ProjectExtensions.PropertyPages
{
    [Guid(GuidList.DotvvmPropertyPage)]
    class DotvvmPropertyPage : PropertyPage
    {

		#region Overriden Properties and Methods
		
        /// <summary>
		/// Help keyword that should be associated with the page
		/// </summary>
		protected override string HelpKeyword
		{
			get { return String.Empty; }
		}

		/// <summary>
		/// Title of the property page.
		/// </summary>
		public override string Title
		{
            get { return "DotVVM"; }
		}

		/// <summary>
		/// Provide the view of our properties.
		/// </summary>
		/// <returns></returns>
		protected override IPageView GetNewPageView()
		{
			return new DotvvmPropertyPageView(this);
		}

		/// <summary>
		/// Use a store implementation designed for flavors.
		/// </summary>
		/// <returns>Store for our properties</returns>
		protected override IPropertyStore GetNewPropertyStore()
		{        
			return new DotvvmPropertyPagePropertyStore();
		}

		#endregion
	}
}