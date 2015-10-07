/****************************** Module Header ******************************\
 * Module Name:  IPageViewSite.cs
 * Project:      CSVSXProjectSubType
 * Copyright (c) Microsoft Corporation.
 * 
 * The IPageViewSite Interface is implemented by the PropertyPage Class, and 
 * used by the PageView Class (or classes that inherit it). 
 * 
 * Through this interface, the PageView Class can get the property values
 * stored in a PropertyPage object, and then use the values to initialize the 
 * controls on the PageView UI. If the controls are edited by the user, the 
 * PageView Class can notify the PropertyPage instance. 
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


namespace DotVVM.VS2015Extension.ProjectExtensions.PropertyPageBase
{
	public interface IPageViewSite
	{
		void PropertyChanged(string propertyName, string propertyValue);

		string GetValueForProperty(string propertyName);
	}
}
