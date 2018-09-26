//------------------------------------------------------------------------------
// <copyright file="PopupMessage.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Class representing a popup message which has been bashed.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

	/// <summary>
	///   <para>Represents a message box that the pop-up blocker dismissed while running calculations for an Excel workbook on an HPC cluster.</para>
	/// </summary>
    [Serializable]
    public class PopupMessage
    {
        /// <summary>
        /// Text on title bar of popup
        /// </summary>
        private string titleBar;
        
        /// <summary>
        /// Message text in popup
        /// </summary>
        private string messageText;

		/// <summary>
		///   <para>Gets the text in the title bar of the dismissed message box.</para>
		/// </summary>
		/// <value>
		///   <para>A <see cref="System.String" /> that contains the text in the title bar.</para>
		/// </value>
        public string TitleBar
        {         
            get { return this.titleBar; }
            internal set { this.titleBar = value; }
        }

		/// <summary>
		///   <para>Gets the text of the message in the dismissed message box.</para>
		/// </summary>
		/// <value>
		///   <para>A <see cref="System.String" /> that contains the text of the message.</para>
		/// </value>
        public string MessageText
        {
            get { return this.messageText; }
            internal set { this.messageText = value; }
        }

        /// <summary>
        ///   <para>Represent the popup as it's title.</para>
        /// </summary>
        /// <returns>
        ///   <para>Title of popup.</para>
        /// </returns>
        public override string ToString()
        {
            return this.titleBar;
        }
    }
}
