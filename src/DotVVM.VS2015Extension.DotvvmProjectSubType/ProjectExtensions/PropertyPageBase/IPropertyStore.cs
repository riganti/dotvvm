/****************************** Module Header ******************************\
 * Module Name:  IPropertyStore.cs
 * Project:      CSVSXProjectSubType
 * Copyright (c) Microsoft Corporation.
 * 
 * The IPropertyStore Interface is implemented by the DotvvmPropertyPagePropertyStore
 * Class. 
 * 
 * It is used to store the Properties of a PropertyPage object.
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
	public delegate void StoreChangedDelegate();

	public interface IPropertyStore
	{
        event StoreChangedDelegate StoreChanged;

        void Dispose();

        void Initialize(object[] dataObject);

        void Persist(string propertyName, string propertyValue);

        string PropertyValue(string propertyName);
	}
}
