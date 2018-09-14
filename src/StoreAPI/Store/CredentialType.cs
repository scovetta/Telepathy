//------------------------------------------------------------------------------
// <copyright file="CredentialType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      It is an utility class asking for the type of the credential.
//      This source file is shared by StoreAPI and Soa SessionAPI.
// </summary>
//------------------------------------------------------------------------------

using System;
using System.Windows.Forms;

namespace Microsoft.Hpc.Scheduler.Store
{
    internal static class CredentialType
    {
        internal static readonly int NoChoice = -1;
        internal static readonly int PwdChoice = 1;
        internal static readonly int CertChoice = 2;

        /// <summary>
        /// Choose the type of credential to be used
        /// </summary>
        /// <param name="fConsole"></param>
        /// <param name="hwndParent"></param>
        /// <param name="choice">-1-> no choice1->password 2->cert</param>
        internal static void PromptForCredentialType(bool fConsole, IntPtr hwndParent, ref int choice, string[] dialogBoxStrings)
        {
            if (fConsole)
            {
                choice = -1;
                while (choice == -1)
                {
                    Console.WriteLine(@"Enter '1' to provide a password or '2' to provide an HPC SoftCard:");
                    string input = Console.ReadLine();
                    if (Int32.TryParse(input, out choice))
                    {
                        if (choice == 1 || choice == 2)
                        {
                            break;
                        }
                        else
                        {
                            choice = -1;
                        }
                    }
                    else
                    {
                        choice = -1;
                    }
                }
            }
            else
            {
                if (hwndParent == new IntPtr(-1))
                {
                    throw new System.Security.Authentication.InvalidCredentialException();
                }

                choice = -1;
                CredentialTypeDialog dialog = new CredentialTypeDialog(dialogBoxStrings);
                dialog.Activate();
                DialogResult result = dialog.ShowDialog(new Win32WindowWrapper(hwndParent));                
                if (result == DialogResult.OK)
                {
                    choice = dialog.Choice;
                }
            }
        }
    }

    internal class Win32WindowWrapper : IWin32Window
    {
        IntPtr _hwnd = new IntPtr(-1);

        internal Win32WindowWrapper(IntPtr hwnd)
        {
            _hwnd = hwnd;
        }
        #region IWin32Window Members

        public IntPtr Handle
        {
            get { return _hwnd; }
        }

        #endregion
    }
}