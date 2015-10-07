/****************************** Module Header ******************************\
 * Module Name:  IPageView.cs
 * Project:      CSVSXProjectSubType
 * Copyright (c) Microsoft Corporation.
 * 
 * The IPageView Interface is implemented by the PageView Class. 
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

using System;
using System.Drawing;
using System.Windows.Forms;

namespace DotVVM.VS2015Extension.ProjectExtensions.PropertyPageBase
{
	public interface IPageView: IDisposable
	{
        void HideView();

        void Initialize(Control parentControl, Rectangle rectangle);

        void MoveView(Rectangle rectangle);

        int ProcessAccelerator(ref Message message);

        void RefreshPropertyValues();

        void ShowView();

        Size ViewSize { get; }
	}
}
