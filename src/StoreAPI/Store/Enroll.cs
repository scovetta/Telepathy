using System;
using System.Collections.Generic;

using System.Text;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using System.Security;
using System.Security.Principal;
using System.Security.Permissions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
//using Security.Cryptography;
//using Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32.SafeHandles;
using Microsoft.Hpc;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;


namespace Microsoft.Hpc.Scheduler.Store
{
    public class Enroll
    {
        const UInt32 CRYPT_USER_KEYSET = 0x00001000;
        const UInt32 CRYPT_MACHINE_KEYSET = 0x00000020;

        #region Microsoft.Hpc.Scheduler.SoftCardLogonNative.dll PINVOKE layer



        [DllImport("Microsoft.Hpc.Scheduler.SoftCardLogonNative.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int HPCUtilEnrollToPfx(
            [MarshalAs(UnmanagedType.LPWStr)] String templateName,
            X509CertificateEnrollmentContext enrollmentContext,
            [MarshalAs(UnmanagedType.LPWStr)] String pfxPassword,
            out IntPtr base64EncodedPfxString
            );


        [DllImport("Microsoft.Hpc.Scheduler.SoftCardLogonNative.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int HPCUtilEnrollToCertificate(
            [MarshalAs(UnmanagedType.LPWStr)] String templateName,
            X509CertificateEnrollmentContext enrollmentContext,
            out IntPtr base64EncodedCertificate
            );

        [DllImport("Microsoft.Hpc.Scheduler.SoftCardLogonNative.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int HPCUtilFreeBuffer(
            IntPtr buffer
            );



        #endregion
       




        public static String GetTemplateCommonNameUserContext(
           String templateFriendlyName
            )
        {
            return GetTemplateCommonName(templateFriendlyName, X509CertificateEnrollmentContext.ContextUser);
        }


        public static String GetTemplateCommonName(
            String templateFriendlyName,
            X509CertificateEnrollmentContext context)
        {
            String templateCommonName = String.Empty;
            //IX509PolicyServerUrl psUrl = new CX509PolicyServerUrl();
            //psUrl.Initialize(context);

            //   
            // Get templates from AD  
            //              
            IX509EnrollmentPolicyServer adPolicyServer = new CX509EnrollmentPolicyActiveDirectoryClass();
            adPolicyServer.Initialize(
                null,
                null,
                X509EnrollmentAuthFlags.X509AuthKerberos,
                false,
                context
                );

            adPolicyServer.LoadPolicy(X509EnrollmentPolicyLoadOption.LoadOptionReload);
            IX509CertificateTemplates templates = adPolicyServer.GetTemplates();

            foreach (IX509CertificateTemplate t in templates)
            {
                //Console.WriteLine(t.get_Property(EnrollmentTemplateProperty.TemplatePropFriendlyName) + " " + t.get_Property(EnrollmentTemplateProperty.TemplatePropCommonName));

                string tFriendlyName = t[(EnrollmentTemplateProperty.TemplatePropFriendlyName)] as string;
                if (string.Compare(templateFriendlyName,tFriendlyName,true) ==0)
                {

                    templateCommonName = t[(EnrollmentTemplateProperty.TemplatePropCommonName)] as string;
                }
            }

            if (String.IsNullOrEmpty(templateCommonName))
            {
                throw new SchedulerException(ErrorCode.Operation_NoTemplateWithFriendlyName, templateFriendlyName);
            }

            return templateCommonName;
        }

        /// <summary>
        /// Enroll in a certificate for this template and use the provided password 
        /// to encrypt the pfx file for the user        
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static String EnrollToPfxUserContext(
            String templateName,
            String password)
        {
            return EnrollToPfx(templateName, password, X509CertificateEnrollmentContext.ContextUser);
        }


        /// <summary>
        /// Enrolls for a certificate with given template
        /// </summary>
        /// <param name="templateName">Certificate Template Common Name</param>
        /// <param name="enrollmentContext">User, machine, force admin</param>
        /// <param name="pfxPassword">Password to use with PFX</param>
        /// <returns>Base64 Encoded String containing PFX blob</returns>
        public static String EnrollToPfx(
            String templateName,
            String password,
            X509CertificateEnrollmentContext enrollmentContext)
        {
            String pfxString;
            IntPtr ptr = IntPtr.Zero;

            try
            {
                //
                // we have to use unmanaged due to a bug 
                // in the managed CertEnroll which doesn't
                // allow our KSP to be enumerated.
                //
                int ret = HPCUtilEnrollToPfx(
                        templateName,
                        enrollmentContext,
                        password,
                        out ptr);

                if (ret != 0)
                    throw new System.ComponentModel.Win32Exception(ret);

                pfxString = Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    HPCUtilFreeBuffer(ptr);
            }

            return pfxString;
        }

        /// <summary>
        /// Enroll in a certificate for this template and use the provided password 
        /// to encrypt the pfx file for the user        
        /// The encoding is asn1 and returned as a stream of bytes
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Byte[] EnrollToPfxAsn1UserContext(
            String templateName,
            String password)
        {

            return EnrollToPfxAsn1(templateName, password, X509CertificateEnrollmentContext.ContextUser);
        }


        /// <summary>
        /// Enrolls for a certificate with given template
        /// </summary>
        /// <param name="templateName">Certificate Template Common Name</param>
        /// <param name="enrollmentContext">User, machine, force admin</param>
        /// <param name="pfxPassword">Password to use with PFX</param>
        /// <returns>Byte array containing Asn encoded PFX blob</returns>
        public static Byte[] EnrollToPfxAsn1(
            String templateName,
            String password,
            X509CertificateEnrollmentContext enrollmentContext)
        {
            return Convert.FromBase64String(
                EnrollToPfx(
                templateName,
                password,
                enrollmentContext));
        }


        /// <summary>
        /// Enrolls for a certificate with given template, returns X509Cert for this user
        /// 
        /// </summary>
        /// <param name="templateName">Certificate Template Name</param>
        /// <param name="enrollmentContext">User, machine, force admin</param>        
        /// <returns>X509Certificate2</returns>
        public static X509Certificate2 EnrollToCertificateUserContext(
            String templateName)
        {

            return EnrollToCertificate(templateName, X509CertificateEnrollmentContext.ContextUser);
        }


        /// <summary>
        /// Enrolls for a certificate with given template, returns X509Cert
        /// </summary>
        /// <param name="templateName">Certificate Template Name</param>
        /// <param name="enrollmentContext">User, machine, force admin</param>        
        /// <returns>X509Certificate2</returns>
        public static X509Certificate2 EnrollToCertificate(
            String templateName,
            X509CertificateEnrollmentContext enrollmentContext)
        {
            String certString;
            IntPtr ptr = IntPtr.Zero;

            try
            {
                //
                // we have to use unmanaged due to a bug 
                // in the managed CertEnroll which doesn't
                // allow our KSP to be enumerated.
                //
                int ret = HPCUtilEnrollToCertificate(
                        templateName,
                        enrollmentContext,
                        out ptr);

                if (ret != 0)
                    throw new System.ComponentModel.Win32Exception(ret);

                certString = Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    HPCUtilFreeBuffer(ptr);
            }

            X509Certificate2 cert = new X509Certificate2();
            cert.Import(Convert.FromBase64String(certString));

            return cert;
        }

    }      
    
}
