/****************************** Module Header ******************************\
 * Module Name:  IPageView.cs
 * Project:      CSVSXProjectSubType
 * Copyright (c) Microsoft Corporation.
 * 
 * This class defines the constants used in this project.
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
    public static class Constants
    {
        #region Following constants are used in IPropertyPage::Show Method.

        public const int SW_SHOW = 5;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_HIDE = 0;

        #endregion

        #region Following constants are used in IPropertyPageSite::OnStatusChange Method.

        /// <summary>
        /// The values in the pages have changed, so the state of the
        /// Apply button should be updated.
        /// </summary>
        public const int PROPPAGESTATUS_DIRTY = 0x1;

        /// <summary>
        /// Now is an appropriate time to apply changes.
        /// </summary>
        public const int PROPPAGESTATUS_VALIDATE = 0x2;

        #endregion
    }
}