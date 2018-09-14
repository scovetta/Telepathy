//------------------------------------------------------------------------------
// <copyright file="CredentialTypeDialog.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      It is a dialog asking for the type of the credential.
//      This source file is shared by StoreAPI and Soa SessionAPI.
// </summary>
//------------------------------------------------------------------------------

using System;
using System.Windows.Forms;

namespace Microsoft.Hpc.Scheduler.Store
{
    internal partial class CredentialTypeDialog : Form
    {
        int _choice = -1;
        internal CredentialTypeDialog(string[] dialogBoxStrings)
        {
            InitializeComponent();

            this.Text = dialogBoxStrings[0];
            label1.Text = dialogBoxStrings[0];
            radioButton1.Text = dialogBoxStrings[1];
            radioButton2.Text = dialogBoxStrings[2];
            okButton.Text = dialogBoxStrings[3];
            cancelButton.Text = dialogBoxStrings[4];
        }


        internal int Choice
        {
            get { return _choice; }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            _choice = 1;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            _choice = 2;
        }

    }
}
