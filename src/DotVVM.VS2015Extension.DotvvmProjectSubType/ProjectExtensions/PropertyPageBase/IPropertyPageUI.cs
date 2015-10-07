/****************************** Module Header ******************************\
 * Module Name:  IPropertyPageUI.cs
 * Project:      CSVSXProjectSubType
 * Copyright (c) Microsoft Corporation.
 * 
 * The IPropertyPageUI Interface is implemented by the PageView Class. It
 * provides the methods to get / set the value of the Controls on a PageView object. 
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

using System.Windows.Forms;

namespace DotVVM.VS2015Extension.ProjectExtensions.PropertyPageBase
{
    public delegate void UserEditCompleteHandler(Control control, string value);

	public interface IPropertyPageUI
	{
		event UserEditCompleteHandler UserEditComplete;

        string GetControlValue(Control control);
        void SetControlValue(Control control, string value);

	}
}
