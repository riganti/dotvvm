/****************************** Module Header ******************************\
 * Module Name:  PropertyControlMap.cs
 * Project:      CSVSXProjectSubType
 * Copyright (c) Microsoft Corporation.
 * 
 * The PropertyControlMap class is used to initialize the Controls on a PageView
 * Object. 
 * 
 * The IPageViewSite Interface is implemented by the PropertyPage class, and 
 * the IPropertyPageUI Interface is implemented by the PageView Class. With the 
 * PropertyControlTable object, the PropertyControlMap object could get a Property
 * value from a PropertyPage object, and use it to initialize the related Control
 * on the PageView object. 
 * 
 * It provides the main UI features of a PageView object. Through this interface, 
 * the PropertyPage object can show / hide / move a PageView object.
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
	public class PropertyControlMap
	{

        // The IPageViewSite Interface is implemented by the PropertyPage class.
        private IPageViewSite pageViewSite;

        // The IPropertyPageUI Interface is implemented by the PageView Class.
        private IPropertyPageUI propertyPageUI;

        // The PropertyControlTable class stores the Control / Property Name KeyValuePairs. 
        // A KeyValuePair contains a Control of a PageView object, and a Property Name of
        // PropertyPage object.
        private PropertyControlTable propertyControlTable;
        

        public PropertyControlMap(IPageViewSite pageViewSite, 
            IPropertyPageUI propertyPageUI, PropertyControlTable propertyControlTable)
        {
            this.propertyControlTable = propertyControlTable;
            this.pageViewSite = pageViewSite;
            this.propertyPageUI = propertyPageUI;
        }

        /// <summary>
        /// Initialize the Controls on a PageView Object using the Properties of
        /// a PropertyPage object. 
        /// </summary>
        public void InitializeControls()
        {
            this.propertyPageUI.UserEditComplete -= 
                new UserEditCompleteHandler(this.propertyPageUI_UserEditComplete);
            foreach (string str in this.propertyControlTable.GetPropertyNames())
            {
                string valueForProperty = this.pageViewSite.GetValueForProperty(str);
                Control controlFromPropertyName =                
                    this.propertyControlTable.GetControlFromPropertyName(str);
                
                this.propertyPageUI.SetControlValue(
                    controlFromPropertyName, valueForProperty);
            }
            this.propertyPageUI.UserEditComplete +=                
                new UserEditCompleteHandler(this.propertyPageUI_UserEditComplete);
        }

        /// <summary>
        /// Notify the PropertyPage object that a Control value is changed.
        /// </summary>
        private void propertyPageUI_UserEditComplete(Control control, string value)
        {
            string propertyNameFromControl = this.propertyControlTable.GetPropertyNameFromControl(control);
            this.pageViewSite.PropertyChanged(propertyNameFromControl, value);
        }

	}
}
