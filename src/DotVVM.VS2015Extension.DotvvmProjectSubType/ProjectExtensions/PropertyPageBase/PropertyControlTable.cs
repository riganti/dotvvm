/****************************** Module Header ******************************\
 * Module Name:  PropertyControlTable.cs
 * Project:      CSVSXProjectSubType
 * Copyright (c) Microsoft Corporation.
 * 
 * The PropertyControlTable class stores the Control / Property Name KeyValuePairs. 
 * A KeyValuePair contains a Control of a PageView object, and a Property Name of
 * PropertyPage object.
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


using System.Collections.Generic;
using System.Windows.Forms;

namespace DotVVM.VS2015Extension.ProjectExtensions.PropertyPageBase
{
	public class PropertyControlTable
	{

        // With these two dictionaries, it is more quick to find a Control or Property Name. 
        private Dictionary<Control, string> controlNameIndex = new Dictionary<Control, string>();
        private Dictionary<string, Control> propertyNameIndex = new Dictionary<string, Control>();

        /// <summary>
        /// Add a Key Value Pair to the dictionaries.
        /// </summary>
        public void Add(string propertyName, Control control)
        {
            this.controlNameIndex.Add(control, propertyName);
            this.propertyNameIndex.Add(propertyName, control);
        }

        /// <summary>
        /// Get the Control which is mapped to a Property.
        /// </summary>
        public Control GetControlFromPropertyName(string propertyName)
        {
            Control control;
            if (this.propertyNameIndex.TryGetValue(propertyName, out control))
            {
                return control;
            }
            return null;
        }

        /// <summary>
        /// Get all Controls.
        /// </summary>
        public List<Control> GetControls()
        {
            Control[] controlArray = new Control[this.controlNameIndex.Count];
            this.controlNameIndex.Keys.CopyTo(controlArray, 0);
            return new List<Control>(controlArray);
        }

        /// <summary>
        /// Get the Property Name which is mapped to a Control.
        /// </summary>
        public string GetPropertyNameFromControl(Control control)
        {
            string str;
            if (this.controlNameIndex.TryGetValue(control, out str))
            {
                return str;
            }
            return null;
        }

        /// <summary>
        /// Get all Property Names.
        /// </summary>
        public List<string> GetPropertyNames()
        {
            string[] strArray = new string[this.propertyNameIndex.Count];
            this.propertyNameIndex.Keys.CopyTo(strArray, 0);
            return new List<string>(strArray);
        }

        /// <summary>
        /// Remove a Key Value Pair from the dictionaries.
        /// </summary>
        public void Remove(string propertyName, Control control)
        {
            this.controlNameIndex.Remove(control);
            this.propertyNameIndex.Remove(propertyName);
        }

	}
}
