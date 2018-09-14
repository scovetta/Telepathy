using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.CustomMarshalers;
using System.Runtime.InteropServices;
using System.Collections;

namespace Microsoft.Hpc.Scheduler.Store
{
    public enum __MIDL___MIDL_itf_certenroll_0000_0069_0001
    {
        LoadOptionCacheOnly = 1,
        LoadOptionDefault = 0,
        LoadOptionRegisterForADChanges = 4,
        LoadOptionReload = 2
    }

    public enum AlgorithmFlags
    {
        AlgorithmFlagsNone,
        AlgorithmFlagsWrap
    }

    public enum AlgorithmOperationFlags
    {
        XCN_NCRYPT_ANY_ASYMMETRIC_OPERATION = 0x1c,
        XCN_NCRYPT_ASYMMETRIC_ENCRYPTION_OPERATION = 4,
        XCN_NCRYPT_CIPHER_OPERATION = 1,
        XCN_NCRYPT_EXACT_MATCH_OPERATION = 0x800000,
        XCN_NCRYPT_HASH_OPERATION = 2,
        XCN_NCRYPT_NO_OPERATION = 0,
        XCN_NCRYPT_PREFER_NON_SIGNATURE_OPERATION = 0x400000,
        XCN_NCRYPT_PREFER_SIGNATURE_ONLY_OPERATION = 0x200000,
        XCN_NCRYPT_PREFERENCE_MASK_OPERATION = 0xe00000,
        XCN_NCRYPT_RNG_OPERATION = 0x20,
        XCN_NCRYPT_SECRET_AGREEMENT_OPERATION = 8,
        XCN_NCRYPT_SIGNATURE_OPERATION = 0x10
    }

    public enum AlgorithmType
    {
        XCN_BCRYPT_UNKNOWN_INTERFACE,
        XCN_BCRYPT_CIPHER_INTERFACE,
        XCN_BCRYPT_HASH_INTERFACE,
        XCN_BCRYPT_ASYMMETRIC_ENCRYPTION_INTERFACE,
        XCN_BCRYPT_SECRET_AGREEMENT_INTERFACE,
        XCN_BCRYPT_SIGNATURE_INTERFACE,
        XCN_BCRYPT_RNG_INTERFACE
    }

    public enum AlternativeNameType
    {
        XCN_CERT_ALT_NAME_DIRECTORY_NAME = 5,
        XCN_CERT_ALT_NAME_DNS_NAME = 3,
        XCN_CERT_ALT_NAME_GUID = 10,
        XCN_CERT_ALT_NAME_IP_ADDRESS = 8,
        XCN_CERT_ALT_NAME_OTHER_NAME = 1,
        XCN_CERT_ALT_NAME_REGISTERED_ID = 9,
        XCN_CERT_ALT_NAME_RFC822_NAME = 2,
        XCN_CERT_ALT_NAME_UNKNOWN = 0,
        XCN_CERT_ALT_NAME_URL = 7,
        XCN_CERT_ALT_NAME_USER_PRINCIPLE_NAME = 11
    }

    [ComImport, CoClass(typeof(CAlternativeNameClass)), Guid("728AB313-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CAlternativeName : IAlternativeName
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E2013-217D-11DA-B2A4-000E7BBB2B09")]
    public class CAlternativeNameClass : IAlternativeName, CAlternativeName
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        public virtual extern void InitializeFromOtherName([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strRawData, [In] bool ToBeWrapped);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeFromRawData([In] AlternativeNameType Type, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strRawData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromString([In] AlternativeNameType Type, [In, MarshalAs(UnmanagedType.BStr)] string strValue);

        // Properties        
        [DispId(0x60020005)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
        [DispId(0x60020004)]
        public virtual extern string strValue { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020003)]
        public virtual extern AlternativeNameType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
    }

    [ComImport, CoClass(typeof(CAlternativeNamesClass)), Guid("728AB314-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CAlternativeNames : IAlternativeNames
    {
    }

    [ComImport, Guid("884E2014-217D-11DA-B2A4-000E7BBB2B09"), ClassInterface((short) 0), TypeLibType((short) 2)]
    public class CAlternativeNamesClass : IAlternativeNames, CAlternativeNames, IEnumerable
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        public virtual extern void Add([In, MarshalAs(UnmanagedType.Interface)] CAlternativeName pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        public virtual extern void Clear();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1), DispId(-4)]
        public virtual extern IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        public virtual extern void Remove([In] int Index);

        // Properties        
        [DispId(1)]
        public virtual extern int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [DispId(0)]
        public virtual extern CAlternativeName this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
    }

    [ComImport, CoClass(typeof(CBinaryConverterClass)), Guid("728AB302-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CBinaryConverter : IBinaryConverter
    {
    }

    [ComImport, TypeLibType((short) 2), Guid("884E2002-217D-11DA-B2A4-000E7BBB2B09"), ClassInterface((short) 0)]
    public class CBinaryConverterClass : IBinaryConverter, CBinaryConverter
    {
        // Methods
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern string StringToString([In, MarshalAs(UnmanagedType.BStr)] string strEncodedIn, [In] EncodingType EncodingIn, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        public virtual extern object StringToVariantByteArray([In, MarshalAs(UnmanagedType.BStr)] string strEncoded, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern string VariantByteArrayToString([In, MarshalAs(UnmanagedType.Struct)] ref object pvarByteArray, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
    }

    [ComImport, CoClass(typeof(CCertificatePoliciesClass)), Guid("728AB31F-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CCertificatePolicies : ICertificatePolicies
    {
    }

    [ComImport, Guid("884E201F-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CCertificatePoliciesClass : ICertificatePolicies, CCertificatePolicies, IEnumerable
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        public virtual extern void Add([In, MarshalAs(UnmanagedType.Interface)] CCertificatePolicy pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        public virtual extern void Clear();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1), DispId(-4)]
        public virtual extern IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        public virtual extern void Remove([In] int Index);

        // Properties        
        [DispId(1)]
        public virtual extern int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [DispId(0)]
        public virtual extern CCertificatePolicy this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
    }

    [ComImport, Guid("728AB31E-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CCertificatePolicyClass))]
    public interface CCertificatePolicy : ICertificatePolicy
    {
    }

    [ComImport, Guid("884E201E-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CCertificatePolicyClass : ICertificatePolicy, CCertificatePolicy
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pValue);

        // Properties        
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        public virtual extern CPolicyQualifiers PolicyQualifiers { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, Guid("728AB32F-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CCertPropertiesClass))]
    public interface CCertProperties : ICertProperties
    {
    }

    [ComImport, TypeLibType((short) 2), Guid("884E202F-217D-11DA-B2A4-000E7BBB2B09"), ClassInterface((short) 0)]
    public class CCertPropertiesClass : ICertProperties, CCertProperties, IEnumerable
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        public virtual extern void Add([In, MarshalAs(UnmanagedType.Interface)] CCertProperty pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        public virtual extern void Clear();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1), DispId(-4)]
        public virtual extern IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        public virtual extern void Remove([In] int Index);

        // Properties        
        [DispId(1)]
        public virtual extern int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [DispId(0)]
        public virtual extern CCertProperty this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
    }

    [ComImport, CoClass(typeof(CCertPropertyClass)), Guid("728AB32E-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CCertProperty : ICertProperty
    {
    }

    [ComImport, CoClass(typeof(CCertPropertyArchivedClass)), Guid("728AB337-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CCertPropertyArchived : ICertPropertyArchived
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E2037-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CCertPropertyArchivedClass : ICertPropertyArchived, CCertPropertyArchived
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void Initialize([In] bool ArchivedValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);

        // Properties
        [DispId(0x60030001)]
        public virtual extern bool Archived { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)] get; }
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020002)]
        public virtual extern CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }        
    }

    [ComImport, Guid("728AB33B-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CCertPropertyArchivedKeyHashClass))]
    public interface CCertPropertyArchivedKeyHash : ICertPropertyArchivedKeyHash
    {
    }

    [ComImport, Guid("884E203B-217D-11DA-B2A4-000E7BBB2B09"), ClassInterface((short) 0), TypeLibType((short) 2)]
    public class CCertPropertyArchivedKeyHashClass : ICertPropertyArchivedKeyHash, CCertPropertyArchivedKeyHash
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void Initialize([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strArchivedKeyHashValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);

        // Properties
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)] get; }                        
        [DispId(0x60020002)]
        public virtual extern CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }        
    }

    [ComImport, Guid("728AB332-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CCertPropertyAutoEnrollClass))]
    public interface CCertPropertyAutoEnroll : ICertPropertyAutoEnroll
    {
    }

    [ComImport, TypeLibType((short) 2), Guid("884E2032-217D-11DA-B2A4-000E7BBB2B09"), ClassInterface((short) 0)]
    public class CCertPropertyAutoEnrollClass : ICertPropertyAutoEnroll, CCertPropertyAutoEnroll
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);

        // Properties        
        [DispId(0x60020002)]
        public virtual extern CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60030001)]
        public virtual extern string TemplateName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)] get; }
    }

    [ComImport, Guid("728AB338-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CCertPropertyBackedUpClass))]
    public interface CCertPropertyBackedUp : ICertPropertyBackedUp
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E2038-217D-11DA-B2A4-000E7BBB2B09")]
    public class CCertPropertyBackedUpClass : ICertPropertyBackedUp, CCertPropertyBackedUp
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void Initialize([In] bool BackedUpValue, [In] DateTime Date);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeFromCurrentTime([In] bool BackedUpValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);

        // Properties
        [DispId(0x60030003)]
        public virtual extern DateTime BackedUpTime { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
        [DispId(0x60030002)]
        public virtual extern bool BackedUpValue { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }                        
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020002)]
        public virtual extern CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }        
    }

    [ComImport, ClassInterface((short) 0), Guid("884E202E-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CCertPropertyClass : ICertProperty, CCertProperty
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);

        // Properties        
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020002)]
        public virtual extern CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }        
    }

    [ComImport, CoClass(typeof(CCertPropertyDescriptionClass)), Guid("728AB331-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CCertPropertyDescription : ICertPropertyDescription
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E2031-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CCertPropertyDescriptionClass : ICertPropertyDescription, CCertPropertyDescription
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.BStr)] string strDescription);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);

        // Properties                
        [DispId(0x60030001)]
        public virtual extern string Description { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)] get; }
        [DispId(0x60020002)]
        public virtual extern CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
    }

    [ComImport, CoClass(typeof(CCertPropertyEnrollmentClass)), Guid("728AB339-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CCertPropertyEnrollment : ICertPropertyEnrollment
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E2039-217D-11DA-B2A4-000E7BBB2B09")]
    public class CCertPropertyEnrollmentClass : ICertPropertyEnrollment, CCertPropertyEnrollment
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void Initialize([In] int RequestId, [In, MarshalAs(UnmanagedType.BStr)] string strCADnsName, [In, MarshalAs(UnmanagedType.BStr)] string strCAName, [In, Optional, DefaultParameterValue("0"), MarshalAs(UnmanagedType.BStr)] string strFriendlyName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);

        // Properties
        [DispId(0x60030002)]
        public virtual extern string CADnsName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60030003)]
        public virtual extern string CAName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }        
        [DispId(0x60030004)]
        public virtual extern string FriendlyName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; }
        [DispId(0x60020002)]
        public virtual extern CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60030001)]
        public virtual extern int RequestId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)] get; }
    }

    [ComImport, Guid("728AB34A-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CCertPropertyEnrollmentPolicyServerClass))]
    public interface CCertPropertyEnrollmentPolicyServer : ICertPropertyEnrollmentPolicyServer
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E204C-217D-11DA-B2A4-000E7BBB2B09")]
    public class CCertPropertyEnrollmentPolicyServerClass : ICertPropertyEnrollmentPolicyServer, CCertPropertyEnrollmentPolicyServer
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030007)]
        public virtual extern X509EnrollmentAuthFlags GetAuthentication();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030008)]
        public virtual extern X509EnrollmentAuthFlags GetEnrollmentServerAuthentication();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        public virtual extern string GetEnrollmentServerUrl();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        public virtual extern string GetPolicyServerId();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern string GetPolicyServerUrl();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030005)]
        public virtual extern EnrollmentPolicyServerPropertyFlags GetPropertyFlags();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)]
        public virtual extern string GetRequestIdString();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)]
        public virtual extern PolicyServerUrlFlags GetUrlFlags();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void Initialize([In] EnrollmentPolicyServerPropertyFlags PropertyFlags, [In] X509EnrollmentAuthFlags AuthFlags, [In] X509EnrollmentAuthFlags EnrollmentServerAuthFlags, [In] PolicyServerUrlFlags UrlFlags, [In, MarshalAs(UnmanagedType.BStr)] string strRequestId, [In, MarshalAs(UnmanagedType.BStr)] string strUrl, [In, MarshalAs(UnmanagedType.BStr)] string strId, [In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentServerUrl);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);

        // Properties                
        [DispId(0x60020002)]
        public virtual extern CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
    }

    [ComImport, Guid("728AB330-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CCertPropertyFriendlyNameClass))]
    public interface CCertPropertyFriendlyName : ICertPropertyFriendlyName
    {
    }

    [ComImport, TypeLibType((short) 2), Guid("884E2030-217D-11DA-B2A4-000E7BBB2B09"), ClassInterface((short) 0)]
    public class CCertPropertyFriendlyNameClass : ICertPropertyFriendlyName, CCertPropertyFriendlyName
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.BStr)] string strFriendlyName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);

        // Properties                
        [DispId(0x60030001)]
        public virtual extern string FriendlyName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)] get; }
        [DispId(0x60020002)]
        public virtual extern CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
    }

    [ComImport, Guid("728AB336-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CCertPropertyKeyProvInfoClass))]
    public interface CCertPropertyKeyProvInfo : ICertPropertyKeyProvInfo
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E2036-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CCertPropertyKeyProvInfoClass : ICertPropertyKeyProvInfo, CCertPropertyKeyProvInfo
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);

        // Properties        
        [DispId(0x60030001)]
        public virtual extern CX509PrivateKey PrivateKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)] get; }
        [DispId(0x60020002)]
        public virtual extern CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
    }

    [ComImport, Guid("728AB33A-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CCertPropertyRenewalClass))]
    public interface CCertPropertyRenewal : ICertPropertyRenewal
    {
    }

    [ComImport, Guid("884E203A-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CCertPropertyRenewalClass : ICertPropertyRenewal, CCertPropertyRenewal
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void Initialize([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strRenewalValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeFromCertificateHash([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);

        // Properties        
        [DispId(0x60020002)]
        public virtual extern CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }        
    }

    [ComImport, Guid("728AB333-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CCertPropertyRequestOriginatorClass))]
    public interface CCertPropertyRequestOriginator : ICertPropertyRequestOriginator
    {
    }

    [ComImport, Guid("884E2033-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CCertPropertyRequestOriginatorClass : ICertPropertyRequestOriginator, CCertPropertyRequestOriginator
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.BStr)] string strRequestOriginator);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeFromLocalRequestOriginator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);

        // Properties        
        [DispId(0x60020002)]
        public virtual extern CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60030002)]
        public virtual extern string RequestOriginator { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, CoClass(typeof(CCertPropertySHA1HashClass)), Guid("728AB334-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CCertPropertySHA1Hash : ICertPropertySHA1Hash
    {
    }

    [ComImport, Guid("884E2034-217D-11DA-B2A4-000E7BBB2B09"), ClassInterface((short) 0), TypeLibType((short) 2)]
    public class CCertPropertySHA1HashClass : ICertPropertySHA1Hash, CCertPropertySHA1Hash
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void Initialize([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strRenewalValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);

        // Properties       
        [DispId(0x60020002)]
        public virtual extern CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }        
    }

    [ComImport, CoClass(typeof(CCryptAttributeClass)), Guid("728AB32C-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CCryptAttribute : ICryptAttribute
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E202C-217D-11DA-B2A4-000E7BBB2B09")]
    public class CCryptAttributeClass : ICryptAttribute, CCryptAttribute
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromObjectId([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeFromValues([In, MarshalAs(UnmanagedType.Interface)] CX509Attributes pAttributes);

        // Properties        
        [DispId(0x60020002)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        public virtual extern CX509Attributes Values { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
    }

    [ComImport, CoClass(typeof(CCryptAttributesClass)), Guid("728AB32D-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CCryptAttributes : ICryptAttributes
    {
    }

    [ComImport, Guid("884E202D-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CCryptAttributesClass : ICryptAttributes, CCryptAttributes, IEnumerable
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        public virtual extern void Add([In, MarshalAs(UnmanagedType.Interface)] CCryptAttribute pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)]
        public virtual extern void AddRange([In, MarshalAs(UnmanagedType.Interface)] CCryptAttributes pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        public virtual extern void Clear();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        public virtual extern IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        public virtual extern void Remove([In] int Index);

        // Properties        
        [DispId(1)]
        public virtual extern int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [DispId(0x60020006)]
        public virtual extern int this[CObjectId pObjectId] { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
        [DispId(0)]
        public virtual extern CCryptAttribute this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
    }

    [ComImport, Guid("728AB307-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CCspInformationClass))]
    public interface CCspInformation : ICspInformation
    {
    }

    [ComImport, TypeLibType((short) 2), Guid("884E2007-217D-11DA-B2A4-000E7BBB2B09"), ClassInterface((short) 0)]
    public class CCspInformationClass : ICspInformation, CCspInformation
    {
        // Methods
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)]
        public virtual extern CCspStatus GetCspStatusFromOperations([In, MarshalAs(UnmanagedType.Interface)] CObjectId pAlgorithm, [In] AlgorithmOperationFlags Operations);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)]
        public virtual extern string GetDefaultSecurityDescriptor([In] bool MachineContext);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromName([In, MarshalAs(UnmanagedType.BStr)] string strName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeFromType([In] X509ProviderType Type, [In, MarshalAs(UnmanagedType.Interface)] CObjectId pAlgorithm, [In] bool MachineContext);

        // Properties     
        [DispId(0x60020002)]
        public virtual extern ICspAlgorithms CspAlgorithms { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        public virtual extern bool HasHardwareRandomNumberGenerator { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
        [DispId(0x60020004)]
        public virtual extern bool IsHardwareDevice { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        public virtual extern bool IsRemovable { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x6002000d)]
        public virtual extern bool IsSmartCard { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000d)] get; }
        [DispId(0x60020006)]
        public virtual extern bool IsSoftwareDevice { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
        [DispId(0x6002000c)]
        public virtual extern X509KeySpec KeySpec { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; }
        [DispId(0x6002000f)]
        public virtual extern bool LegacyCsp { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000f)] get; }
        [DispId(0x60020008)]
        public virtual extern int MaxKeyContainerNameLength { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; }
        [DispId(0x60020009)]
        public virtual extern string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] get; }
        [DispId(0x6002000a)]
        public virtual extern X509ProviderType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; }
        [DispId(0x60020007)]
        public virtual extern bool Valid { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; }
        [DispId(0x6002000b)]
        public virtual extern int Version { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000b)] get; }
    }

    [ComImport, Guid("728AB308-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CCspInformationsClass))]
    public interface CCspInformations : ICspInformations
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E2008-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CCspInformationsClass : ICspInformations, CCspInformations, IEnumerable
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        public virtual extern void Add([In, MarshalAs(UnmanagedType.Interface)] CCspInformation pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void AddAvailableCsps();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        public virtual extern void Clear();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)]
        public virtual extern ICspStatuses GetCspStatusesFromOperations([In] AlgorithmOperationFlags Operations, [In, MarshalAs(UnmanagedType.Interface)] CCspInformation pCspInformation);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)]
        public virtual extern CCspStatus GetCspStatusFromProviderName([In, MarshalAs(UnmanagedType.BStr)] string strProviderName, [In] X509KeySpec LegacyKeySpec);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)]
        public virtual extern ICspAlgorithms GetEncryptionCspAlgorithms([In, MarshalAs(UnmanagedType.Interface)] CCspInformation pCspInformation);
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1), DispId(-4)]
        public virtual extern IEnumerator GetEnumerator();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000b)]
        public virtual extern CObjectIds GetHashAlgorithms([In, MarshalAs(UnmanagedType.Interface)] CCspInformation pCspInformation);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        public virtual extern void Remove([In] int Index);

        // Properties        
        [DispId(1)]
        public virtual extern int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [DispId(0)]
        public virtual extern CCspInformation this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(0x60020007)]
        public virtual extern CCspInformation this[string strName] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; }
    }

    [ComImport, Guid("728AB309-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CCspStatusClass))]
    public interface CCspStatus : ICspStatus
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E2009-217D-11DA-B2A4-000E7BBB2B09")]
    public class CCspStatusClass : ICspStatus, CCspStatus
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CCspInformation pCsp, [In, MarshalAs(UnmanagedType.Interface)] ICspAlgorithm pAlgorithm);

        // Properties        
        [DispId(0x60020003)]
        public virtual extern ICspAlgorithm CspAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
        [DispId(0x60020004)]
        public virtual extern CCspInformation CspInformation { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020006)]
        public virtual extern string DisplayName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
        [DispId(0x60020005)]
        public virtual extern IX509EnrollmentStatus EnrollmentStatus { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020001)]
        public virtual extern int Ordinal { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] set; }
    }

    public enum CERTENROLL_OBJECTID
    {
        XCN_OID_ANSI_X942 = 0x35,
        XCN_OID_ANSI_X942_DH = 0x36,
        XCN_OID_ANY_APPLICATION_POLICY = 0xd8,
        XCN_OID_ANY_CERT_POLICY = 180,
        XCN_OID_APPLICATION_CERT_POLICIES = 0xe5,
        XCN_OID_APPLICATION_POLICY_CONSTRAINTS = 0xe7,
        XCN_OID_APPLICATION_POLICY_MAPPINGS = 230,
        XCN_OID_ARCHIVED_KEY_ATTR = 0xe8,
        XCN_OID_ARCHIVED_KEY_CERT_HASH = 0xeb,
        XCN_OID_AUTHORITY_INFO_ACCESS = 0xcc,
        XCN_OID_AUTHORITY_KEY_IDENTIFIER = 0xa9,
        XCN_OID_AUTHORITY_KEY_IDENTIFIER2 = 0xb5,
        XCN_OID_AUTHORITY_REVOCATION_LIST = 0x9c,
        XCN_OID_AUTO_ENROLL_CTL_USAGE = 0xd9,
        XCN_OID_BACKGROUND_OTHER_LOGOTYPE = 0x147,
        XCN_OID_BASIC_CONSTRAINTS = 0xaf,
        XCN_OID_BASIC_CONSTRAINTS2 = 0xb2,
        XCN_OID_BIOMETRIC_EXT = 0xcd,
        XCN_OID_BUSINESS_CATEGORY = 0x85,
        XCN_OID_CA_CERTIFICATE = 0x9b,
        XCN_OID_CERT_EXTENSIONS = 0xcf,
        XCN_OID_CERT_ISSUER_SERIAL_NUMBER_MD5_HASH_PROP_ID = 0x153,
        XCN_OID_CERT_KEY_IDENTIFIER_PROP_ID = 0x152,
        XCN_OID_CERT_MANIFOLD = 0xdb,
        XCN_OID_CERT_MD5_HASH_PROP_ID = 0x155,
        XCN_OID_CERT_POLICIES = 0xb3,
        XCN_OID_CERT_POLICIES_95 = 0xab,
        XCN_OID_CERT_POLICIES_95_QUALIFIER1 = 0x119,
        XCN_OID_CERT_PROP_ID_PREFIX = 0x151,
        XCN_OID_CERT_SUBJECT_NAME_MD5_HASH_PROP_ID = 340,
        XCN_OID_CERTIFICATE_REVOCATION_LIST = 0x9d,
        XCN_OID_CERTIFICATE_TEMPLATE = 0xe2,
        XCN_OID_CERTSRV_CA_VERSION = 220,
        XCN_OID_CERTSRV_CROSSCA_VERSION = 240,
        XCN_OID_CERTSRV_PREVIOUS_CERT_HASH = 0xdd,
        XCN_OID_CMC = 0x130,
        XCN_OID_CMC_ADD_ATTRIBUTES = 0x145,
        XCN_OID_CMC_ADD_EXTENSIONS = 0x138,
        XCN_OID_CMC_DATA_RETURN = 0x134,
        XCN_OID_CMC_DECRYPTED_POP = 0x13a,
        XCN_OID_CMC_ENCRYPTED_POP = 0x139,
        XCN_OID_CMC_GET_CERT = 0x13c,
        XCN_OID_CMC_GET_CRL = 0x13d,
        XCN_OID_CMC_ID_CONFIRM_CERT_ACCEPTANCE = 0x144,
        XCN_OID_CMC_ID_POP_LINK_RANDOM = 0x142,
        XCN_OID_CMC_ID_POP_LINK_WITNESS = 0x143,
        XCN_OID_CMC_IDENTIFICATION = 0x132,
        XCN_OID_CMC_IDENTITY_PROOF = 0x133,
        XCN_OID_CMC_LRA_POP_WITNESS = 0x13b,
        XCN_OID_CMC_QUERY_PENDING = 0x141,
        XCN_OID_CMC_RECIPIENT_NONCE = 0x137,
        XCN_OID_CMC_REG_INFO = 0x13f,
        XCN_OID_CMC_RESPONSE_INFO = 320,
        XCN_OID_CMC_REVOKE_REQUEST = 0x13e,
        XCN_OID_CMC_SENDER_NONCE = 310,
        XCN_OID_CMC_STATUS_INFO = 0x131,
        XCN_OID_CMC_TRANSACTION_ID = 0x135,
        XCN_OID_COMMON_NAME = 0x79,
        XCN_OID_COUNTRY_NAME = 0x7c,
        XCN_OID_CRL_DIST_POINTS = 0xbb,
        XCN_OID_CRL_NEXT_PUBLISH = 0xdf,
        XCN_OID_CRL_NUMBER = 0xbd,
        XCN_OID_CRL_REASON_CODE = 0xb9,
        XCN_OID_CRL_SELF_CDP = 0xe9,
        XCN_OID_CRL_VIRTUAL_BASE = 0xde,
        XCN_OID_CROSS_CERT_DIST_POINTS = 210,
        XCN_OID_CROSS_CERTIFICATE_PAIR = 0x9e,
        XCN_OID_CT_PKI_DATA = 0x12d,
        XCN_OID_CT_PKI_RESPONSE = 0x12e,
        XCN_OID_CTL = 0xd3,
        XCN_OID_DELTA_CRL_INDICATOR = 190,
        XCN_OID_DESCRIPTION = 0x83,
        XCN_OID_DESTINATION_INDICATOR = 0x91,
        XCN_OID_DEVICE_SERIAL_NUMBER = 0x7b,
        XCN_OID_DN_QUALIFIER = 0xa1,
        XCN_OID_DOMAIN_COMPONENT = 0xa2,
        XCN_OID_DRM = 0x111,
        XCN_OID_DRM_INDIVIDUALIZATION = 0x112,
        XCN_OID_DS = 0x3a,
        XCN_OID_DS_EMAIL_REPLICATION = 0xed,
        XCN_OID_DSALG = 0x3b,
        XCN_OID_DSALG_CRPT = 60,
        XCN_OID_DSALG_HASH = 0x3d,
        XCN_OID_DSALG_RSA = 0x3f,
        XCN_OID_DSALG_SIGN = 0x3e,
        XCN_OID_ECC_PUBLIC_KEY = 0x15d,
        XCN_OID_ECDSA_SHA1 = 0x162,
        XCN_OID_ECDSA_SPECIFIED = 0x162,
        XCN_OID_EFS_RECOVERY = 260,
        XCN_OID_EMBEDDED_NT_CRYPTO = 0x108,
        XCN_OID_ENCRYPTED_KEY_HASH = 0xef,
        XCN_OID_ENHANCED_KEY_USAGE = 0xbc,
        XCN_OID_ENROLL_CERTTYPE_EXTENSION = 0xda,
        XCN_OID_ENROLLMENT_AGENT = 0xc9,
        XCN_OID_ENROLLMENT_CSP_PROVIDER = 0xc7,
        XCN_OID_ENROLLMENT_NAME_VALUE_PAIR = 0xc6,
        XCN_OID_ENTERPRISE_OID_ROOT = 0xe3,
        XCN_OID_FACSIMILE_TELEPHONE_NUMBER = 0x8d,
        XCN_OID_FRESHEST_CRL = 0xc0,
        XCN_OID_GIVEN_NAME = 0x9f,
        XCN_OID_INFOSEC = 0x63,
        XCN_OID_INFOSEC_mosaicConfidentiality = 0x67,
        XCN_OID_INFOSEC_mosaicIntegrity = 0x69,
        XCN_OID_INFOSEC_mosaicKeyManagement = 0x6d,
        XCN_OID_INFOSEC_mosaicKMandSig = 0x6f,
        XCN_OID_INFOSEC_mosaicKMandUpdSig = 0x77,
        XCN_OID_INFOSEC_mosaicSignature = 0x65,
        XCN_OID_INFOSEC_mosaicTokenProtection = 0x6b,
        XCN_OID_INFOSEC_mosaicUpdatedInteg = 120,
        XCN_OID_INFOSEC_mosaicUpdatedSig = 0x76,
        XCN_OID_INFOSEC_sdnsConfidentiality = 0x66,
        XCN_OID_INFOSEC_sdnsIntegrity = 0x68,
        XCN_OID_INFOSEC_sdnsKeyManagement = 0x6c,
        XCN_OID_INFOSEC_sdnsKMandSig = 110,
        XCN_OID_INFOSEC_sdnsSignature = 100,
        XCN_OID_INFOSEC_sdnsTokenProtection = 0x6a,
        XCN_OID_INFOSEC_SuiteAConfidentiality = 0x71,
        XCN_OID_INFOSEC_SuiteAIntegrity = 0x72,
        XCN_OID_INFOSEC_SuiteAKeyManagement = 0x74,
        XCN_OID_INFOSEC_SuiteAKMandSig = 0x75,
        XCN_OID_INFOSEC_SuiteASignature = 0x70,
        XCN_OID_INFOSEC_SuiteATokenProtection = 0x73,
        XCN_OID_INITIALS = 160,
        XCN_OID_INTERNATIONAL_ISDN_NUMBER = 0x8f,
        XCN_OID_IPSEC_KP_IKE_INTERMEDIATE = 0xfe,
        XCN_OID_ISSUED_CERT_HASH = 0xec,
        XCN_OID_ISSUER_ALT_NAME = 0xae,
        XCN_OID_ISSUER_ALT_NAME2 = 0xb8,
        XCN_OID_ISSUING_DIST_POINT = 0xbf,
        XCN_OID_KEY_ATTRIBUTES = 170,
        XCN_OID_KEY_USAGE = 0xb0,
        XCN_OID_KEY_USAGE_RESTRICTION = 0xac,
        XCN_OID_KEYID_RDN = 0xa8,
        XCN_OID_KP_CA_EXCHANGE = 0xe0,
        XCN_OID_KP_CSP_SIGNATURE = 0x110,
        XCN_OID_KP_CTL_USAGE_SIGNING = 0xff,
        XCN_OID_KP_DOCUMENT_SIGNING = 0x10c,
        XCN_OID_KP_EFS = 0x103,
        XCN_OID_KP_KEY_RECOVERY = 0x10b,
        XCN_OID_KP_KEY_RECOVERY_AGENT = 0xe1,
        XCN_OID_KP_LIFETIME_SIGNING = 0x10d,
        XCN_OID_KP_MOBILE_DEVICE_SOFTWARE = 270,
        XCN_OID_KP_QUALIFIED_SUBORDINATION = 0x10a,
        XCN_OID_KP_SMART_DISPLAY = 0x10f,
        XCN_OID_KP_SMARTCARD_LOGON = 0x115,
        XCN_OID_KP_TIME_STAMP_SIGNING = 0x100,
        XCN_OID_LEGACY_POLICY_MAPPINGS = 0xc3,
        XCN_OID_LICENSE_SERVER = 0x114,
        XCN_OID_LICENSES = 0x113,
        XCN_OID_LOCAL_MACHINE_KEYSET = 0xa6,
        XCN_OID_LOCALITY_NAME = 0x7d,
        XCN_OID_LOGOTYPE_EXT = 0xce,
        XCN_OID_LOYALTY_OTHER_LOGOTYPE = 0x146,
        XCN_OID_MEMBER = 0x95,
        XCN_OID_NAME_CONSTRAINTS = 0xc1,
        XCN_OID_NETSCAPE = 0x121,
        XCN_OID_NETSCAPE_BASE_URL = 0x124,
        XCN_OID_NETSCAPE_CA_POLICY_URL = 0x128,
        XCN_OID_NETSCAPE_CA_REVOCATION_URL = 0x126,
        XCN_OID_NETSCAPE_CERT_EXTENSION = 290,
        XCN_OID_NETSCAPE_CERT_RENEWAL_URL = 0x127,
        XCN_OID_NETSCAPE_CERT_SEQUENCE = 300,
        XCN_OID_NETSCAPE_CERT_TYPE = 0x123,
        XCN_OID_NETSCAPE_COMMENT = 0x12a,
        XCN_OID_NETSCAPE_DATA_TYPE = 0x12b,
        XCN_OID_NETSCAPE_REVOCATION_URL = 0x125,
        XCN_OID_NETSCAPE_SSL_SERVER_NAME = 0x129,
        XCN_OID_NEXT_UPDATE_LOCATION = 0xd0,
        XCN_OID_NIST_sha256 = 0x159,
        XCN_OID_NIST_sha384 = 0x15a,
        XCN_OID_NIST_sha512 = 0x15b,
        XCN_OID_NONE = 0,
        XCN_OID_NT_PRINCIPAL_NAME = 0xd6,
        XCN_OID_NT5_CRYPTO = 0x106,
        XCN_OID_NTDS_REPLICATION = 0xf1,
        XCN_OID_OEM_WHQL_CRYPTO = 0x107,
        XCN_OID_OIW = 0x40,
        XCN_OID_OIWDIR = 0x5d,
        XCN_OID_OIWDIR_CRPT = 0x5e,
        XCN_OID_OIWDIR_HASH = 0x5f,
        XCN_OID_OIWDIR_md2 = 0x61,
        XCN_OID_OIWDIR_md2RSA = 0x62,
        XCN_OID_OIWDIR_SIGN = 0x60,
        XCN_OID_OIWSEC = 0x41,
        XCN_OID_OIWSEC_desCBC = 70,
        XCN_OID_OIWSEC_desCFB = 0x48,
        XCN_OID_OIWSEC_desECB = 0x45,
        XCN_OID_OIWSEC_desEDE = 80,
        XCN_OID_OIWSEC_desMAC = 0x49,
        XCN_OID_OIWSEC_desOFB = 0x47,
        XCN_OID_OIWSEC_dhCommMod = 0x4f,
        XCN_OID_OIWSEC_dsa = 0x4b,
        XCN_OID_OIWSEC_dsaComm = 0x53,
        XCN_OID_OIWSEC_dsaCommSHA = 0x54,
        XCN_OID_OIWSEC_dsaCommSHA1 = 0x5b,
        XCN_OID_OIWSEC_dsaSHA1 = 90,
        XCN_OID_OIWSEC_keyHashSeal = 0x56,
        XCN_OID_OIWSEC_md2RSASign = 0x57,
        XCN_OID_OIWSEC_md4RSA = 0x42,
        XCN_OID_OIWSEC_md4RSA2 = 0x44,
        XCN_OID_OIWSEC_md5RSA = 0x43,
        XCN_OID_OIWSEC_md5RSASign = 0x58,
        XCN_OID_OIWSEC_mdc2 = 0x52,
        XCN_OID_OIWSEC_mdc2RSA = 0x4d,
        XCN_OID_OIWSEC_rsaSign = 0x4a,
        XCN_OID_OIWSEC_rsaXchg = 0x55,
        XCN_OID_OIWSEC_sha = 0x51,
        XCN_OID_OIWSEC_sha1 = 0x59,
        XCN_OID_OIWSEC_sha1RSASign = 0x5c,
        XCN_OID_OIWSEC_shaDSA = 0x4c,
        XCN_OID_OIWSEC_shaRSA = 0x4e,
        XCN_OID_ORGANIZATION_NAME = 0x80,
        XCN_OID_ORGANIZATIONAL_UNIT_NAME = 0x81,
        XCN_OID_OS_VERSION = 200,
        XCN_OID_OWNER = 150,
        XCN_OID_PHYSICAL_DELIVERY_OFFICE_NAME = 0x89,
        XCN_OID_PKCS = 2,
        XCN_OID_PKCS_1 = 5,
        XCN_OID_PKCS_10 = 14,
        XCN_OID_PKCS_12 = 15,
        XCN_OID_PKCS_12_EXTENDED_ATTRIBUTES = 0xa7,
        XCN_OID_PKCS_12_FRIENDLY_NAME_ATTR = 0xa3,
        XCN_OID_PKCS_12_KEY_PROVIDER_NAME_ATTR = 0xa5,
        XCN_OID_PKCS_12_LOCAL_KEY_ID = 0xa4,
        XCN_OID_PKCS_2 = 6,
        XCN_OID_PKCS_3 = 7,
        XCN_OID_PKCS_4 = 8,
        XCN_OID_PKCS_5 = 9,
        XCN_OID_PKCS_6 = 10,
        XCN_OID_PKCS_7 = 11,
        XCN_OID_PKCS_7_DATA = 0x149,
        XCN_OID_PKCS_7_DIGESTED = 0x14d,
        XCN_OID_PKCS_7_ENCRYPTED = 0x14e,
        XCN_OID_PKCS_7_ENVELOPED = 0x14b,
        XCN_OID_PKCS_7_SIGNED = 330,
        XCN_OID_PKCS_7_SIGNEDANDENVELOPED = 0x14c,
        XCN_OID_PKCS_8 = 12,
        XCN_OID_PKCS_9 = 13,
        XCN_OID_PKCS_9_CONTENT_TYPE = 0x14f,
        XCN_OID_PKCS_9_MESSAGE_DIGEST = 0x150,
        XCN_OID_PKIX = 0xca,
        XCN_OID_PKIX_ACC_DESCR = 0x11a,
        XCN_OID_PKIX_CA_ISSUERS = 0x11c,
        XCN_OID_PKIX_KP = 0xf3,
        XCN_OID_PKIX_KP_CLIENT_AUTH = 0xf5,
        XCN_OID_PKIX_KP_CODE_SIGNING = 0xf6,
        XCN_OID_PKIX_KP_EMAIL_PROTECTION = 0xf7,
        XCN_OID_PKIX_KP_IPSEC_END_SYSTEM = 0xf8,
        XCN_OID_PKIX_KP_IPSEC_TUNNEL = 0xf9,
        XCN_OID_PKIX_KP_IPSEC_USER = 250,
        XCN_OID_PKIX_KP_OCSP_SIGNING = 0xfc,
        XCN_OID_PKIX_KP_SERVER_AUTH = 0xf4,
        XCN_OID_PKIX_KP_TIMESTAMP_SIGNING = 0xfb,
        XCN_OID_PKIX_NO_SIGNATURE = 0x12f,
        XCN_OID_PKIX_OCSP = 0x11b,
        XCN_OID_PKIX_OCSP_BASIC_SIGNED_RESPONSE = 0x148,
        XCN_OID_PKIX_OCSP_NOCHECK = 0xfd,
        XCN_OID_PKIX_PE = 0xcb,
        XCN_OID_PKIX_POLICY_QUALIFIER_CPS = 0x117,
        XCN_OID_PKIX_POLICY_QUALIFIER_USERNOTICE = 280,
        XCN_OID_POLICY_CONSTRAINTS = 0xc4,
        XCN_OID_POLICY_MAPPINGS = 0xc2,
        XCN_OID_POST_OFFICE_BOX = 0x88,
        XCN_OID_POSTAL_ADDRESS = 0x86,
        XCN_OID_POSTAL_CODE = 0x87,
        XCN_OID_PREFERRED_DELIVERY_METHOD = 0x92,
        XCN_OID_PRESENTATION_ADDRESS = 0x93,
        XCN_OID_PRIVATEKEY_USAGE_PERIOD = 0xb1,
        XCN_OID_PRODUCT_UPDATE = 0xd7,
        XCN_OID_RDN_DUMMY_SIGNER = 0xe4,
        XCN_OID_REASON_CODE_HOLD = 0xba,
        XCN_OID_REGISTERED_ADDRESS = 0x90,
        XCN_OID_REMOVE_CERTIFICATE = 0xd1,
        XCN_OID_RENEWAL_CERTIFICATE = 0xc5,
        XCN_OID_REQUEST_CLIENT_INFO = 0xee,
        XCN_OID_REQUIRE_CERT_CHAIN_POLICY = 0xea,
        XCN_OID_ROLE_OCCUPANT = 0x97,
        XCN_OID_ROOT_LIST_SIGNER = 0x109,
        XCN_OID_RSA = 1,
        XCN_OID_RSA_certExtensions = 0x27,
        XCN_OID_RSA_challengePwd = 0x24,
        XCN_OID_RSA_contentType = 0x20,
        XCN_OID_RSA_counterSign = 0x23,
        XCN_OID_RSA_data = 0x17,
        XCN_OID_RSA_DES_EDE3_CBC = 0x33,
        XCN_OID_RSA_DH = 0x16,
        XCN_OID_RSA_digestedData = 0x1b,
        XCN_OID_RSA_emailAddr = 30,
        XCN_OID_RSA_ENCRYPT = 4,
        XCN_OID_RSA_encryptedData = 0x1d,
        XCN_OID_RSA_envelopedData = 0x19,
        XCN_OID_RSA_extCertAttrs = 0x26,
        XCN_OID_RSA_HASH = 3,
        XCN_OID_RSA_hashedData = 0x1c,
        XCN_OID_RSA_MD2 = 0x2e,
        XCN_OID_RSA_MD2RSA = 0x11,
        XCN_OID_RSA_MD4 = 0x2f,
        XCN_OID_RSA_MD4RSA = 0x12,
        XCN_OID_RSA_MD5 = 0x30,
        XCN_OID_RSA_MD5RSA = 0x13,
        XCN_OID_RSA_messageDigest = 0x21,
        XCN_OID_RSA_MGF1 = 0x15c,
        XCN_OID_RSA_preferSignedData = 0x29,
        XCN_OID_RSA_RC2CBC = 0x31,
        XCN_OID_RSA_RC4 = 50,
        XCN_OID_RSA_RC5_CBCPad = 0x34,
        XCN_OID_RSA_RSA = 0x10,
        XCN_OID_RSA_SETOAEP_RSA = 0x15,
        XCN_OID_RSA_SHA1RSA = 20,
        XCN_OID_RSA_SHA256RSA = 0x156,
        XCN_OID_RSA_SHA384RSA = 0x157,
        XCN_OID_RSA_SHA512RSA = 0x158,
        XCN_OID_RSA_signedData = 0x18,
        XCN_OID_RSA_signEnvData = 0x1a,
        XCN_OID_RSA_signingTime = 0x22,
        XCN_OID_RSA_SMIMEalg = 0x2a,
        XCN_OID_RSA_SMIMEalgCMS3DESwrap = 0x2c,
        XCN_OID_RSA_SMIMEalgCMSRC2wrap = 0x2d,
        XCN_OID_RSA_SMIMEalgESDH = 0x2b,
        XCN_OID_RSA_SMIMECapabilities = 40,
        XCN_OID_RSA_SSA_PSS = 0x161,
        XCN_OID_RSA_unstructAddr = 0x25,
        XCN_OID_RSA_unstructName = 0x1f,
        XCN_OID_SEARCH_GUIDE = 0x84,
        XCN_OID_SEE_ALSO = 0x98,
        XCN_OID_SERIALIZED = 0xd5,
        XCN_OID_SERVER_GATED_CRYPTO = 0x101,
        XCN_OID_SGC_NETSCAPE = 0x102,
        XCN_OID_SORTED_CTL = 0xd4,
        XCN_OID_STATE_OR_PROVINCE_NAME = 0x7e,
        XCN_OID_STREET_ADDRESS = 0x7f,
        XCN_OID_SUBJECT_ALT_NAME = 0xad,
        XCN_OID_SUBJECT_ALT_NAME2 = 0xb7,
        XCN_OID_SUBJECT_DIR_ATTRS = 0xf2,
        XCN_OID_SUBJECT_KEY_IDENTIFIER = 0xb6,
        XCN_OID_SUPPORTED_APPLICATION_CONTEXT = 0x94,
        XCN_OID_SUR_NAME = 0x7a,
        XCN_OID_TELEPHONE_NUMBER = 0x8a,
        XCN_OID_TELETEXT_TERMINAL_IDENTIFIER = 140,
        XCN_OID_TELEX_NUMBER = 0x8b,
        XCN_OID_TITLE = 130,
        XCN_OID_USER_CERTIFICATE = 0x9a,
        XCN_OID_USER_PASSWORD = 0x99,
        XCN_OID_VERISIGN_BITSTRING_6_13 = 0x11f,
        XCN_OID_VERISIGN_ISS_STRONG_CRYPTO = 0x120,
        XCN_OID_VERISIGN_ONSITE_JURISDICTION_HASH = 0x11e,
        XCN_OID_VERISIGN_PRIVATE_6_9 = 0x11d,
        XCN_OID_WHQL_CRYPTO = 0x105,
        XCN_OID_X21_ADDRESS = 0x8e,
        XCN_OID_X957 = 0x37,
        XCN_OID_X957_DSA = 0x38,
        XCN_OID_X957_SHA1DSA = 0x39,
        XCN_OID_YESNO_TRUST_ATTR = 0x116
    }

    public enum CERTENROLL_PROPERTYID
    {
        XCN_CERT_ACCESS_STATE_PROP_ID = 14,
        XCN_CERT_AIA_URL_RETRIEVED_PROP_ID = 0x43,
        XCN_CERT_ARCHIVED_KEY_HASH_PROP_ID = 0x41,
        XCN_CERT_ARCHIVED_PROP_ID = 0x13,
        XCN_CERT_AUTHORITY_INFO_ACCESS_PROP_ID = 0x44,
        XCN_CERT_AUTO_ENROLL_PROP_ID = 0x15,
        XCN_CERT_AUTO_ENROLL_RETRY_PROP_ID = 0x42,
        XCN_CERT_BACKED_UP_PROP_ID = 0x45,
        XCN_CERT_CEP_PROP_ID = 0x57,
        XCN_CERT_CROSS_CERT_DIST_POINTS_PROP_ID = 0x17,
        XCN_CERT_CTL_USAGE_PROP_ID = 9,
        XCN_CERT_DATE_STAMP_PROP_ID = 0x1b,
        XCN_CERT_DESCRIPTION_PROP_ID = 13,
        XCN_CERT_EFS_PROP_ID = 0x11,
        XCN_CERT_ENHKEY_USAGE_PROP_ID = 9,
        XCN_CERT_ENROLLMENT_PROP_ID = 0x1a,
        XCN_CERT_EXTENDED_ERROR_INFO_PROP_ID = 30,
        XCN_CERT_FIRST_RESERVED_PROP_ID = 0x5c,
        XCN_CERT_FIRST_USER_PROP_ID = 0x8000,
        XCN_CERT_FORTEZZA_DATA_PROP_ID = 0x12,
        XCN_CERT_FRIENDLY_NAME_PROP_ID = 11,
        XCN_CERT_HASH_PROP_ID = 3,
        XCN_CERT_IE30_RESERVED_PROP_ID = 7,
        XCN_CERT_ISSUER_PUBLIC_KEY_MD5_HASH_PROP_ID = 0x18,
        XCN_CERT_ISSUER_SERIAL_NUMBER_MD5_HASH_PROP_ID = 0x1c,
        XCN_CERT_KEY_CONTEXT_PROP_ID = 5,
        XCN_CERT_KEY_IDENTIFIER_PROP_ID = 20,
        XCN_CERT_KEY_PROV_HANDLE_PROP_ID = 1,
        XCN_CERT_KEY_PROV_INFO_PROP_ID = 2,
        XCN_CERT_KEY_SPEC_PROP_ID = 6,
        XCN_CERT_LAST_RESERVED_PROP_ID = 0x7fff,
        XCN_CERT_LAST_USER_PROP_ID = 0xffff,
        XCN_CERT_MD5_HASH_PROP_ID = 4,
        XCN_CERT_NEW_KEY_PROP_ID = 0x4a,
        XCN_CERT_NEXT_UPDATE_LOCATION_PROP_ID = 10,
        XCN_CERT_OCSP_RESPONSE_PROP_ID = 70,
        XCN_CERT_PUBKEY_ALG_PARA_PROP_ID = 0x16,
        XCN_CERT_PUBKEY_HASH_RESERVED_PROP_ID = 8,
        XCN_CERT_PVK_FILE_PROP_ID = 12,
        XCN_CERT_RENEWAL_PROP_ID = 0x40,
        XCN_CERT_REQUEST_ORIGINATOR_PROP_ID = 0x47,
        XCN_CERT_SHA1_HASH_PROP_ID = 3,
        XCN_CERT_SIGNATURE_HASH_PROP_ID = 15,
        XCN_CERT_SMART_CARD_DATA_PROP_ID = 0x10,
        XCN_CERT_SOURCE_LOCATION_PROP_ID = 0x48,
        XCN_CERT_SOURCE_URL_PROP_ID = 0x49,
        XCN_CERT_STORE_LOCALIZED_NAME_PROP_ID = 0x1000,
        XCN_CERT_SUBJECT_NAME_MD5_HASH_PROP_ID = 0x1d,
        XCN_CERT_SUBJECT_PUBLIC_KEY_MD5_HASH_PROP_ID = 0x19,
        XCN_PROPERTYID_NONE = 0
    }

    [ComImport, CoClass(typeof(CObjectIdClass)), Guid("728AB300-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CObjectId : IObjectId
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E2000-217D-11DA-B2A4-000E7BBB2B09")]
    public class CObjectIdClass : IObjectId, CObjectId
    {
        // Methods
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)]
        public virtual extern string GetAlgorithmName([In] ObjectIdGroupId GroupId, [In] ObjectIdPublicKeyFlags KeyFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        public virtual extern void InitializeFromAlgorithmName([In] ObjectIdGroupId GroupId, [In] ObjectIdPublicKeyFlags KeyFlags, [In] AlgorithmFlags AlgFlags, [In, MarshalAs(UnmanagedType.BStr)] string strAlgorithmName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeFromName([In] CERTENROLL_OBJECTID Name);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeFromValue([In, MarshalAs(UnmanagedType.BStr)] string strValue);

        // Properties      
        [DispId(0x60020004)]
        public virtual extern string FriendlyName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] set; }
        [DispId(0x60020003)]
        public virtual extern CERTENROLL_OBJECTID Name { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
        [DispId(0x60020006)]
        public virtual extern string Value { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
    }

    [ComImport, CoClass(typeof(CObjectIdsClass)), Guid("728AB301-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CObjectIds : IObjectIds
    {
    }

    [ComImport, TypeLibType((short) 2), Guid("884E2001-217D-11DA-B2A4-000E7BBB2B09"), ClassInterface((short) 0)]
    public class CObjectIdsClass : IObjectIds, CObjectIds, IEnumerable
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        public virtual extern void Add([In, MarshalAs(UnmanagedType.Interface)] CObjectId pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void AddRange([In, MarshalAs(UnmanagedType.Interface)] CObjectIds pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        public virtual extern void Clear();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        public virtual extern IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        public virtual extern void Remove([In] int Index);

        // Properties        
        [DispId(1)]
        public virtual extern int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [DispId(0)]
        public virtual extern CObjectId this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
    }

    public enum CommitTemplateFlags
    {
        CommitFlagDeleteTemplate = 4,
        CommitFlagSaveTemplateGenerateOID = 1,
        CommitFlagSaveTemplateOverwrite = 3,
        CommitFlagSaveTemplateUseCurrentOID = 2
    }

    [ComImport, Guid("728AB31C-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CPolicyQualifierClass))]
    public interface CPolicyQualifier : IPolicyQualifier
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E201C-217D-11DA-B2A4-000E7BBB2B09")]
    public class CPolicyQualifierClass : IPolicyQualifier, CPolicyQualifier
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void InitializeEncode([In, MarshalAs(UnmanagedType.BStr)] string strQualifier, [In] PolicyQualifierType Type);

        // Properties
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        public virtual extern string Qualifier { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020003)]
        public virtual extern PolicyQualifierType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
    }

    [ComImport, CoClass(typeof(CPolicyQualifiersClass)), Guid("728AB31D-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CPolicyQualifiers : IPolicyQualifiers
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E201D-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CPolicyQualifiersClass : IPolicyQualifiers, CPolicyQualifiers, IEnumerable
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        public virtual extern void Add([In, MarshalAs(UnmanagedType.Interface)] CPolicyQualifier pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        public virtual extern void Clear();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        public virtual extern IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        public virtual extern void Remove([In] int Index);

        // Properties        
        [DispId(1)]
        public virtual extern int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [DispId(0)]
        public virtual extern CPolicyQualifier this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
    }

    [ComImport, Guid("728AB33D-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CSignerCertificateClass))]
    public interface CSignerCertificate : ISignerCertificate
    {
    }

    [ComImport, Guid("884E203D-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CSignerCertificateClass : ISignerCertificate, CSignerCertificate
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In] bool MachineContext, [In] X509PrivateKeyVerify VerifyType, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertifcate);

        // Properties    
        [DispId(0x60020001)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020005)]
        public virtual extern int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] set; }
        [DispId(0x60020009)]
        public virtual extern string Pin { [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] set; }
        [DispId(0x60020002)]
        public virtual extern CX509PrivateKey PrivateKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x6002000a)]
        public virtual extern IX509SignatureInformation SignatureInformation { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; }
        [DispId(0x60020003)]
        public virtual extern bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60020007)]
        public virtual extern string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] set; }
    }

    [ComImport, Guid("728AB31A-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CSmimeCapabilitiesClass))]
    public interface CSmimeCapabilities : ISmimeCapabilities
    {
    }

    [ComImport, Guid("884E201A-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CSmimeCapabilitiesClass : ISmimeCapabilities, CSmimeCapabilities, IEnumerable
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        public virtual extern void Add([In, MarshalAs(UnmanagedType.Interface)] CSmimeCapability pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)]
        public virtual extern void AddAvailableSmimeCapabilities([In] bool MachineContext);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void AddFromCsp([In, MarshalAs(UnmanagedType.Interface)] CCspInformation pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        public virtual extern void Clear();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1), DispId(-4)]
        public virtual extern IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        public virtual extern void Remove([In] int Index);

        // Properties      
        [DispId(1)]
        public virtual extern int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [DispId(0)]
        public virtual extern CSmimeCapability this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
    }

    [ComImport, Guid("728AB319-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CSmimeCapabilityClass))]
    public interface CSmimeCapability : ISmimeCapability
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E2019-217D-11DA-B2A4-000E7BBB2B09")]
    public class CSmimeCapabilityClass : ISmimeCapability, CSmimeCapability
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] int BitCount);

        // Properties
        [DispId(0x60020002)]
        public virtual extern int BitCount { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }               
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
    }

    [ComImport, CoClass(typeof(CX500DistinguishedNameClass)), Guid("728AB303-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CX500DistinguishedName : IX500DistinguishedName
    {
    }

    [ComImport, ClassInterface((short) 0), TypeLibType((short) 2), Guid("884E2003-217D-11DA-B2A4-000E7BBB2B09")]
    public class CX500DistinguishedNameClass : IX500DistinguishedName, CX500DistinguishedName
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Decode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedName, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X500NameFlags.XCN_CERT_NAME_STR_NONE)] X500NameFlags NameFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void Encode([In, MarshalAs(UnmanagedType.BStr)] string strName, [In, Optional, DefaultParameterValue(X500NameFlags.XCN_CERT_NAME_STR_NONE)] X500NameFlags NameFlags);

        // Properties
        [DispId(0x60020003)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }               
        [DispId(0x60020002)]
        public virtual extern string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, Guid("728AB322-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509AttributeClass))]
    public interface CX509Attribute : IX509Attribute
    {
    }

    [ComImport, CoClass(typeof(CX509AttributeArchiveKeyClass)), Guid("728AB327-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CX509AttributeArchiveKey : IX509AttributeArchiveKey
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E2027-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CX509AttributeArchiveKeyClass : IX509AttributeArchiveKey, CX509AttributeArchiveKey
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pKey, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCAXCert, [In, MarshalAs(UnmanagedType.Interface)] CObjectId pAlgorithm, [In] int EncryptionStrength);

        // Properties   
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60030003)]
        public virtual extern CObjectId EncryptionAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
        [DispId(0x60030004)]
        public virtual extern int EncryptionStrength { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }        
    }

    [ComImport, CoClass(typeof(CX509AttributeArchiveKeyHashClass)), Guid("728AB328-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CX509AttributeArchiveKeyHash : IX509AttributeArchiveKeyHash
    {
    }

    [ComImport, ClassInterface((short) 0), TypeLibType((short) 2), Guid("884E2028-217D-11DA-B2A4-000E7BBB2B09")]
    public class CX509AttributeArchiveKeyHashClass : IX509AttributeArchiveKeyHash, CX509AttributeArchiveKeyHash
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncodeFromEncryptedKeyBlob([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncryptedKeyBlob);

        // Properties
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }                
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }        
    }

    [ComImport, ClassInterface((short) 0), Guid("884E2022-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CX509AttributeClass : IX509Attribute, CX509Attribute
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);

        // Properties                
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, Guid("728AB325-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509AttributeClientIdClass))]
    public interface CX509AttributeClientId : IX509AttributeClientId
    {
    }

    [ComImport, TypeLibType((short) 2), Guid("884E2025-217D-11DA-B2A4-000E7BBB2B09"), ClassInterface((short) 0)]
    public class CX509AttributeClientIdClass : IX509AttributeClientId, CX509AttributeClientId
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In] RequestClientInfoClientId ClientId, [In, MarshalAs(UnmanagedType.BStr)] string strMachineDnsName, [In, MarshalAs(UnmanagedType.BStr)] string strUserSamName, [In, MarshalAs(UnmanagedType.BStr)] string strProcessName);

        // Properties     
        [DispId(0x60030002)]
        public virtual extern RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60030003)]
        public virtual extern string MachineDnsName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60030005)]
        public virtual extern string ProcessName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030005)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60030004)]
        public virtual extern string UserSamName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; }
    }

    [ComImport, Guid("728AB32B-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509AttributeCspProviderClass))]
    public interface CX509AttributeCspProvider : IX509AttributeCspProvider
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E202B-217D-11DA-B2A4-000E7BBB2B09")]
    public class CX509AttributeCspProviderClass : IX509AttributeCspProvider, CX509AttributeCspProvider
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In] X509KeySpec KeySpec, [In, MarshalAs(UnmanagedType.BStr)] string strProviderName, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strSignature);

        // Properties        
        [DispId(0x60030002)]
        public virtual extern X509KeySpec KeySpec { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60030003)]
        public virtual extern string ProviderName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }        
    }

    [ComImport, Guid("728AB324-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509AttributeExtensionsClass))]
    public interface CX509AttributeExtensions : IX509AttributeExtensions
    {
    }

    [ComImport, Guid("884E2024-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CX509AttributeExtensionsClass : IX509AttributeExtensions, CX509AttributeExtensions
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CX509Extensions pExtensions);

        // Properties        
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60030002)]
        public virtual extern CX509Extensions X509Extensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, CoClass(typeof(CX509AttributeOSVersionClass)), Guid("728AB32A-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CX509AttributeOSVersion : IX509AttributeOSVersion
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E202A-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CX509AttributeOSVersionClass : IX509AttributeOSVersion, CX509AttributeOSVersion
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In, MarshalAs(UnmanagedType.BStr)] string strOSVersion);

        // Properties        
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60030002)]
        public virtual extern string OSVersion { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, CoClass(typeof(CX509AttributeRenewalCertificateClass)), Guid("728AB326-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CX509AttributeRenewalCertificate : IX509AttributeRenewalCertificate
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E2026-217D-11DA-B2A4-000E7BBB2B09")]
    public class CX509AttributeRenewalCertificateClass : IX509AttributeRenewalCertificate, CX509AttributeRenewalCertificate
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCert);

        // Properties        
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        
    }

    [ComImport, Guid("728AB323-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509AttributesClass))]
    public interface CX509Attributes : IX509Attributes
    {
    }

    [ComImport, Guid("884E2023-217D-11DA-B2A4-000E7BBB2B09"), ClassInterface((short) 0), TypeLibType((short) 2)]
    public class CX509AttributesClass : IX509Attributes, CX509Attributes, IEnumerable
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        public virtual extern void Add([In, MarshalAs(UnmanagedType.Interface)] CX509Attribute pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        public virtual extern void Clear();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1), DispId(-4)]
        public virtual extern IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        public virtual extern void Remove([In] int Index);

        // Properties        
        [DispId(1)]
        public virtual extern int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [DispId(0)]
        public virtual extern CX509Attribute this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
    }

    [ComImport, Guid("728AB35A-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509CertificateRequestCertificateClass))]
    public interface CX509CertificateRequestCertificate : IX509CertificateRequestCertificate2
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E2043-217D-11DA-B2A4-000E7BBB2B09")]
    public class CX509CertificateRequestCertificateClass : IX509CertificateRequestCertificate2, CX509CertificateRequestCertificate
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040000)]
        public virtual extern void CheckPublicKeySignature([In, MarshalAs(UnmanagedType.Interface)] CX509PublicKey pPublicKey);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030005)]
        public virtual extern void CheckSignature([In, Optional, DefaultParameterValue(Pkcs10AllowedSignatureTypes.AllowedKeySignature)] Pkcs10AllowedSignatureTypes AllowedSignatureTypes);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void Encode();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003001b)]
        public virtual extern ICspStatuses GetCspStatuses([In] X509KeySpec KeySpec);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        public virtual extern IX509CertificateRequest GetInnerRequest([In] InnerRequestLevel Level);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)]
        public virtual extern void InitializeDecode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedData, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        public virtual extern void InitializeFromCertificate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X509RequestInheritOptions.InheritDefault)] X509RequestInheritOptions InheritOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeFromPrivateKey([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pPrivateKey, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050001)]
        public virtual extern void InitializeFromPrivateKeyTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pPrivateKey, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        public virtual extern void InitializeFromPublicKey([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PublicKey pPublicKey, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050000)]
        public virtual extern void InitializeFromTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)]
        public virtual extern bool IsSmartCard();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        public virtual extern void ResetForEncode();

        // Properties
        [DispId(0x60020016)]
        public virtual extern bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }                   
        [DispId(0x60020010)]
        public virtual extern RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60030017)]
        public virtual extern CObjectIds CriticalExtensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030017)] get; }
        [DispId(0x60030015)]
        public virtual extern CCryptAttributes CryptAttributes { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030015)] get; }
        [DispId(0x60020012)]
        public virtual extern CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x6003000f)]
        public virtual extern ICspStatuses CspStatuses { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000f)] get; }
        [DispId(0x60020005)]
        public virtual extern X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020014)]
        public virtual extern CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x60040001)]
        public virtual extern CX500DistinguishedName Issuer { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040001)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040001)] set; }
        [DispId(0x60030013)]
        public virtual extern string KeyContainerNamePrefix { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030013)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030013)] set; }
        [DispId(0x60040005)]
        public virtual extern DateTime NotAfter { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040005)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040005)] set; }
        [DispId(0x60040003)]
        public virtual extern DateTime NotBefore { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040003)] set; }
        [DispId(0x6003000a)]
        public virtual extern bool NullSigned { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000a)] get; }
                
        [DispId(0x6002000e)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x6002000e)] set; }


        [DispId(0x60020008)]
        public virtual extern int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x60050002)]
        public virtual extern IX509EnrollmentPolicyServer PolicyServer { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050002)] get; }
        [DispId(0x60030009)]
        public virtual extern CX509PrivateKey PrivateKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030009)] get; }
        [DispId(0x60030008)]
        public virtual extern CX509PublicKey PublicKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030008)] get; }                
        [DispId(0x6003000b)]
        public virtual extern bool ReuseKey { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000b)] get; }      
        [DispId(0x60030012)]
        public virtual extern IX509SignatureInformation SignatureInformation { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030012)] get; }
        [DispId(0x60040009)]
        public virtual extern CSignerCertificate SignerCertificate { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040009)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040009)] set; }
        [DispId(0x60020006)]
        public virtual extern bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x60030010)]
        public virtual extern bool SmimeCapabilities { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030010)] set; }
        [DispId(0x6003000d)]
        public virtual extern CX500DistinguishedName Subject { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000d)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000d)] set; }
        [DispId(0x6002000c)]
        public virtual extern bool SuppressDefaults { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }
        [DispId(0x60030018)]
        public virtual extern CObjectIds SuppressOids { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030018)] get; }
        [DispId(0x60050003)]
        public virtual extern IX509CertificateTemplate Template { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050003)] get; }
        [DispId(0x60030007)]
        public virtual extern CObjectId TemplateObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030007)] get; }
        [DispId(0x60020004)]
        public virtual extern X509RequestType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x6002000a)]
        public virtual extern string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x60030016)]
        public virtual extern CX509Extensions X509Extensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030016)] get; }
    }

    [ComImport, CoClass(typeof(CX509CertificateRequestCmcClass)), Guid("728AB35D-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CX509CertificateRequestCmc : IX509CertificateRequestCmc2
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E2045-217D-11DA-B2A4-000E7BBB2B09")]
    public class CX509CertificateRequestCmcClass : IX509CertificateRequestCmc2, CX509CertificateRequestCmc
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050005)]
        public virtual extern void CheckCertificateSignature([In, MarshalAs(UnmanagedType.Interface)] CSignerCertificate pSignerCertificate, [In] bool ValidateCertificateChain);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050004)]
        public virtual extern void CheckSignature([In, Optional, DefaultParameterValue(Pkcs10AllowedSignatureTypes.AllowedKeySignature)] Pkcs10AllowedSignatureTypes AllowedSignatureTypes);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void Encode();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        public virtual extern IX509CertificateRequest GetInnerRequest([In] InnerRequestLevel Level);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        public virtual extern void InitializeDecode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedData, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeFromCertificate([In] X509CertificateEnrollmentContext Context, [In] bool RenewalRequest, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X509RequestInheritOptions.InheritDefault)] X509RequestInheritOptions InheritOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        public virtual extern void InitializeFromInnerRequest([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pInnerRequest);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050001)]
        public virtual extern void InitializeFromInnerRequestTemplate([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pInnerRequest, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040000)]
        public virtual extern void InitializeFromInnerRequestTemplateName([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pInnerRequest, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050000)]
        public virtual extern void InitializeFromTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        public virtual extern void ResetForEncode();

        // Properties
        [DispId(0x60020016)]
        public virtual extern bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }
        [DispId(0x6004000d)]
        public virtual extern bool ArchivePrivateKey { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6004000d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6004000d)] set; }
     
     

        [DispId(0x60020010)]
        public virtual extern RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60040006)]
        public virtual extern CObjectIds CriticalExtensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040006)] get; }
        [DispId(0x60040003)]
        public virtual extern CCryptAttributes CryptAttributes { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040003)] get; }
        [DispId(0x60020012)]
        public virtual extern CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        
        [DispId(0x60040011)]
        public virtual extern CObjectId EncryptionAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040011)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040011)] set; }
        [DispId(0x60040013)]
        public virtual extern int EncryptionStrength { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040013)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040013)] set; }
        [DispId(0x60020005)]
        public virtual extern X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020014)]
        public virtual extern CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        
        [DispId(0x6002000e)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6004000f)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6004000f)] set; }

        [DispId(0x60040004)]
        public virtual extern IX509NameValuePairs NameValuePairs { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040004)] get; }
        [DispId(0x60040002)]
        public virtual extern bool NullSigned { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040002)] get; }
        [DispId(0x60020008)]
        public virtual extern int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x60050002)]
        public virtual extern IX509EnrollmentPolicyServer PolicyServer { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050002)] get; }        
        [DispId(0x60030004)]
        public virtual extern string RequesterName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] set; }
        
        [DispId(0x6004000c)]
        public virtual extern IX509SignatureInformation SignatureInformation { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6004000c)] get; }
        [DispId(0x60030006)]
        public virtual extern CSignerCertificate SignerCertificate { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)] set; }
        [DispId(0x60040016)]
        public virtual extern ISignerCertificates SignerCertificates { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040016)] get; }
        [DispId(0x60020006)]
        public virtual extern bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x6002000c)]
        public virtual extern bool SuppressDefaults { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }
        [DispId(0x60040007)]
        public virtual extern CObjectIds SuppressOids { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040007)] get; }
        [DispId(0x60050003)]
        public virtual extern IX509CertificateTemplate Template { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050003)] get; }
        [DispId(0x60040001)]
        public virtual extern CObjectId TemplateObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040001)] get; }
        [DispId(0x60040008)]
        public virtual extern int TransactionId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040008)] set; }
        [DispId(0x60020004)]
        public virtual extern X509RequestType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x6002000a)]
        public virtual extern string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x60040005)]
        public virtual extern CX509Extensions X509Extensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040005)] get; }
    }

    [ComImport, Guid("728AB35B-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509CertificateRequestPkcs10Class))]
    public interface CX509CertificateRequestPkcs10 : IX509CertificateRequestPkcs10V2
    {
    }

    [ComImport, TypeLibType((short) 2), Guid("884E2042-217D-11DA-B2A4-000E7BBB2B09"), ClassInterface((short) 0)]
    public class CX509CertificateRequestPkcs10Class : IX509CertificateRequestPkcs10V2, CX509CertificateRequestPkcs10
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030005)]
        public virtual extern void CheckSignature([In, Optional, DefaultParameterValue(Pkcs10AllowedSignatureTypes.AllowedKeySignature)] Pkcs10AllowedSignatureTypes AllowedSignatureTypes);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void Encode();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003001b)]
        public virtual extern ICspStatuses GetCspStatuses([In] X509KeySpec KeySpec);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        public virtual extern IX509CertificateRequest GetInnerRequest([In] InnerRequestLevel Level);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)]
        public virtual extern void InitializeDecode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedData, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        public virtual extern void InitializeFromCertificate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X509RequestInheritOptions.InheritDefault)] X509RequestInheritOptions InheritOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeFromPrivateKey([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pPrivateKey, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040001)]
        public virtual extern void InitializeFromPrivateKeyTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pPrivateKey, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        public virtual extern void InitializeFromPublicKey([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PublicKey pPublicKey, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040002)]
        public virtual extern void InitializeFromPublicKeyTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PublicKey pPublicKey, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040000)]
        public virtual extern void InitializeFromTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)]
        public virtual extern bool IsSmartCard();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        public virtual extern void ResetForEncode();

        // Properties
        [DispId(0x60020016)]
        public virtual extern bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }
                
        [DispId(0x60020010)]
        public virtual extern RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60030017)]
        public virtual extern CObjectIds CriticalExtensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030017)] get; }
        [DispId(0x60030015)]
        public virtual extern CCryptAttributes CryptAttributes { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030015)] get; }
        [DispId(0x60020012)]
        public virtual extern CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x6003000f)]
        public virtual extern ICspStatuses CspStatuses { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000f)] get; }
        [DispId(0x60020005)]
        public virtual extern X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020014)]
        public virtual extern CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x60030013)]
        public virtual extern string KeyContainerNamePrefix { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030013)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030013)] set; }
        [DispId(0x6003000a)]
        public virtual extern bool NullSigned { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000a)] get; }

        
        [DispId(0x6002000e)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x6002000e)] set; }


        [DispId(0x60020008)]
        public virtual extern int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x60040003)]
        public virtual extern IX509EnrollmentPolicyServer PolicyServer { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040003)] get; }
        [DispId(0x60030009)]
        public virtual extern CX509PrivateKey PrivateKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030009)] get; }
        [DispId(0x60030008)]
        public virtual extern CX509PublicKey PublicKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030008)] get; }        
        [DispId(0x6003000b)]
        public virtual extern bool ReuseKey { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000b)] get; }        
        [DispId(0x60030012)]
        public virtual extern IX509SignatureInformation SignatureInformation { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030012)] get; }
        [DispId(0x60020006)]
        public virtual extern bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x60030010)]
        public virtual extern bool SmimeCapabilities { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030010)] set; }
        [DispId(0x6003000d)]
        public virtual extern CX500DistinguishedName Subject { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000d)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000d)] set; }
        [DispId(0x6002000c)]
        public virtual extern bool SuppressDefaults { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }
        [DispId(0x60030018)]
        public virtual extern CObjectIds SuppressOids { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030018)] get; }
        [DispId(0x60040004)]
        public virtual extern IX509CertificateTemplate Template { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040004)] get; }
        [DispId(0x60030007)]
        public virtual extern CObjectId TemplateObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030007)] get; }
        [DispId(0x60020004)]
        public virtual extern X509RequestType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x6002000a)]
        public virtual extern string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x60030016)]
        public virtual extern CX509Extensions X509Extensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030016)] get; }
    }

    [ComImport, Guid("728AB35C-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509CertificateRequestPkcs7Class))]
    public interface CX509CertificateRequestPkcs7 : IX509CertificateRequestPkcs7V2
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E2044-217D-11DA-B2A4-000E7BBB2B09")]
    public class CX509CertificateRequestPkcs7Class : IX509CertificateRequestPkcs7V2, CX509CertificateRequestPkcs7
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040003)]
        public virtual extern void CheckCertificateSignature([In] bool ValidateCertificateChain);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void Encode();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        public virtual extern IX509CertificateRequest GetInnerRequest([In] InnerRequestLevel Level);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        public virtual extern void InitializeDecode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedData, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeFromCertificate([In] X509CertificateEnrollmentContext Context, [In] bool RenewalRequest, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X509RequestInheritOptions.InheritDefault)] X509RequestInheritOptions InheritOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        public virtual extern void InitializeFromInnerRequest([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pInnerRequest);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040000)]
        public virtual extern void InitializeFromTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        public virtual extern void ResetForEncode();

        // Properties
        [DispId(0x60020016)]
        public virtual extern bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }        
                
        [DispId(0x60020010)]
        public virtual extern RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60020012)]
        public virtual extern CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x60020005)]
        public virtual extern X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020014)]
        public virtual extern CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x60020008)]
        public virtual extern int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x60040001)]
        public virtual extern IX509EnrollmentPolicyServer PolicyServer { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040001)] get; }
        

        [DispId(0x6002000e)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x6002000e)] set; }


        [DispId(0x60030004)]
        public virtual extern string RequesterName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] set; }
        [DispId(0x60030006)]
        public virtual extern CSignerCertificate SignerCertificate { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)] set; }
        [DispId(0x60020006)]
        public virtual extern bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x6002000c)]
        public virtual extern bool SuppressDefaults { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }
        [DispId(0x60040002)]
        public virtual extern IX509CertificateTemplate Template { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040002)] get; }
        [DispId(0x60020004)]
        public virtual extern X509RequestType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x6002000a)]
        public virtual extern string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
    }

    [ComImport, CoClass(typeof(CX509CertificateTemplateADWritableClass)), Guid("F49466A7-395A-4E9E-B6E7-32B331600DC0")]
    public interface CX509CertificateTemplateADWritable : IX509CertificateTemplateWritable
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("8336E323-2E6A-4A04-937C-548F681839B3")]
    public class CX509CertificateTemplateADWritableClass : IX509CertificateTemplateWritable, CX509CertificateTemplateADWritable
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void Commit([In] CommitTemplateFlags commitFlags, [In, MarshalAs(UnmanagedType.BStr)] string strServerContext);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pValue);

        // Properties        
        [DispId(0x60020002)]
        public virtual extern object this[EnrollmentTemplateProperty Property] { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        public virtual extern IX509CertificateTemplate Template { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
    }

    [ComImport, Guid("728AB350-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509EnrollmentClass))]
    public interface CX509Enrollment : IX509Enrollment2
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E2046-217D-11DA-B2A4-000E7BBB2B09")]
    public class CX509EnrollmentClass : IX509Enrollment2, CX509Enrollment
    {
        // Methods
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern string CreatePFX([In, MarshalAs(UnmanagedType.BStr)] string strPassword, [In] PFXExportOptions ExportOptions, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        public virtual extern string CreateRequest([In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)]
        public virtual extern void Enroll();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        public virtual extern void InitializeFromRequest([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pRequest);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeFromTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void InstallResponse([In] InstallResponseRestrictionFlags Restrictions, [In, MarshalAs(UnmanagedType.BStr)] string strResponse, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strPassword);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InstallResponse2([In] InstallResponseRestrictionFlags Restrictions, [In, MarshalAs(UnmanagedType.BStr)] string strResponse, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strPassword, [In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentPolicyServerUrl, [In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentPolicyServerID, [In] PolicyServerUrlFlags EnrollmentPolicyServerFlags, [In] X509EnrollmentAuthFlags AuthFlags);

        // Properties
        [DispId(0x60020016)]
        public virtual extern string CAConfigString { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; }
        
        
        [DispId(0x6002000f)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000f)] get; }
        [DispId(0x60020013)]
        public virtual extern string CertificateDescription { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020013)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020013)] set; }
        [DispId(0x60020011)]
        public virtual extern string CertificateFriendlyName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020011)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020011)] set; }
        [DispId(0x6002000d)]
        public virtual extern X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000d)] get; }
        [DispId(0x6002000c)]
        public virtual extern IX509NameValuePairs NameValuePairs { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; }
        [DispId(0x6002000a)]
        public virtual extern int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x60030002)]
        public virtual extern IX509EnrollmentPolicyServer PolicyServer { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60020007)]
        public virtual extern IX509CertificateRequest Request { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; }
        [DispId(0x60020015)]
        public virtual extern int RequestId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020015)] get; }
        [DispId(0x60030004)]
        public virtual extern string RequestIdString { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; }       
        [DispId(0x60020008)]
        public virtual extern bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000e)]
        public virtual extern IX509EnrollmentStatus Status { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; }
        [DispId(0x60030003)]
        public virtual extern IX509CertificateTemplate Template { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
    }

    [ComImport, Guid("728AB351-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509EnrollmentHelperClass))]
    public interface CX509EnrollmentHelper : IX509EnrollmentHelper
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E2050-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CX509EnrollmentHelperClass : IX509EnrollmentHelper, CX509EnrollmentHelper
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void AddEnrollmentServer([In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentServerURI, [In] X509EnrollmentAuthFlags AuthFlags, [In, MarshalAs(UnmanagedType.BStr)] string strCredential, [In, MarshalAs(UnmanagedType.BStr)] string strPassword);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void AddPolicyServer([In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentPolicyServerURI, [In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentPolicyID, [In] PolicyServerUrlFlags EnrollmentPolicyServerFlags, [In] X509EnrollmentAuthFlags AuthFlags, [In, MarshalAs(UnmanagedType.BStr)] string strCredential, [In, MarshalAs(UnmanagedType.BStr)] string strPassword);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        public virtual extern string Enroll([In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentPolicyServerURI, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName, [In] EncodingType Encoding, [In] WebEnrollmentFlags enrollFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        public virtual extern void Initialize([In] X509CertificateEnrollmentContext Context);
    }

    [ComImport, Guid("13B79026-2181-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509EnrollmentPolicyActiveDirectoryClass))]
    public interface CX509EnrollmentPolicyActiveDirectory : IX509EnrollmentPolicyServer
    {
    }

    [ComImport, Guid("91F39027-217F-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CX509EnrollmentPolicyActiveDirectoryClass : IX509EnrollmentPolicyServer, CX509EnrollmentPolicyActiveDirectory
    {
        // Methods
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020015)]
        public virtual extern object Export([In] X509EnrollmentPolicyExportFlags exportFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)]
        public virtual extern bool GetAllowUnTrustedCA();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020011)]
        public virtual extern X509EnrollmentAuthFlags GetAuthFlags();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)]
        public virtual extern string GetCacheDir();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000f)]
        public virtual extern string GetCachePath();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)]
        public virtual extern ICertificationAuthorities GetCAs();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        public virtual extern ICertificationAuthorities GetCAsForTemplate([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern CObjectIds GetCustomOids();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000b)]
        public virtual extern string GetFriendlyName();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)]
        public virtual extern bool GetIsDefaultCEP();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)]
        public virtual extern DateTime GetLastUpdateTime();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)]
        public virtual extern DateTime GetNextUpdateTime();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)]
        public virtual extern string GetPolicyServerId();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)]
        public virtual extern string GetPolicyServerUrl();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        public virtual extern IX509CertificateTemplates GetTemplates();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000d)]
        public virtual extern bool GetUseClientId();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.BStr)] string bstrPolicyServerUrl, [In, MarshalAs(UnmanagedType.BStr)] string bstrPolicyServerId, [In] X509EnrollmentAuthFlags AuthFlags, [In] bool fIsUnTrusted, [In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)]
        public virtual extern void InitializeImport([In, MarshalAs(UnmanagedType.Struct)] object val);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void LoadPolicy([In, ComAliasName("CERTENROLLLib.X509EnrollmentPolicyLoadOption")] X509EnrollmentPolicyLoadOption option);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020013)]
        public virtual extern bool QueryChanges();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)]
        public virtual extern void SetCredential([In] int hWndParent, [In] X509EnrollmentAuthFlags flag, [In, MarshalAs(UnmanagedType.BStr)] string strCredential, [In, MarshalAs(UnmanagedType.BStr)] string strPassword);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void Validate();

        // Properties        
        [DispId(0x60020016)]
        public virtual extern uint Cost { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }
    }

    [ComImport, Guid("13B79026-2181-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509EnrollmentPolicyWebServiceClass))]
    public interface CX509EnrollmentPolicyWebService : IX509EnrollmentPolicyServer
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("91F39028-217F-11DA-B2A4-000E7BBB2B09")]
    public class CX509EnrollmentPolicyWebServiceClass : IX509EnrollmentPolicyServer, CX509EnrollmentPolicyWebService
    {
        // Methods
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020015)]
        public virtual extern object Export([In] X509EnrollmentPolicyExportFlags exportFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)]
        public virtual extern bool GetAllowUnTrustedCA();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020011)]
        public virtual extern X509EnrollmentAuthFlags GetAuthFlags();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)]
        public virtual extern string GetCacheDir();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000f)]
        public virtual extern string GetCachePath();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)]
        public virtual extern ICertificationAuthorities GetCAs();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        public virtual extern ICertificationAuthorities GetCAsForTemplate([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern CObjectIds GetCustomOids();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000b)]
        public virtual extern string GetFriendlyName();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)]
        public virtual extern bool GetIsDefaultCEP();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)]
        public virtual extern DateTime GetLastUpdateTime();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)]
        public virtual extern DateTime GetNextUpdateTime();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)]
        public virtual extern string GetPolicyServerId();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)]
        public virtual extern string GetPolicyServerUrl();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        public virtual extern IX509CertificateTemplates GetTemplates();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000d)]
        public virtual extern bool GetUseClientId();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.BStr)] string bstrPolicyServerUrl, [In, MarshalAs(UnmanagedType.BStr)] string bstrPolicyServerId, [In] X509EnrollmentAuthFlags AuthFlags, [In] bool fIsUnTrusted, [In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)]
        public virtual extern void InitializeImport([In, MarshalAs(UnmanagedType.Struct)] object val);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void LoadPolicy([In, ComAliasName("CERTENROLLLib.X509EnrollmentPolicyLoadOption")] X509EnrollmentPolicyLoadOption option);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020013)]
        public virtual extern bool QueryChanges();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)]
        public virtual extern void SetCredential([In] int hWndParent, [In] X509EnrollmentAuthFlags flag, [In, MarshalAs(UnmanagedType.BStr)] string strCredential, [In, MarshalAs(UnmanagedType.BStr)] string strPassword);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void Validate();

        // Properties        
        [DispId(0x60020016)]
        public virtual extern uint Cost { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }
    }

    [ComImport, Guid("728AB349-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509EnrollmentWebClassFactoryClass))]
    public interface CX509EnrollmentWebClassFactory : IX509EnrollmentWebClassFactory
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E2049-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CX509EnrollmentWebClassFactoryClass : IX509EnrollmentWebClassFactory, CX509EnrollmentWebClassFactory
    {
        // Methods
        [return: MarshalAs(UnmanagedType.IUnknown)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern object CreateObject([In, MarshalAs(UnmanagedType.BStr)] string strProgID);
    }

    [ComImport, Guid("728AB30D-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509ExtensionClass))]
    public interface CX509Extension : IX509Extension
    {
    }

    [ComImport, CoClass(typeof(CX509ExtensionAlternativeNamesClass)), Guid("728AB315-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CX509ExtensionAlternativeNames : IX509ExtensionAlternativeNames
    {
    }

    [ComImport, ClassInterface((short) 0), TypeLibType((short) 2), Guid("884E2015-217D-11DA-B2A4-000E7BBB2B09")]
    public class CX509ExtensionAlternativeNamesClass : IX509ExtensionAlternativeNames, CX509ExtensionAlternativeNames
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CAlternativeNames pValue);

        // Properties
        [DispId(0x60030002)]
        public virtual extern CAlternativeNames AlternativeNames { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
                
        [DispId(0x60020003)]
        public virtual extern bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, Guid("728AB318-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509ExtensionAuthorityKeyIdentifierClass))]
    public interface CX509ExtensionAuthorityKeyIdentifier : IX509ExtensionAuthorityKeyIdentifier
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E2018-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CX509ExtensionAuthorityKeyIdentifierClass : IX509ExtensionAuthorityKeyIdentifier, CX509ExtensionAuthorityKeyIdentifier
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strKeyIdentifier);

        // Properties
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
               
        [DispId(0x60020003)]
        public virtual extern bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
                
    }

    [ComImport, Guid("728AB316-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509ExtensionBasicConstraintsClass))]
    public interface CX509ExtensionBasicConstraints : IX509ExtensionBasicConstraints
    {
    }

    [ComImport, Guid("884E2016-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CX509ExtensionBasicConstraintsClass : IX509ExtensionBasicConstraints, CX509ExtensionBasicConstraints
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In] bool IsCA, [In] int PathLenConstraint);

        // Properties        
        [DispId(0x60020003)]
        public virtual extern bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60030002)]
        public virtual extern bool IsCA { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60030003)]
        public virtual extern int PathLenConstraint { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, CoClass(typeof(CX509ExtensionCertificatePoliciesClass)), Guid("728AB320-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CX509ExtensionCertificatePolicies : IX509ExtensionCertificatePolicies
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E2020-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CX509ExtensionCertificatePoliciesClass : IX509ExtensionCertificatePolicies, CX509ExtensionCertificatePolicies
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CCertificatePolicies pValue);

        // Properties        
        [DispId(0x60020003)]
        public virtual extern bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60030002)]
        public virtual extern CCertificatePolicies Policies { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, ClassInterface((short) 0), Guid("884E200D-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CX509ExtensionClass : IX509Extension, CX509Extension
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);

        // Properties        
        [DispId(0x60020003)]
        public virtual extern bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, CoClass(typeof(CX509ExtensionEnhancedKeyUsageClass)), Guid("728AB310-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CX509ExtensionEnhancedKeyUsage : IX509ExtensionEnhancedKeyUsage
    {
    }

    [ComImport, Guid("884E2010-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CX509ExtensionEnhancedKeyUsageClass : IX509ExtensionEnhancedKeyUsage, CX509ExtensionEnhancedKeyUsage
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CObjectIds pValue);

        // Properties        
        [DispId(0x60020003)]
        public virtual extern bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60030002)]
        public virtual extern CObjectIds EnhancedKeyUsage { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, Guid("728AB30F-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509ExtensionKeyUsageClass))]
    public interface CX509ExtensionKeyUsage : IX509ExtensionKeyUsage
    {
    }

    [ComImport, Guid("884E200F-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CX509ExtensionKeyUsageClass : IX509ExtensionKeyUsage, CX509ExtensionKeyUsage
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In] X509KeyUsageFlags UsageFlags);

        // Properties        
        [DispId(0x60020003)]
        public virtual extern bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60030002)]
        public virtual extern X509KeyUsageFlags KeyUsage { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, Guid("728AB321-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509ExtensionMSApplicationPoliciesClass))]
    public interface CX509ExtensionMSApplicationPolicies : IX509ExtensionMSApplicationPolicies
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E2021-217D-11DA-B2A4-000E7BBB2B09")]
    public class CX509ExtensionMSApplicationPoliciesClass : IX509ExtensionMSApplicationPolicies, CX509ExtensionMSApplicationPolicies
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CCertificatePolicies pValue);

        // Properties     
        [DispId(0x60020003)]
        public virtual extern bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60030002)]
        public virtual extern CCertificatePolicies Policies { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, Guid("728AB30E-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509ExtensionsClass))]
    public interface CX509Extensions : IX509Extensions
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E200E-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CX509ExtensionsClass : IX509Extensions, CX509Extensions, IEnumerable
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        public virtual extern void Add([In, MarshalAs(UnmanagedType.Interface)] CX509Extension pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)]
        public virtual extern void AddRange([In, MarshalAs(UnmanagedType.Interface)] CX509Extensions pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        public virtual extern void Clear();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        public virtual extern IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        public virtual extern void Remove([In] int Index);

        // Properties        
        [DispId(1)]
        public virtual extern int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [DispId(0x60020006)]
        public virtual extern int this[CObjectId pObjectId] { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
        [DispId(0)]
        public virtual extern CX509Extension this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
    }

    [ComImport, Guid("728AB31B-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509ExtensionSmimeCapabilitiesClass))]
    public interface CX509ExtensionSmimeCapabilities : IX509ExtensionSmimeCapabilities
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E201B-217D-11DA-B2A4-000E7BBB2B09")]
    public class CX509ExtensionSmimeCapabilitiesClass : IX509ExtensionSmimeCapabilities, CX509ExtensionSmimeCapabilities
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CSmimeCapabilities pValue);

        // Properties        
        [DispId(0x60020003)]
        public virtual extern bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60030002)]
        public virtual extern CSmimeCapabilities SmimeCapabilities { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, Guid("728AB317-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509ExtensionSubjectKeyIdentifierClass))]
    public interface CX509ExtensionSubjectKeyIdentifier : IX509ExtensionSubjectKeyIdentifier
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("884E2017-217D-11DA-B2A4-000E7BBB2B09")]
    public class CX509ExtensionSubjectKeyIdentifierClass : IX509ExtensionSubjectKeyIdentifier, CX509ExtensionSubjectKeyIdentifier
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strKeyIdentifier);

        // Properties        
        [DispId(0x60020003)]
        public virtual extern bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }                
    }

    [ComImport, Guid("728AB312-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509ExtensionTemplateClass))]
    public interface CX509ExtensionTemplate : IX509ExtensionTemplate
    {
    }

    [ComImport, Guid("884E2012-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CX509ExtensionTemplateClass : IX509ExtensionTemplate, CX509ExtensionTemplate
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CObjectId pTemplateOid, [In] int MajorVersion, [In] int MinorVersion);

        // Properties        
        [DispId(0x60020003)]
        public virtual extern bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60030003)]
        public virtual extern int MajorVersion { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
        [DispId(0x60030004)]
        public virtual extern int MinorVersion { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60030002)]
        public virtual extern CObjectId TemplateOid { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, Guid("728AB311-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509ExtensionTemplateNameClass))]
    public interface CX509ExtensionTemplateName : IX509ExtensionTemplateName
    {
    }

    [ComImport, Guid("884E2011-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2), ClassInterface((short) 0)]
    public class CX509ExtensionTemplateNameClass : IX509ExtensionTemplateName, CX509ExtensionTemplateName
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        public virtual extern void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        public virtual extern void InitializeEncode([In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);

        // Properties        
        [DispId(0x60020003)]
        public virtual extern bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60020001)]
        public virtual extern CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60030002)]
        public virtual extern string TemplateName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, Guid("728AB352-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509MachineEnrollmentFactoryClass))]
    public interface CX509MachineEnrollmentFactory : IX509MachineEnrollmentFactory
    {
    }

    [ComImport, TypeLibType((short) 2), Guid("884E2051-217D-11DA-B2A4-000E7BBB2B09"), ClassInterface((short) 0)]
    public class CX509MachineEnrollmentFactoryClass : IX509MachineEnrollmentFactory, CX509MachineEnrollmentFactory
    {
        // Methods
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern CX509EnrollmentHelper CreateObject([In, MarshalAs(UnmanagedType.BStr)] string strProgID);
    }

    [ComImport, Guid("728AB33F-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509NameValuePairClass))]
    public interface CX509NameValuePair : IX509NameValuePair
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E203F-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CX509NameValuePairClass : IX509NameValuePair, CX509NameValuePair
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.BStr)] string strName, [In, MarshalAs(UnmanagedType.BStr)] string strValue);

        // Properties        
        [DispId(0x60020002)]
        public virtual extern string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020001)]
        public virtual extern string Value { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
    }

    [ComImport, Guid("884E204B-217D-11DA-B2A4-000E7BBB2B09"), CoClass(typeof(CX509PolicyServerListManagerClass))]
    public interface CX509PolicyServerListManager : IX509PolicyServerListManager
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("91F39029-217F-11DA-B2A4-000E7BBB2B09")]
    public class CX509PolicyServerListManagerClass : IX509PolicyServerListManager, CX509PolicyServerListManager, IEnumerable
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        public virtual extern void Add([In, MarshalAs(UnmanagedType.Interface)] CX509PolicyServerUrl pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        public virtual extern void Clear();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        public virtual extern IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern void Initialize([In] X509CertificateEnrollmentContext Context, [In] PolicyServerUrlFlags Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        public virtual extern void Remove([In] int Index);

        // Properties        
        [DispId(1)]
        public virtual extern int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [DispId(0)]
        public virtual extern CX509PolicyServerUrl this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
    }

    [ComImport, CoClass(typeof(CX509PolicyServerUrlClass)), Guid("884E204A-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CX509PolicyServerUrl : IX509PolicyServerUrl
    {
    }

    [ComImport, TypeLibType((short) 2), ClassInterface((short) 0), Guid("91F3902A-217F-11DA-B2A4-000E7BBB2B09")]
    public class CX509PolicyServerUrlClass : IX509PolicyServerUrl, CX509PolicyServerUrl
    {
        // Methods
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000b)]
        public virtual extern string GetStringProperty([In] PolicyServerUrlPropertyID PropertyId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)]
        public virtual extern void RemoveFromRegistry([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)]
        public virtual extern void SetStringProperty([In] PolicyServerUrlPropertyID PropertyId, [In, MarshalAs(UnmanagedType.BStr)] string pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000d)]
        public virtual extern void UpdateRegistry([In] X509CertificateEnrollmentContext Context);

        // Properties
        [DispId(0x60020007)]
        public virtual extern X509EnrollmentAuthFlags AuthFlags { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] set; }                
        [DispId(0x60020009)]
        public virtual extern uint Cost { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] set; }
        [DispId(0x60020003)]
        public virtual extern bool Default { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60020005)]
        public virtual extern PolicyServerUrlFlags Flags { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] set; }
        [DispId(0x60020001)]
        public virtual extern string Url { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] set; }
    }

    [ComImport, CoClass(typeof(CX509PrivateKeyClass)), Guid("728AB30C-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CX509PrivateKey : IX509PrivateKey
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E200C-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CX509PrivateKeyClass : IX509PrivateKey, CX509PrivateKey
    {
        // Methods
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        public virtual extern void Close();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void Create();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        public virtual extern void Delete();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern string Export([In, MarshalAs(UnmanagedType.BStr)] string strExportType, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)]
        public virtual extern CX509PublicKey ExportPublicKey();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        public virtual extern void Import([In, MarshalAs(UnmanagedType.BStr)] string strExportType, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedKey, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Open();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)]
        public virtual extern void Verify([In] X509PrivateKeyVerify VerifyType);

        // Properties
        [DispId(0x60020018)]
        public virtual extern CObjectId Algorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020018)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020018)] set; }
                
        [DispId(0x60020028)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020028)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020028)] set; }
        [DispId(0x60020008)]
        public virtual extern string ContainerName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000a)]
        public virtual extern string ContainerNamePrefix { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x6002000e)]
        public virtual extern CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] set; }
        [DispId(0x60020010)]
        public virtual extern CCspStatus CspStatus { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x6002002c)]
        public virtual extern bool DefaultContainer { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002c)] get; }
        [DispId(0x60020038)]
        public virtual extern string Description { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020038)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020038)] set; }
        [DispId(0x6002002d)]
        public virtual extern bool Existing { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002d)] set; }
        [DispId(0x6002001e)]
        public virtual extern X509PrivateKeyExportFlags ExportPolicy { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002001e)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002001e)] set; }
        [DispId(0x60020036)]
        public virtual extern string FriendlyName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020036)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020036)] set; }
        [DispId(0x60020022)]
        public virtual extern X509PrivateKeyProtection KeyProtection { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020022)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020022)] set; }
        [DispId(0x6002001a)]
        public virtual extern X509KeySpec KeySpec { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002001a)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002001a)] set; }
        [DispId(0x60020020)]
        public virtual extern X509PrivateKeyUsageFlags KeyUsage { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020020)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020020)] set; }
        [DispId(0x60020016)]
        public virtual extern bool LegacyCsp { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }
        [DispId(0x6002001c)]
        public virtual extern int Length { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002001c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002001c)] set; }
        [DispId(0x60020024)]
        public virtual extern bool MachineContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020024)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020024)] set; }
        [DispId(0x6002002b)]
        public virtual extern bool Opened { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002b)] get; }
        [DispId(0x60020031)]
        public virtual extern int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020031)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020031)] set; }
        [DispId(0x60020035)]
        public virtual extern string Pin { [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020035)] set; }
        [DispId(0x60020012)]
        public virtual extern string ProviderName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x60020014)]
        public virtual extern X509ProviderType ProviderType { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x6002000c)]
        public virtual extern string ReaderName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }
        [DispId(0x60020026)]
        public virtual extern string SecurityDescriptor { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020026)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020026)] set; }
        [DispId(0x6002002f)]
        public virtual extern bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002f)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002f)] set; }
        [DispId(0x60020033)]
        public virtual extern string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020033)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020033)] set; }
        [DispId(0x6002002a)]
        public virtual extern string UniqueContainerName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002a)] get; }
    }

    [ComImport, CoClass(typeof(CX509PublicKeyClass)), Guid("728AB30B-217D-11DA-B2A4-000E7BBB2B09")]
    public interface CX509PublicKey : IX509PublicKey
    {
    }

    [ComImport, ClassInterface((short) 0), Guid("884E200B-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 2)]
    public class CX509PublicKeyClass : IX509PublicKey, CX509PublicKey
    {
        // Methods
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        public virtual extern string ComputeKeyIdentifier([In] KeyIdentifierHashAlgorithm Algorithm, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        public virtual extern void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedKey, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedParameters, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        public virtual extern void InitializeFromEncodedPublicKeyInfo([In, MarshalAs(UnmanagedType.BStr)] string strEncodedPublicKeyInfo, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);

        // Properties
        [DispId(0x60020002)]
        public virtual extern CObjectId Algorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        
        [DispId(0x60020004)]
        public virtual extern string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }                        
        [DispId(0x60020003)]
        public virtual extern int Length { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
    }

    public enum EncodingType
    {
        XCN_CRYPT_STRING_ANY = 7,
        XCN_CRYPT_STRING_BASE64 = 1,
        XCN_CRYPT_STRING_BASE64_ANY = 6,
        XCN_CRYPT_STRING_BASE64HEADER = 0,
        XCN_CRYPT_STRING_BASE64REQUESTHEADER = 3,
        XCN_CRYPT_STRING_BASE64X509CRLHEADER = 9,
        XCN_CRYPT_STRING_BINARY = 2,
        XCN_CRYPT_STRING_HASHDATA = 0x10000000,
        XCN_CRYPT_STRING_HEX = 4,
        XCN_CRYPT_STRING_HEX_ANY = 8,
        XCN_CRYPT_STRING_HEXADDR = 10,
        XCN_CRYPT_STRING_HEXASCII = 5,
        XCN_CRYPT_STRING_HEXASCIIADDR = 11,
        XCN_CRYPT_STRING_HEXRAW = 12,
        XCN_CRYPT_STRING_NOCR = -2147483648,
        XCN_CRYPT_STRING_NOCRLF = 0x40000000,
        XCN_CRYPT_STRING_STRICT = 0x20000000
    }

    public enum EnrollmentCAProperty
    {
        CAPropCertificate = 7,
        CAPropCertificateTypes = 6,
        CAPropCommonName = 1,
        CAPropDescription = 8,
        CAPropDistinguishedName = 2,
        CAPropDNSName = 5,
        CAPropRenewalOnly = 12,
        CAPropSanitizedName = 3,
        CAPropSanitizedShortName = 4,
        CAPropSecurity = 11,
        CAPropSiteName = 10,
        CAPropWebServers = 9
    }

    public enum EnrollmentDisplayStatus
    {
        DisplayNo,
        DisplayYes
    }

    public enum EnrollmentEnrollStatus
    {
        EnrollDenied = 0x100,
        Enrolled = 1,
        EnrollError = 0x10,
        EnrollPended = 2,
        EnrollSkipped = 0x40,
        EnrollUIDeferredEnrollmentRequired = 4,
        EnrollUnknown = 0x20
    }

    public enum EnrollmentPolicyServerPropertyFlags
    {
        DefaultNone,
        DefaultPolicyServer
    }

    public enum EnrollmentSelectionStatus
    {
        SelectedNo,
        SelectedYes
    }

    public enum EnrollmentTemplateProperty
    {
        TemplatePropAsymmetricAlgorithm = 0x12,
        TemplatePropCertificatePolicies = 0x10,
        TemplatePropCommonName = 1,
        TemplatePropCryptoProviders = 4,
        TemplatePropDescription = 6,
        TemplatePropEKUs = 3,
        TemplatePropEnrollmentFlags = 0x18,
        TemplatePropExtensions = 0x1d,
        TemplatePropFriendlyName = 2,
        TemplatePropGeneralFlags = 0x1b,
        TemplatePropHashAlgorithm = 0x16,
        TemplatePropKeySecurityDescriptor = 0x13,
        TemplatePropKeySpec = 7,
        TemplatePropKeyUsage = 0x17,
        TemplatePropMajorRevision = 5,
        TemplatePropMinimumKeySize = 11,
        TemplatePropMinorRevision = 9,
        TemplatePropOID = 12,
        TemplatePropPrivateKeyFlags = 0x1a,
        TemplatePropRACertificatePolicies = 14,
        TemplatePropRAEKUs = 15,
        TemplatePropRASignatureCount = 10,
        TemplatePropRenewalPeriod = 0x1f,
        TemplatePropSchemaVersion = 8,
        TemplatePropSecurityDescriptor = 0x1c,
        TemplatePropSubjectNameFlags = 0x19,
        TemplatePropSupersede = 13,
        TemplatePropSymmetricAlgorithm = 20,
        TemplatePropSymmetricKeyLength = 0x15,
        TemplatePropV1ApplicationPolicy = 0x11,
        TemplatePropValidityPeriod = 30
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB313-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IAlternativeName
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromString([In] AlternativeNameType Type, [In, MarshalAs(UnmanagedType.BStr)] string strValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeFromRawData([In] AlternativeNameType Type, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strRawData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void InitializeFromOtherName([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strRawData, [In] bool ToBeWrapped);
        [DispId(0x60020003)]
        AlternativeNameType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
        [DispId(0x60020004)]
        string strValue { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB314-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IAlternativeNames : IEnumerable
    {
        [DispId(0)]
        CAlternativeName this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        
        
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CAlternativeName pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB302-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IBinaryConverter
    {
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        string StringToString([In, MarshalAs(UnmanagedType.BStr)] string strEncodedIn, [In] EncodingType EncodingIn, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        string VariantByteArrayToString([In, MarshalAs(UnmanagedType.Struct)] ref object pvarByteArray, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        object StringToVariantByteArray([In, MarshalAs(UnmanagedType.BStr)] string strEncoded, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB31F-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ICertificatePolicies : IEnumerable
    {
        [DispId(0)]
        CCertificatePolicy this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1), DispId(-4)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CCertificatePolicy pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB31E-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ICertificatePolicy
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pValue);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        CPolicyQualifiers PolicyQualifiers { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, Guid("13B79005-2181-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface ICertificationAuthorities : IEnumerable
    {
        [DispId(0)]
        ICertificationAuthority this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] ICertificationAuthority pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void ComputeSiteCosts();
        [DispId(0x60020007)]
        ICertificationAuthority this[string strName] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; }
    }

    [ComImport, Guid("835D1F61-1E95-4BC8-B4D3-976C42B968F7"), TypeLibType((short) 0x1040)]
    public interface ICertificationAuthority
    {
        [DispId(0x60020000)]
        object this[EnrollmentCAProperty Property] { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)] get; }
    }

    [ComImport, Guid("728AB32F-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface ICertProperties : IEnumerable
    {
        [DispId(0)]
        CCertProperty this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1), DispId(-4)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CCertProperty pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
    }

    [ComImport, Guid("728AB32E-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface ICertProperty
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020002)]
        CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
    }

    [ComImport, Guid("728AB337-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface ICertPropertyArchived : ICertProperty
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020002)]
        CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void Initialize([In] bool ArchivedValue);
        [DispId(0x60030001)]
        bool Archived { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)] get; }
    }

    [ComImport, Guid("728AB33B-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface ICertPropertyArchivedKeyHash : ICertProperty
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020002)]
        CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void Initialize([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strArchivedKeyHashValue);        
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB332-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ICertPropertyAutoEnroll : ICertProperty
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020002)]
        CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void Initialize([In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [DispId(0x60030001)]
        string TemplateName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)] get; }
    }

    [ComImport, Guid("728AB338-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface ICertPropertyBackedUp : ICertProperty
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020002)]
        CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeFromCurrentTime([In] bool BackedUpValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void Initialize([In] bool BackedUpValue, [In] DateTime Date);
        [DispId(0x60030002)]
        bool BackedUpValue { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60030003)]
        DateTime BackedUpTime { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB331-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ICertPropertyDescription : ICertProperty
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020002)]
        CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void Initialize([In, MarshalAs(UnmanagedType.BStr)] string strDescription);
        [DispId(0x60030001)]
        string Description { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)] get; }
    }

    [ComImport, Guid("728AB339-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface ICertPropertyEnrollment : ICertProperty
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020002)]
        CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void Initialize([In] int RequestId, [In, MarshalAs(UnmanagedType.BStr)] string strCADnsName, [In, MarshalAs(UnmanagedType.BStr)] string strCAName, [In, Optional, DefaultParameterValue("0"), MarshalAs(UnmanagedType.BStr)] string strFriendlyName);
        [DispId(0x60030001)]
        int RequestId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)] get; }
        [DispId(0x60030002)]
        string CADnsName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60030003)]
        string CAName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
        [DispId(0x60030004)]
        string FriendlyName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; }
    }

    [ComImport, Guid("728AB34A-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface ICertPropertyEnrollmentPolicyServer : ICertProperty
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020002)]
        CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void Initialize([In] EnrollmentPolicyServerPropertyFlags PropertyFlags, [In] X509EnrollmentAuthFlags AuthFlags, [In] X509EnrollmentAuthFlags EnrollmentServerAuthFlags, [In] PolicyServerUrlFlags UrlFlags, [In, MarshalAs(UnmanagedType.BStr)] string strRequestId, [In, MarshalAs(UnmanagedType.BStr)] string strUrl, [In, MarshalAs(UnmanagedType.BStr)] string strId, [In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentServerUrl);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        string GetPolicyServerUrl();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        string GetPolicyServerId();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        string GetEnrollmentServerUrl();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)]
        string GetRequestIdString();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030005)]
        EnrollmentPolicyServerPropertyFlags GetPropertyFlags();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)]
        PolicyServerUrlFlags GetUrlFlags();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030007)]
        X509EnrollmentAuthFlags GetAuthentication();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030008)]
        X509EnrollmentAuthFlags GetEnrollmentServerAuthentication();
    }

    [ComImport, Guid("728AB330-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface ICertPropertyFriendlyName : ICertProperty
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020002)]
        CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void Initialize([In, MarshalAs(UnmanagedType.BStr)] string strFriendlyName);
        [DispId(0x60030001)]
        string FriendlyName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB336-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ICertPropertyKeyProvInfo : ICertProperty
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020002)]
        CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pValue);
        [DispId(0x60030001)]
        CX509PrivateKey PrivateKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)] get; }
    }

    [ComImport, Guid("728AB33A-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface ICertPropertyRenewal : ICertProperty
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020002)]
        CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void Initialize([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strRenewalValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeFromCertificateHash([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);        
    }

    [ComImport, Guid("728AB333-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface ICertPropertyRequestOriginator : ICertProperty
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020002)]
        CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void Initialize([In, MarshalAs(UnmanagedType.BStr)] string strRequestOriginator);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeFromLocalRequestOriginator();
        [DispId(0x60030002)]
        string RequestOriginator { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB334-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ICertPropertySHA1Hash : ICertProperty
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020002)]
        CERTENROLL_PROPERTYID PropertyId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void RemoveFromCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void SetValueOnCertificate([In] bool MachineContext, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void Initialize([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strRenewalValue);        
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB32C-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ICryptAttribute
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromObjectId([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeFromValues([In, MarshalAs(UnmanagedType.Interface)] CX509Attributes pAttributes);
        [DispId(0x60020002)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        CX509Attributes Values { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB32D-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ICryptAttributes : IEnumerable
    {
        [DispId(0)]
        CCryptAttribute this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1), DispId(-4)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CCryptAttribute pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
        [DispId(0x60020006)]
        int this[CObjectId pObjectId] { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)]
        void AddRange([In, MarshalAs(UnmanagedType.Interface)] CCryptAttributes pValue);
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB305-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ICspAlgorithm
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        CObjectId GetAlgorithmOid([In] int Length, [In] AlgorithmFlags AlgFlags);
        [DispId(0x60020001)]
        int DefaultLength { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        int IncrementLength { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        string LongName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
        [DispId(0x60020004)]
        bool Valid { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        int MaxLength { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        int MinLength { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
        [DispId(0x60020007)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; }
        [DispId(0x60020008)]
        AlgorithmType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; }
        [DispId(0x60020009)]
        AlgorithmOperationFlags Operations { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB306-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ICspAlgorithms : IEnumerable
    {
        [DispId(0)]
        ICspAlgorithm this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1), DispId(-4)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] ICspAlgorithm pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
        [DispId(0x60020006)]
        ICspAlgorithm this[string strName] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
        [DispId(0x60020007)]
        int this[CObjectId pObjectId] { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; }
    }

    [ComImport, Guid("728AB307-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface ICspInformation
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromName([In, MarshalAs(UnmanagedType.BStr)] string strName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeFromType([In] X509ProviderType Type, [In, MarshalAs(UnmanagedType.Interface)] CObjectId pAlgorithm, [In] bool MachineContext);
        [DispId(0x60020002)]
        ICspAlgorithms CspAlgorithms { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool HasHardwareRandomNumberGenerator { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
        [DispId(0x60020004)]
        bool IsHardwareDevice { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        bool IsRemovable { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        bool IsSoftwareDevice { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
        [DispId(0x60020007)]
        bool Valid { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; }
        [DispId(0x60020008)]
        int MaxKeyContainerNameLength { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; }
        [DispId(0x60020009)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] get; }
        [DispId(0x6002000a)]
        X509ProviderType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; }
        [DispId(0x6002000b)]
        int Version { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000b)] get; }
        [DispId(0x6002000c)]
        X509KeySpec KeySpec { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; }
        [DispId(0x6002000d)]
        bool IsSmartCard { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000d)] get; }
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)]
        string GetDefaultSecurityDescriptor([In] bool MachineContext);
        [DispId(0x6002000f)]
        bool LegacyCsp { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000f)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)]
        CCspStatus GetCspStatusFromOperations([In, MarshalAs(UnmanagedType.Interface)] CObjectId pAlgorithm, [In] AlgorithmOperationFlags Operations);
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB308-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ICspInformations : IEnumerable
    {
        [DispId(0)]
        CCspInformation this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CCspInformation pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void AddAvailableCsps();
        [DispId(0x60020007)]
        CCspInformation this[string strName] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)]
        CCspStatus GetCspStatusFromProviderName([In, MarshalAs(UnmanagedType.BStr)] string strProviderName, [In] X509KeySpec LegacyKeySpec);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)]
        ICspStatuses GetCspStatusesFromOperations([In] AlgorithmOperationFlags Operations, [In, MarshalAs(UnmanagedType.Interface)] CCspInformation pCspInformation);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)]
        ICspAlgorithms GetEncryptionCspAlgorithms([In, MarshalAs(UnmanagedType.Interface)] CCspInformation pCspInformation);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000b)]
        CObjectIds GetHashAlgorithms([In, MarshalAs(UnmanagedType.Interface)] CCspInformation pCspInformation);
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB309-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ICspStatus
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CCspInformation pCsp, [In, MarshalAs(UnmanagedType.Interface)] ICspAlgorithm pAlgorithm);
        [DispId(0x60020001)]
        int Ordinal { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] set; }
        [DispId(0x60020003)]
        ICspAlgorithm CspAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
        [DispId(0x60020004)]
        CCspInformation CspInformation { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        IX509EnrollmentStatus EnrollmentStatus { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        string DisplayName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB30A-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ICspStatuses : IEnumerable
    {
        [DispId(0)]
        CCspStatus this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1), DispId(-4)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CCspStatus pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
        [DispId(0x60020006)]
        CCspStatus this[string strCspName, string strAlgorithmName] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }        
        [DispId(0x60020008)]
        CCspStatus this[string strCspName, string strAlgorithmName, AlgorithmOperationFlags Operations] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; }
        [DispId(0x60020009)]
        CCspStatus this[CCspStatus pCspStatus] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] get; }
    }

    public enum InnerRequestLevel
    {
        LevelInnermost,
        LevelNext
    }

    public enum InstallResponseRestrictionFlags
    {
        AllowNone = 0,
        AllowNoOutstandingRequest = 1,
        AllowUntrustedCertificate = 2,
        AllowUntrustedRoot = 4
    }

    [ComImport, Guid("728AB300-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IObjectId
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeFromName([In] CERTENROLL_OBJECTID Name);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeFromValue([In, MarshalAs(UnmanagedType.BStr)] string strValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void InitializeFromAlgorithmName([In] ObjectIdGroupId GroupId, [In] ObjectIdPublicKeyFlags KeyFlags, [In] AlgorithmFlags AlgFlags, [In, MarshalAs(UnmanagedType.BStr)] string strAlgorithmName);
        [DispId(0x60020003)]
        CERTENROLL_OBJECTID Name { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
        [DispId(0x60020004)]
        string FriendlyName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] set; }
        [DispId(0x60020006)]
        string Value { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)]
        string GetAlgorithmName([In] ObjectIdGroupId GroupId, [In] ObjectIdPublicKeyFlags KeyFlags);
    }

    [ComImport, Guid("728AB301-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IObjectIds : IEnumerable
    {
        [DispId(0)]
        CObjectId this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CObjectId pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void AddRange([In, MarshalAs(UnmanagedType.Interface)] CObjectIds pValue);
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB31C-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IPolicyQualifier
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void InitializeEncode([In, MarshalAs(UnmanagedType.BStr)] string strQualifier, [In] PolicyQualifierType Type);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string Qualifier { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        PolicyQualifierType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB31D-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IPolicyQualifiers : IEnumerable
    {
        [DispId(0)]
        CPolicyQualifier this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1), DispId(-4)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CPolicyQualifier pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB33D-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ISignerCertificate
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In] bool MachineContext, [In] X509PrivateKeyVerify VerifyType, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCertifcate);
        [DispId(0x60020001)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        CX509PrivateKey PrivateKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60020005)]
        int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] set; }
        [DispId(0x60020007)]
        string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] set; }
        [DispId(0x60020009)]
        string Pin { [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] set; }
        [DispId(0x6002000a)]
        IX509SignatureInformation SignatureInformation { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB33E-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ISignerCertificates : IEnumerable
    {
        [DispId(0)]
        CSignerCertificate this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CSignerCertificate pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        int Find([In, MarshalAs(UnmanagedType.Interface)] CSignerCertificate pSignerCert);
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB31A-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ISmimeCapabilities : IEnumerable
    {
        [DispId(0)]
        CSmimeCapability this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CSmimeCapability pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void AddFromCsp([In, MarshalAs(UnmanagedType.Interface)] CCspInformation pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)]
        void AddAvailableSmimeCapabilities([In] bool MachineContext);
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB319-217D-11DA-B2A4-000E7BBB2B09")]
    public interface ISmimeCapability
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] int BitCount);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        int BitCount { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, Guid("728AB303-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX500DistinguishedName
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Decode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedName, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X500NameFlags.XCN_CERT_NAME_STR_NONE)] X500NameFlags NameFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void Encode([In, MarshalAs(UnmanagedType.BStr)] string strName, [In, Optional, DefaultParameterValue(X500NameFlags.XCN_CERT_NAME_STR_NONE)] X500NameFlags NameFlags);
        [DispId(0x60020002)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
    }

    [ComImport, Guid("728AB322-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509Attribute
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, Guid("728AB327-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509AttributeArchiveKey : IX509Attribute
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pKey, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCAXCert, [In, MarshalAs(UnmanagedType.Interface)] CObjectId pAlgorithm, [In] int EncryptionStrength);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);        
        [DispId(0x60030003)]
        CObjectId EncryptionAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
        [DispId(0x60030004)]
        int EncryptionStrength { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB328-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509AttributeArchiveKeyHash : IX509Attribute
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncodeFromEncryptedKeyBlob([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncryptedKeyBlob);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);        
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB325-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509AttributeClientId : IX509Attribute
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In] RequestClientInfoClientId ClientId, [In, MarshalAs(UnmanagedType.BStr)] string strMachineDnsName, [In, MarshalAs(UnmanagedType.BStr)] string strUserSamName, [In, MarshalAs(UnmanagedType.BStr)] string strProcessName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60030002)]
        RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60030003)]
        string MachineDnsName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
        [DispId(0x60030004)]
        string UserSamName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; }
        [DispId(0x60030005)]
        string ProcessName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030005)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB32B-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509AttributeCspProvider : IX509Attribute
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In] X509KeySpec KeySpec, [In, MarshalAs(UnmanagedType.BStr)] string strProviderName, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strSignature);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60030002)]
        X509KeySpec KeySpec { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60030003)]
        string ProviderName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }        
    }

    [ComImport, Guid("728AB324-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509AttributeExtensions : IX509Attribute
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CX509Extensions pExtensions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60030002)]
        CX509Extensions X509Extensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB32A-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509AttributeOSVersion : IX509Attribute
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In, MarshalAs(UnmanagedType.BStr)] string strOSVersion);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60030002)]
        string OSVersion { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, Guid("728AB326-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509AttributeRenewalCertificate : IX509Attribute
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strCert);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);        
    }

    [ComImport, Guid("728AB323-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509Attributes : IEnumerable
    {
        [DispId(0)]
        CX509Attribute this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CX509Attribute pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB341-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509CertificateRequest
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void Encode();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void ResetForEncode();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        IX509CertificateRequest GetInnerRequest([In] InnerRequestLevel Level);
        [DispId(0x60020004)]
        X509RequestType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x60020008)]
        int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000a)]
        string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x6002000c)]
        bool SuppressDefaults { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }
        [DispId(0x6002000e)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] set; }
        [DispId(0x60020010)]
        RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60020012)]
        CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x60020014)]
        CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x60020016)]
        bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }        
    }

    [ComImport, Guid("728AB343-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509CertificateRequestCertificate : IX509CertificateRequestPkcs10
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void Encode();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void ResetForEncode();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        IX509CertificateRequest GetInnerRequest([In] InnerRequestLevel Level);
        [DispId(0x60020004)]
        X509RequestType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x60020008)]
        int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000a)]
        string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x6002000c)]
        bool SuppressDefaults { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }

        [DispId(0x6002000e)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] set; }
        
        [DispId(0x60020010)]
        RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60020012)]
        CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x60020014)]
        CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x60020016)]
        bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }        
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeFromPrivateKey([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pPrivateKey, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        void InitializeFromPublicKey([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PublicKey pPublicKey, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        void InitializeFromCertificate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X509RequestInheritOptions.InheritDefault)] X509RequestInheritOptions InheritOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)]
        void InitializeDecode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedData, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030005)]
        void CheckSignature([In, Optional, DefaultParameterValue(Pkcs10AllowedSignatureTypes.AllowedKeySignature)] Pkcs10AllowedSignatureTypes AllowedSignatureTypes);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)]
        bool IsSmartCard();
        [DispId(0x60030007)]
        CObjectId TemplateObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030007)] get; }
        [DispId(0x60030008)]
        CX509PublicKey PublicKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030008)] get; }
        [DispId(0x60030009)]
        CX509PrivateKey PrivateKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030009)] get; }
        [DispId(0x6003000a)]
        bool NullSigned { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000a)] get; }
        [DispId(0x6003000b)]
        bool ReuseKey { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000b)] get; }       
        [DispId(0x6003000d)]
        CX500DistinguishedName Subject { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000d)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000d)] set; }
        [DispId(0x6003000f)]
        ICspStatuses CspStatuses { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000f)] get; }
        [DispId(0x60030010)]
        bool SmimeCapabilities { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030010)] set; }
        [DispId(0x60030012)]
        IX509SignatureInformation SignatureInformation { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030012)] get; }
        [DispId(0x60030013)]
        string KeyContainerNamePrefix { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030013)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030013)] set; }
        [DispId(0x60030015)]
        CCryptAttributes CryptAttributes { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030015)] get; }
        [DispId(0x60030016)]
        CX509Extensions X509Extensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030016)] get; }
        [DispId(0x60030017)]
        CObjectIds CriticalExtensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030017)] get; }
        [DispId(0x60030018)]
        CObjectIds SuppressOids { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030018)] get; }
     
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003001b)]
        ICspStatuses GetCspStatuses([In] X509KeySpec KeySpec);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040000)]
        void CheckPublicKeySignature([In, MarshalAs(UnmanagedType.Interface)] CX509PublicKey pPublicKey);
        [DispId(0x60040001)]
        CX500DistinguishedName Issuer { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040001)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040001)] set; }
        [DispId(0x60040003)]
        DateTime NotBefore { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040003)] set; }
        [DispId(0x60040005)]
        DateTime NotAfter { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040005)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040005)] set; }
    
        [DispId(0x60040009)]
        CSignerCertificate SignerCertificate { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040009)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040009)] set; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB35A-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509CertificateRequestCertificate2 : IX509CertificateRequestCertificate
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void Encode();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void ResetForEncode();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        IX509CertificateRequest GetInnerRequest([In] InnerRequestLevel Level);
        [DispId(0x60020004)]
        X509RequestType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x60020008)]
        int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000a)]
        string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x6002000c)]
        bool SuppressDefaults { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }

        [DispId(0x6002000e)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] set; }
        [DispId(0x60020010)]
        RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60020012)]
        CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x60020014)]
        CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x60020016)]
        bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }
        
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeFromPrivateKey([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pPrivateKey, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        void InitializeFromPublicKey([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PublicKey pPublicKey, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        void InitializeFromCertificate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X509RequestInheritOptions.InheritDefault)] X509RequestInheritOptions InheritOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)]
        void InitializeDecode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedData, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030005)]
        void CheckSignature([In, Optional, DefaultParameterValue(Pkcs10AllowedSignatureTypes.AllowedKeySignature)] Pkcs10AllowedSignatureTypes AllowedSignatureTypes);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)]
        bool IsSmartCard();
        [DispId(0x60030007)]
        CObjectId TemplateObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030007)] get; }
        [DispId(0x60030008)]
        CX509PublicKey PublicKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030008)] get; }
        [DispId(0x60030009)]
        CX509PrivateKey PrivateKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030009)] get; }
        [DispId(0x6003000a)]
        bool NullSigned { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000a)] get; }
        [DispId(0x6003000b)]
        bool ReuseKey { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000b)] get; }
       
        [DispId(0x6003000d)]
        CX500DistinguishedName Subject { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000d)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000d)] set; }
        [DispId(0x6003000f)]
        ICspStatuses CspStatuses { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000f)] get; }
        [DispId(0x60030010)]
        bool SmimeCapabilities { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030010)] set; }
        [DispId(0x60030012)]
        IX509SignatureInformation SignatureInformation { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030012)] get; }
        [DispId(0x60030013)]
        string KeyContainerNamePrefix { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030013)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030013)] set; }
        [DispId(0x60030015)]
        CCryptAttributes CryptAttributes { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030015)] get; }
        [DispId(0x60030016)]
        CX509Extensions X509Extensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030016)] get; }
        [DispId(0x60030017)]
        CObjectIds CriticalExtensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030017)] get; }
        [DispId(0x60030018)]
        CObjectIds SuppressOids { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030018)] get; }
      
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003001b)]
        ICspStatuses GetCspStatuses([In] X509KeySpec KeySpec);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040000)]
        void CheckPublicKeySignature([In, MarshalAs(UnmanagedType.Interface)] CX509PublicKey pPublicKey);
        [DispId(0x60040001)]
        CX500DistinguishedName Issuer { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040001)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040001)] set; }
        [DispId(0x60040003)]
        DateTime NotBefore { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040003)] set; }
        [DispId(0x60040005)]
        DateTime NotAfter { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040005)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040005)] set; }
      
        [DispId(0x60040009)]
        CSignerCertificate SignerCertificate { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040009)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040009)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050000)]
        void InitializeFromTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050001)]
        void InitializeFromPrivateKeyTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pPrivateKey, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [DispId(0x60050002)]
        IX509EnrollmentPolicyServer PolicyServer { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050002)] get; }
        [DispId(0x60050003)]
        IX509CertificateTemplate Template { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050003)] get; }
    }

    [ComImport, Guid("728AB345-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509CertificateRequestCmc : IX509CertificateRequestPkcs7
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void Encode();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void ResetForEncode();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        IX509CertificateRequest GetInnerRequest([In] InnerRequestLevel Level);
        [DispId(0x60020004)]
        X509RequestType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x60020008)]
        int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000a)]
        string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x6002000c)]
        bool SuppressDefaults { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }
        
        [DispId(0x6002000e)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] set; }

        [DispId(0x60020010)]
        RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60020012)]
        CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x60020014)]
        CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x60020016)]
        bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }
        
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeFromCertificate([In] X509CertificateEnrollmentContext Context, [In] bool RenewalRequest, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X509RequestInheritOptions.InheritDefault)] X509RequestInheritOptions InheritOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        void InitializeFromInnerRequest([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pInnerRequest);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        void InitializeDecode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedData, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [DispId(0x60030004)]
        string RequesterName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] set; }
        [DispId(0x60030006)]
        CSignerCertificate SignerCertificate { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040000)]
        void InitializeFromInnerRequestTemplateName([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pInnerRequest, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [DispId(0x60040001)]
        CObjectId TemplateObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040001)] get; }
        [DispId(0x60040002)]
        bool NullSigned { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040002)] get; }
        [DispId(0x60040003)]
        CCryptAttributes CryptAttributes { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040003)] get; }
        [DispId(0x60040004)]
        IX509NameValuePairs NameValuePairs { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040004)] get; }
        [DispId(0x60040005)]
        CX509Extensions X509Extensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040005)] get; }
        [DispId(0x60040006)]
        CObjectIds CriticalExtensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040006)] get; }
        [DispId(0x60040007)]
        CObjectIds SuppressOids { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040007)] get; }
        [DispId(0x60040008)]
        int TransactionId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040008)] set; }
      
        [DispId(0x6004000c)]
        IX509SignatureInformation SignatureInformation { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6004000c)] get; }
        [DispId(0x6004000d)]
        bool ArchivePrivateKey { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6004000d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6004000d)] set; }
       
        [DispId(0x60040011)]
        CObjectId EncryptionAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040011)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040011)] set; }
        [DispId(0x60040013)]
        int EncryptionStrength { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040013)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040013)] set; }
        
        [DispId(0x60040016)]
        ISignerCertificates SignerCertificates { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040016)] get; }
    }

    [ComImport, Guid("728AB35D-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509CertificateRequestCmc2 : IX509CertificateRequestCmc
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void Encode();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void ResetForEncode();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        IX509CertificateRequest GetInnerRequest([In] InnerRequestLevel Level);
        [DispId(0x60020004)]
        X509RequestType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x60020008)]
        int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000a)]
        string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x6002000c)]
        bool SuppressDefaults { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }

        [DispId(0x6002000e)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] set; }
        [DispId(0x60020010)]
        RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60020012)]
        CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x60020014)]
        CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x60020016)]
        bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }
        

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeFromCertificate([In] X509CertificateEnrollmentContext Context, [In] bool RenewalRequest, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X509RequestInheritOptions.InheritDefault)] X509RequestInheritOptions InheritOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        void InitializeFromInnerRequest([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pInnerRequest);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        void InitializeDecode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedData, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [DispId(0x60030004)]
        string RequesterName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] set; }
        [DispId(0x60030006)]
        CSignerCertificate SignerCertificate { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040000)]
        void InitializeFromInnerRequestTemplateName([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pInnerRequest, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [DispId(0x60040001)]
        CObjectId TemplateObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040001)] get; }
        [DispId(0x60040002)]
        bool NullSigned { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040002)] get; }
        [DispId(0x60040003)]
        CCryptAttributes CryptAttributes { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040003)] get; }
        [DispId(0x60040004)]
        IX509NameValuePairs NameValuePairs { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040004)] get; }
        [DispId(0x60040005)]
        CX509Extensions X509Extensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040005)] get; }
        [DispId(0x60040006)]
        CObjectIds CriticalExtensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040006)] get; }
        [DispId(0x60040007)]
        CObjectIds SuppressOids { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040007)] get; }
        [DispId(0x60040008)]
        int TransactionId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040008)] set; }
        
        [DispId(0x6004000c)]
        IX509SignatureInformation SignatureInformation { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6004000c)] get; }
        [DispId(0x6004000d)]
        bool ArchivePrivateKey { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6004000d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6004000d)] set; }
        
        [DispId(0x60040011)]
        CObjectId EncryptionAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040011)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040011)] set; }
        [DispId(0x60040013)]
        int EncryptionStrength { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040013)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040013)] set; }
        
        [DispId(0x60040016)]
        ISignerCertificates SignerCertificates { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040016)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050000)]
        void InitializeFromTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050001)]
        void InitializeFromInnerRequestTemplate([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pInnerRequest, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [DispId(0x60050002)]
        IX509EnrollmentPolicyServer PolicyServer { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050002)] get; }
        [DispId(0x60050003)]
        IX509CertificateTemplate Template { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050003)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050004)]
        void CheckSignature([In, Optional, DefaultParameterValue(Pkcs10AllowedSignatureTypes.AllowedKeySignature)] Pkcs10AllowedSignatureTypes AllowedSignatureTypes);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60050005)]
        void CheckCertificateSignature([In, MarshalAs(UnmanagedType.Interface)] CSignerCertificate pSignerCertificate, [In] bool ValidateCertificateChain);
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB342-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509CertificateRequestPkcs10 : IX509CertificateRequest
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void Encode();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void ResetForEncode();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        IX509CertificateRequest GetInnerRequest([In] InnerRequestLevel Level);
        [DispId(0x60020004)]
        X509RequestType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x60020008)]
        int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000a)]
        string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x6002000c)]
        bool SuppressDefaults { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }
        
        [DispId(0x6002000e)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] set; }
        [DispId(0x60020010)]
        RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60020012)]
        CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x60020014)]
        CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x60020016)]
        bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }
        
       
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeFromPrivateKey([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pPrivateKey, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        void InitializeFromPublicKey([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PublicKey pPublicKey, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        void InitializeFromCertificate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X509RequestInheritOptions.InheritDefault)] X509RequestInheritOptions InheritOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)]
        void InitializeDecode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedData, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030005)]
        void CheckSignature([In, Optional, DefaultParameterValue(Pkcs10AllowedSignatureTypes.AllowedKeySignature)] Pkcs10AllowedSignatureTypes AllowedSignatureTypes);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)]
        bool IsSmartCard();
        [DispId(0x60030007)]
        CObjectId TemplateObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030007)] get; }
        [DispId(0x60030008)]
        CX509PublicKey PublicKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030008)] get; }
        [DispId(0x60030009)]
        CX509PrivateKey PrivateKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030009)] get; }
        [DispId(0x6003000a)]
        bool NullSigned { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000a)] get; }
        [DispId(0x6003000b)]
        bool ReuseKey { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000b)] get; }
    
        [DispId(0x6003000d)]
        CX500DistinguishedName Subject { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000d)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000d)] set; }
        [DispId(0x6003000f)]
        ICspStatuses CspStatuses { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000f)] get; }
        [DispId(0x60030010)]
        bool SmimeCapabilities { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030010)] set; }
        [DispId(0x60030012)]
        IX509SignatureInformation SignatureInformation { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030012)] get; }
        [DispId(0x60030013)]
        string KeyContainerNamePrefix { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030013)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030013)] set; }
        [DispId(0x60030015)]
        CCryptAttributes CryptAttributes { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030015)] get; }
        [DispId(0x60030016)]
        CX509Extensions X509Extensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030016)] get; }
        [DispId(0x60030017)]
        CObjectIds CriticalExtensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030017)] get; }
        [DispId(0x60030018)]
        CObjectIds SuppressOids { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030018)] get; }
        
        
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003001b)]
        ICspStatuses GetCspStatuses([In] X509KeySpec KeySpec);
    }

    [ComImport, Guid("728AB35B-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509CertificateRequestPkcs10V2 : IX509CertificateRequestPkcs10
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void Encode();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void ResetForEncode();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        IX509CertificateRequest GetInnerRequest([In] InnerRequestLevel Level);
        [DispId(0x60020004)]
        X509RequestType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x60020008)]
        int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000a)]
        string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x6002000c)]
        bool SuppressDefaults { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }
        
        [DispId(0x6002000e)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] set; }
        [DispId(0x60020010)]
        RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60020012)]
        CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x60020014)]
        CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x60020016)]
        bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }

        
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeFromPrivateKey([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pPrivateKey, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        void InitializeFromPublicKey([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PublicKey pPublicKey, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        void InitializeFromCertificate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X509RequestInheritOptions.InheritDefault)] X509RequestInheritOptions InheritOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)]
        void InitializeDecode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedData, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030005)]
        void CheckSignature([In, Optional, DefaultParameterValue(Pkcs10AllowedSignatureTypes.AllowedKeySignature)] Pkcs10AllowedSignatureTypes AllowedSignatureTypes);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)]
        bool IsSmartCard();
        [DispId(0x60030007)]
        CObjectId TemplateObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030007)] get; }
        [DispId(0x60030008)]
        CX509PublicKey PublicKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030008)] get; }
        [DispId(0x60030009)]
        CX509PrivateKey PrivateKey { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030009)] get; }
        [DispId(0x6003000a)]
        bool NullSigned { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000a)] get; }
        [DispId(0x6003000b)]
        bool ReuseKey { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000b)] get; }
     
        [DispId(0x6003000d)]
        CX500DistinguishedName Subject { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000d)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000d)] set; }
        [DispId(0x6003000f)]
        ICspStatuses CspStatuses { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003000f)] get; }
        [DispId(0x60030010)]
        bool SmimeCapabilities { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030010)] set; }
        [DispId(0x60030012)]
        IX509SignatureInformation SignatureInformation { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030012)] get; }
        [DispId(0x60030013)]
        string KeyContainerNamePrefix { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030013)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030013)] set; }
        [DispId(0x60030015)]
        CCryptAttributes CryptAttributes { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030015)] get; }
        [DispId(0x60030016)]
        CX509Extensions X509Extensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030016)] get; }
        [DispId(0x60030017)]
        CObjectIds CriticalExtensions { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030017)] get; }
        [DispId(0x60030018)]
        CObjectIds SuppressOids { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030018)] get; }        
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6003001b)]
        ICspStatuses GetCspStatuses([In] X509KeySpec KeySpec);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040000)]
        void InitializeFromTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040001)]
        void InitializeFromPrivateKeyTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PrivateKey pPrivateKey, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040002)]
        void InitializeFromPublicKeyTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] CX509PublicKey pPublicKey, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [DispId(0x60040003)]
        IX509EnrollmentPolicyServer PolicyServer { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040003)] get; }
        [DispId(0x60040004)]
        IX509CertificateTemplate Template { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040004)] get; }
    }

    [ComImport, Guid("728AB344-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509CertificateRequestPkcs7 : IX509CertificateRequest
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void Encode();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void ResetForEncode();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        IX509CertificateRequest GetInnerRequest([In] InnerRequestLevel Level);
        [DispId(0x60020004)]
        X509RequestType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x60020008)]
        int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000a)]
        string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x6002000c)]
        bool SuppressDefaults { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }
        
        [DispId(0x6002000e)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] set; }
        [DispId(0x60020010)]
        RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60020012)]
        CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x60020014)]
        CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x60020016)]
        bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }
        
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeFromCertificate([In] X509CertificateEnrollmentContext Context, [In] bool RenewalRequest, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X509RequestInheritOptions.InheritDefault)] X509RequestInheritOptions InheritOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        void InitializeFromInnerRequest([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pInnerRequest);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        void InitializeDecode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedData, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [DispId(0x60030004)]
        string RequesterName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] set; }
        [DispId(0x60030006)]
        CSignerCertificate SignerCertificate { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)] set; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB35C-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509CertificateRequestPkcs7V2 : IX509CertificateRequestPkcs7
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void Encode();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void ResetForEncode();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        IX509CertificateRequest GetInnerRequest([In] InnerRequestLevel Level);
        [DispId(0x60020004)]
        X509RequestType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        [DispId(0x60020005)]
        X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; }
        [DispId(0x60020006)]
        bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x60020008)]
        int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000a)]
        string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x6002000c)]
        bool SuppressDefaults { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }
        
        [DispId(0x6002000e)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] set; }
        [DispId(0x60020010)]
        RequestClientInfoClientId ClientId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60020012)]
        CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x60020014)]
        CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x60020016)]
        bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }
                
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeFromCertificate([In] X509CertificateEnrollmentContext Context, [In] bool RenewalRequest, [In, MarshalAs(UnmanagedType.BStr)] string strCertificate, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding, [In, Optional, DefaultParameterValue(X509RequestInheritOptions.InheritDefault)] X509RequestInheritOptions InheritOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)]
        void InitializeFromInnerRequest([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pInnerRequest);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)]
        void InitializeDecode([In, MarshalAs(UnmanagedType.BStr)] string strEncodedData, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [DispId(0x60030004)]
        string RequesterName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] set; }
        [DispId(0x60030006)]
        CSignerCertificate SignerCertificate { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030006)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040000)]
        void InitializeFromTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [DispId(0x60040001)]
        IX509EnrollmentPolicyServer PolicyServer { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040001)] get; }
        [DispId(0x60040002)]
        IX509CertificateTemplate Template { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040002)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60040003)]
        void CheckCertificateSignature([In] bool ValidateCertificateChain);
    }

    [ComImport, Guid("54244A13-555A-4E22-896D-1B0E52F76406"), TypeLibType((short) 0x1040)]
    public interface IX509CertificateTemplate
    {
        [DispId(0x60020000)]
        object this[EnrollmentTemplateProperty Property] { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)] get; }
    }

    [ComImport, Guid("13B79003-2181-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509CertificateTemplates : IEnumerable
    {
        [DispId(0)]
        IX509CertificateTemplate this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
        [DispId(0x60020006)]
        IX509CertificateTemplate this[string bstrName] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
        [DispId(0x60020007)]
        IX509CertificateTemplate this[CObjectId pOid] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; }
    }

    [ComImport, Guid("F49466A7-395A-4E9E-B6E7-32B331600DC0"), TypeLibType((short) 0x1040)]
    public interface IX509CertificateTemplateWritable
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void Commit([In] CommitTemplateFlags commitFlags, [In, MarshalAs(UnmanagedType.BStr)] string strServerContext);
        [DispId(0x60020002)]
        object this[EnrollmentTemplateProperty Property] { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        IX509CertificateTemplate Template { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
    }

    [ComImport, Guid("728AB346-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509Enrollment
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void InitializeFromRequest([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pRequest);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        string CreateRequest([In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)]
        void Enroll();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void InstallResponse([In] InstallResponseRestrictionFlags Restrictions, [In, MarshalAs(UnmanagedType.BStr)] string strResponse, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strPassword);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        string CreatePFX([In, MarshalAs(UnmanagedType.BStr)] string strPassword, [In] PFXExportOptions ExportOptions, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [DispId(0x60020007)]
        IX509CertificateRequest Request { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; }
        [DispId(0x60020008)]
        bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000a)]
        int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x6002000c)]
        IX509NameValuePairs NameValuePairs { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; }
        [DispId(0x6002000d)]
        X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000d)] get; }
        [DispId(0x6002000e)]
        IX509EnrollmentStatus Status { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; }
        [DispId(0x6002000f)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000f)] get; }
        
        [DispId(0x60020011)]
        string CertificateFriendlyName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020011)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020011)] set; }
        [DispId(0x60020013)]
        string CertificateDescription { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020013)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020013)] set; }
        [DispId(0x60020015)]
        int RequestId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020015)] get; }
        [DispId(0x60020016)]
        string CAConfigString { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB350-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509Enrollment2 : IX509Enrollment
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeFromTemplateName([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void InitializeFromRequest([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateRequest pRequest);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        string CreateRequest([In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)]
        void Enroll();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void InstallResponse([In] InstallResponseRestrictionFlags Restrictions, [In, MarshalAs(UnmanagedType.BStr)] string strResponse, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strPassword);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        string CreatePFX([In, MarshalAs(UnmanagedType.BStr)] string strPassword, [In] PFXExportOptions ExportOptions, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [DispId(0x60020007)]
        IX509CertificateRequest Request { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; }
        [DispId(0x60020008)]
        bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000a)]
        int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x6002000c)]
        IX509NameValuePairs NameValuePairs { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; }
        [DispId(0x6002000d)]
        X509CertificateEnrollmentContext EnrollmentContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000d)] get; }
        [DispId(0x6002000e)]
        IX509EnrollmentStatus Status { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; }
        [DispId(0x6002000f)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000f)] get; }
        
        [DispId(0x60020011)]
        string CertificateFriendlyName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020011)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020011)] set; }
        [DispId(0x60020013)]
        string CertificateDescription { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020013)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020013)] set; }
        [DispId(0x60020015)]
        int RequestId { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020015)] get; }
        [DispId(0x60020016)]
        string CAConfigString { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeFromTemplate([In] X509CertificateEnrollmentContext Context, [In, MarshalAs(UnmanagedType.Interface)] IX509EnrollmentPolicyServer pPolicyServer, [In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InstallResponse2([In] InstallResponseRestrictionFlags Restrictions, [In, MarshalAs(UnmanagedType.BStr)] string strResponse, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strPassword, [In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentPolicyServerUrl, [In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentPolicyServerID, [In] PolicyServerUrlFlags EnrollmentPolicyServerFlags, [In] X509EnrollmentAuthFlags AuthFlags);
        [DispId(0x60030002)]
        IX509EnrollmentPolicyServer PolicyServer { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60030003)]
        IX509CertificateTemplate Template { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
        [DispId(0x60030004)]
        string RequestIdString { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; }
    }

    [ComImport, Guid("728AB351-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509EnrollmentHelper
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void AddPolicyServer([In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentPolicyServerURI, [In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentPolicyID, [In] PolicyServerUrlFlags EnrollmentPolicyServerFlags, [In] X509EnrollmentAuthFlags AuthFlags, [In, MarshalAs(UnmanagedType.BStr)] string strCredential, [In, MarshalAs(UnmanagedType.BStr)] string strPassword);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void AddEnrollmentServer([In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentServerURI, [In] X509EnrollmentAuthFlags AuthFlags, [In, MarshalAs(UnmanagedType.BStr)] string strCredential, [In, MarshalAs(UnmanagedType.BStr)] string strPassword);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        string Enroll([In, MarshalAs(UnmanagedType.BStr)] string strEnrollmentPolicyServerURI, [In, MarshalAs(UnmanagedType.BStr)] string strTemplateName, [In] EncodingType Encoding, [In] WebEnrollmentFlags enrollFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        void Initialize([In] X509CertificateEnrollmentContext Context);
    }

    [ComImport, Guid("13B79026-2181-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509EnrollmentPolicyServer
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.BStr)] string bstrPolicyServerUrl, [In, MarshalAs(UnmanagedType.BStr)] string bstrPolicyServerId, [In] X509EnrollmentAuthFlags AuthFlags, [In] bool fIsUnTrusted, [In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void LoadPolicy([In, ComAliasName("CERTENROLLLib.X509EnrollmentPolicyLoadOption")] X509EnrollmentPolicyLoadOption option);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        IX509CertificateTemplates GetTemplates();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        ICertificationAuthorities GetCAsForTemplate([In, MarshalAs(UnmanagedType.Interface)] IX509CertificateTemplate pTemplate);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)]
        ICertificationAuthorities GetCAs();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void Validate();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        CObjectIds GetCustomOids();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)]
        DateTime GetNextUpdateTime();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)]
        DateTime GetLastUpdateTime();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)]
        string GetPolicyServerUrl();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)]
        string GetPolicyServerId();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000b)]
        string GetFriendlyName();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)]
        bool GetIsDefaultCEP();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000d)]
        bool GetUseClientId();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)]
        bool GetAllowUnTrustedCA();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000f)]
        string GetCachePath();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)]
        string GetCacheDir();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020011)]
        X509EnrollmentAuthFlags GetAuthFlags();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)]
        void SetCredential([In] int hWndParent, [In] X509EnrollmentAuthFlags flag, [In, MarshalAs(UnmanagedType.BStr)] string strCredential, [In, MarshalAs(UnmanagedType.BStr)] string strPassword);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020013)]
        bool QueryChanges();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)]
        void InitializeImport([In, MarshalAs(UnmanagedType.Struct)] object val);
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020015)]
        object Export([In] X509EnrollmentPolicyExportFlags exportFlags);
        [DispId(0x60020016)]
        uint Cost { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }
    }

    [ComImport, Guid("728AB304-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509EnrollmentStatus
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void AppendText([In, MarshalAs(UnmanagedType.BStr)] string strText);
        [DispId(0x60020001)]
        string Text { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] set; }
        [DispId(0x60020003)]
        EnrollmentSelectionStatus Selected { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60020005)]
        EnrollmentDisplayStatus Display { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] set; }
        [DispId(0x60020007)]
        EnrollmentEnrollStatus Status { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] set; }
        [DispId(0x60020009)]
        int Error { [return: MarshalAs(UnmanagedType.Error)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] get; [param: In, MarshalAs(UnmanagedType.Error)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] set; }
        [DispId(0x6002000b)]
        string ErrorText { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000b)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB349-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509EnrollmentWebClassFactory
    {
        [return: MarshalAs(UnmanagedType.IUnknown)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        object CreateObject([In, MarshalAs(UnmanagedType.BStr)] string strProgID);
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB30D-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509Extension
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB315-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509ExtensionAlternativeNames : IX509Extension
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CAlternativeNames pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60030002)]
        CAlternativeNames AlternativeNames { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB318-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509ExtensionAuthorityKeyIdentifier : IX509Extension
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strKeyIdentifier);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);        
    }

    [ComImport, Guid("728AB316-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509ExtensionBasicConstraints : IX509Extension
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In] bool IsCA, [In] int PathLenConstraint);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60030002)]
        bool IsCA { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60030003)]
        int PathLenConstraint { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
    }

    [ComImport, Guid("728AB320-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509ExtensionCertificatePolicies : IX509Extension
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CCertificatePolicies pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60030002)]
        CCertificatePolicies Policies { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, Guid("728AB310-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509ExtensionEnhancedKeyUsage : IX509Extension
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CObjectIds pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60030002)]
        CObjectIds EnhancedKeyUsage { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, Guid("728AB30F-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509ExtensionKeyUsage : IX509Extension
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In] X509KeyUsageFlags UsageFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60030002)]
        X509KeyUsageFlags KeyUsage { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, Guid("728AB321-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509ExtensionMSApplicationPolicies : IX509Extension
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CCertificatePolicies pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60030002)]
        CCertificatePolicies Policies { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB30E-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509Extensions : IEnumerable
    {
        [DispId(0)]
        CX509Extension this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 1), DispId(-4)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CX509Extension pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
        [DispId(0x60020006)]
        int this[CObjectId pObjectId] { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)]
        void AddRange([In, MarshalAs(UnmanagedType.Interface)] CX509Extensions pValue);
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB31B-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509ExtensionSmimeCapabilities : IX509Extension
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CSmimeCapabilities pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60030002)]
        CSmimeCapabilities SmimeCapabilities { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, Guid("728AB317-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509ExtensionSubjectKeyIdentifier : IX509Extension
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strKeyIdentifier);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB312-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509ExtensionTemplate : IX509Extension
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In, MarshalAs(UnmanagedType.Interface)] CObjectId pTemplateOid, [In] int MajorVersion, [In] int MinorVersion);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60030002)]
        CObjectId TemplateOid { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
        [DispId(0x60030003)]
        int MajorVersion { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030003)] get; }
        [DispId(0x60030004)]
        int MinorVersion { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030004)] get; }
    }

    [ComImport, Guid("728AB311-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509ExtensionTemplateName : IX509Extension
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60020001)]
        CObjectId ObjectId { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        bool Critical { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030000)]
        void InitializeEncode([In, MarshalAs(UnmanagedType.BStr)] string strTemplateName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030001)]
        void InitializeDecode([In] EncodingType Encoding, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedData);
        [DispId(0x60030002)]
        string TemplateName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60030002)] get; }
    }

    [ComImport, Guid("728AB352-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509MachineEnrollmentFactory
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        CX509EnrollmentHelper CreateObject([In, MarshalAs(UnmanagedType.BStr)] string strProgID);
    }

    [ComImport, Guid("728AB33F-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509NameValuePair
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.BStr)] string strName, [In, MarshalAs(UnmanagedType.BStr)] string strValue);
        [DispId(0x60020001)]
        string Value { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; }
        [DispId(0x60020002)]
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
    }

    [ComImport, Guid("728AB340-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509NameValuePairs : IEnumerable
    {
        [DispId(0)]
        CX509NameValuePair this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CX509NameValuePair pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
    }

    [ComImport, Guid("884E204B-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509PolicyServerListManager : IEnumerable
    {
        [DispId(0)]
        CX509PolicyServerUrl this[int Index] { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0)] get; }
        [DispId(1)]
        int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(1)] get; }
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-4), TypeLibFunc((short) 1)]
        IEnumerator GetEnumerator();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(2)]
        void Add([In, MarshalAs(UnmanagedType.Interface)] CX509PolicyServerUrl pVal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(3)]
        void Remove([In] int Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(4)]
        void Clear();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        void Initialize([In] X509CertificateEnrollmentContext Context, [In] PolicyServerUrlFlags Flags);
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("884E204A-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509PolicyServerUrl
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In] X509CertificateEnrollmentContext Context);
        [DispId(0x60020001)]
        string Url { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)] set; }
        [DispId(0x60020003)]
        bool Default { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] set; }
        [DispId(0x60020005)]
        PolicyServerUrlFlags Flags { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)] set; }
        [DispId(0x60020007)]
        X509EnrollmentAuthFlags AuthFlags { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)] set; }
        [DispId(0x60020009)]
        uint Cost { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] set; }
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000b)]
        string GetStringProperty([In] PolicyServerUrlPropertyID PropertyId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)]
        void SetStringProperty([In] PolicyServerUrlPropertyID PropertyId, [In, MarshalAs(UnmanagedType.BStr)] string pValue);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000d)]
        void UpdateRegistry([In] X509CertificateEnrollmentContext Context);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)]
        void RemoveFromRegistry([In] X509CertificateEnrollmentContext Context);
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB30C-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509PrivateKey
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Open();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void Create();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)]
        void Close();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)]
        void Delete();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)]
        void Verify([In] X509PrivateKeyVerify VerifyType);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020005)]
        void Import([In, MarshalAs(UnmanagedType.BStr)] string strExportType, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedKey, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        string Export([In, MarshalAs(UnmanagedType.BStr)] string strExportType, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020007)]
        CX509PublicKey ExportPublicKey();
        [DispId(0x60020008)]
        string ContainerName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] set; }
        [DispId(0x6002000a)]
        string ContainerNamePrefix { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000a)] set; }
        [DispId(0x6002000c)]
        string ReaderName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)] set; }
        [DispId(0x6002000e)]
        CCspInformations CspInformations { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000e)] set; }
        [DispId(0x60020010)]
        CCspStatus CspStatus { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020010)] set; }
        [DispId(0x60020012)]
        string ProviderName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020012)] set; }
        [DispId(0x60020014)]
        X509ProviderType ProviderType { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020014)] set; }
        [DispId(0x60020016)]
        bool LegacyCsp { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020016)] set; }
        [DispId(0x60020018)]
        CObjectId Algorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020018)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020018)] set; }
        [DispId(0x6002001a)]
        X509KeySpec KeySpec { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002001a)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002001a)] set; }
        [DispId(0x6002001c)]
        int Length { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002001c)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002001c)] set; }
        [DispId(0x6002001e)]
        X509PrivateKeyExportFlags ExportPolicy { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002001e)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002001e)] set; }
        [DispId(0x60020020)]
        X509PrivateKeyUsageFlags KeyUsage { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020020)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020020)] set; }
        [DispId(0x60020022)]
        X509PrivateKeyProtection KeyProtection { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020022)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020022)] set; }
        [DispId(0x60020024)]
        bool MachineContext { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020024)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020024)] set; }
        [DispId(0x60020026)]
        string SecurityDescriptor { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020026)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020026)] set; }
        [DispId(0x60020028)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020028)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020028)] set; }
        [DispId(0x6002002a)]
        string UniqueContainerName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002a)] get; }
        [DispId(0x6002002b)]
        bool Opened { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002b)] get; }
        [DispId(0x6002002c)]
        bool DefaultContainer { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002c)] get; }
        [DispId(0x6002002d)]
        bool Existing { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002d)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002d)] set; }
        [DispId(0x6002002f)]
        bool Silent { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002f)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002002f)] set; }
        [DispId(0x60020031)]
        int ParentWindow { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020031)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020031)] set; }
        [DispId(0x60020033)]
        string UIContextMessage { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020033)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020033)] set; }
        [DispId(0x60020035)]
        string Pin { [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020035)] set; }
        [DispId(0x60020036)]
        string FriendlyName { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020036)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020036)] set; }
        [DispId(0x60020038)]
        string Description { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020038)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020038)] set; }
    }

    [ComImport, TypeLibType((short) 0x1040), Guid("728AB30B-217D-11DA-B2A4-000E7BBB2B09")]
    public interface IX509PublicKey
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)]
        void Initialize([In, MarshalAs(UnmanagedType.Interface)] CObjectId pObjectId, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedKey, [In, MarshalAs(UnmanagedType.BStr)] string strEncodedParameters, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020001)]
        void InitializeFromEncodedPublicKeyInfo([In, MarshalAs(UnmanagedType.BStr)] string strEncodedPublicKeyInfo, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
        [DispId(0x60020002)]
        CObjectId Algorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; }
        [DispId(0x60020003)]
        int Length { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020003)] get; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; }
        
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)]
        string ComputeKeyIdentifier([In] KeyIdentifierHashAlgorithm Algorithm, [In, Optional, DefaultParameterValue(EncodingType.XCN_CRYPT_STRING_BASE64)] EncodingType Encoding);
    }

    [ComImport, Guid("728AB33C-217D-11DA-B2A4-000E7BBB2B09"), TypeLibType((short) 0x1040)]
    public interface IX509SignatureInformation
    {
        [DispId(0x60020000)]
        CObjectId HashAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020000)] set; }
        [DispId(0x60020002)]
        CObjectId PublicKeyAlgorithm { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020002)] set; }
        [DispId(0x60020004)]
        string this[EncodingType Encoding] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020004)] set; }
        [DispId(0x60020006)]
        bool AlternateSignatureAlgorithm { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020006)] set; }
        [DispId(0x60020008)]
        bool AlternateSignatureAlgorithmSet { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020008)] get; }
        [DispId(0x60020009)]
        bool NullSigned { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x60020009)] set; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000b)]
        CObjectId GetSignatureAlgorithm([In] bool Pkcs7Signature, [In] bool SignatureKey);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x6002000c)]
        void SetDefaultValues();
    }

    public enum KeyIdentifierHashAlgorithm
    {
        SKIHashDefault,
        SKIHashSha1,
        SKIHashCapiSha1
    }

    public enum ObjectIdGroupId
    {
        XCN_CRYPT_ANY_GROUP_ID = 0,
        XCN_CRYPT_ENCRYPT_ALG_OID_GROUP_ID = 2,
        XCN_CRYPT_ENHKEY_USAGE_OID_GROUP_ID = 7,
        XCN_CRYPT_EXT_OR_ATTR_OID_GROUP_ID = 6,
        XCN_CRYPT_FIRST_ALG_OID_GROUP_ID = 1,
        XCN_CRYPT_GROUP_ID_MASK = 0xffff,
        XCN_CRYPT_HASH_ALG_OID_GROUP_ID = 1,
        XCN_CRYPT_KEY_LENGTH_MASK = 0xfff0000,
        XCN_CRYPT_LAST_ALG_OID_GROUP_ID = 4,
        XCN_CRYPT_LAST_OID_GROUP_ID = 10,
        XCN_CRYPT_OID_DISABLE_SEARCH_DS_FLAG = -2147483648,
        XCN_CRYPT_OID_INFO_OID_GROUP_BIT_LEN_MASK = 0xfff0000,
        XCN_CRYPT_OID_INFO_OID_GROUP_BIT_LEN_SHIFT = 0x10,
        XCN_CRYPT_OID_PREFER_CNG_ALGID_FLAG = 0x40000000,
        XCN_CRYPT_POLICY_OID_GROUP_ID = 8,
        XCN_CRYPT_PUBKEY_ALG_OID_GROUP_ID = 3,
        XCN_CRYPT_RDN_ATTR_OID_GROUP_ID = 5,
        XCN_CRYPT_SIGN_ALG_OID_GROUP_ID = 4,
        XCN_CRYPT_TEMPLATE_OID_GROUP_ID = 9
    }

    public enum ObjectIdPublicKeyFlags
    {
        XCN_CRYPT_OID_INFO_PUBKEY_ANY = 0,
        XCN_CRYPT_OID_INFO_PUBKEY_ENCRYPT_KEY_FLAG = 0x40000000,
        XCN_CRYPT_OID_INFO_PUBKEY_SIGN_KEY_FLAG = -2147483648
    }

    public enum PFXExportOptions
    {
        PFXExportEEOnly,
        PFXExportChainNoRoot,
        PFXExportChainWithRoot
    }

    public enum Pkcs10AllowedSignatureTypes
    {
        AllowedKeySignature = 1,
        AllowedNullSignature = 2
    }

    public enum PolicyQualifierType
    {
        PolicyQualifierTypeUnknown,
        PolicyQualifierTypeUrl,
        PolicyQualifierTypeUserNotice
    }

    public enum PolicyServerUrlFlags
    {
        PsfAllowUnTrustedCA = 0x20,
        PsfAutoEnrollmentEnabled = 0x10,
        PsfLocationGroupPolicy = 1,
        PsfLocationRegistry = 2,
        PsfNone = 0,
        PsfUseClientId = 4
    }

    public enum PolicyServerUrlPropertyID
    {
        PsPolicyID,
        PsFriendlyName
    }

    public enum RequestClientInfoClientId
    {
        ClientIdAutoEnroll = 6,
        ClientIdAutoEnroll2003 = 2,
        ClientIdCertReq = 9,
        ClientIdCertReq2003 = 4,
        ClientIdDefaultRequest = 5,
        ClientIdEOBO = 8,
        ClientIdNone = 0,
        ClientIdRequestWizard = 7,
        ClientIdTest = 10,
        ClientIdUserStart = 0x3e8,
        ClientIdWizard2003 = 3,
        ClientIdXEnroll2003 = 1
    }

    public enum WebEnrollmentFlags
    {
        EnrollPrompt = 1
    }

    public enum X500NameFlags
    {
        XCN_CERT_NAME_STR_COMMA_FLAG = 0x4000000,
        XCN_CERT_NAME_STR_CRLF_FLAG = 0x8000000,
        XCN_CERT_NAME_STR_DISABLE_IE4_UTF8_FLAG = 0x10000,
        XCN_CERT_NAME_STR_DISABLE_UTF8_DIR_STR_FLAG = 0x100000,
        XCN_CERT_NAME_STR_ENABLE_PUNYCODE_FLAG = 0x200000,
        XCN_CERT_NAME_STR_ENABLE_T61_UNICODE_FLAG = 0x20000,
        XCN_CERT_NAME_STR_ENABLE_UTF8_UNICODE_FLAG = 0x40000,
        XCN_CERT_NAME_STR_FORCE_UTF8_DIR_STR_FLAG = 0x80000,
        XCN_CERT_NAME_STR_FORWARD_FLAG = 0x1000000,
        XCN_CERT_NAME_STR_NO_PLUS_FLAG = 0x20000000,
        XCN_CERT_NAME_STR_NO_QUOTING_FLAG = 0x10000000,
        XCN_CERT_NAME_STR_NONE = 0,
        XCN_CERT_NAME_STR_REVERSE_FLAG = 0x2000000,
        XCN_CERT_NAME_STR_SEMICOLON_FLAG = 0x40000000,
        XCN_CERT_OID_NAME_STR = 2,
        XCN_CERT_SIMPLE_NAME_STR = 1,
        XCN_CERT_X500_NAME_STR = 3,
        XCN_CERT_XML_NAME_STR = 4
    }

    public enum X509CertificateEnrollmentContext
    {
        ContextAdministratorForceMachine = 3,
        ContextMachine = 2,
        ContextUser = 1
    }

    public enum X509CertificateTemplateEnrollmentFlag
    {
        EnrollmentAddOCSPNoCheck = 0x1000,
        EnrollmentAddTemplateName = 0x200,
        EnrollmentAllowEnrollOnBehalfOf = 0x800,
        EnrollmentAutoEnrollment = 0x20,
        EnrollmentAutoEnrollmentCheckUserDSCertificate = 0x10,
        EnrollmentDomainAuthenticationNotRequired = 0x80,
        EnrollmentIncludeBasicConstraintsForEECerts = 0x8000,
        EnrollmentIncludeSymmetricAlgorithms = 1,
        EnrollmentNoRevocationInfoInCerts = 0x4000,
        EnrollmentPendAllRequests = 2,
        EnrollmentPreviousApprovalValidateReenrollment = 0x40,
        EnrollmentPublishToDS = 8,
        EnrollmentPublishToKRAContainer = 4,
        EnrollmentRemoveInvalidCertificateFromPersonalStore = 0x400,
        EnrollmentReuseKeyOnFullSmartCard = 0x2000,
        EnrollmentUserInteractionRequired = 0x100
    }

    public enum X509CertificateTemplateGeneralFlag
    {
        GeneralCA = 0x80,
        GeneralCrossCA = 0x800,
        GeneralDefault = 0x10000,
        GeneralDonotPersist = 0x1000,
        GeneralMachineType = 0x40,
        GeneralModified = 0x20000
    }

    public enum X509CertificateTemplatePrivateKeyFlag
    {
        PrivateKeyExportable = 0x10,
        PrivateKeyRequireAlternateSignatureAlgorithm = 0x40,
        PrivateKeyRequireArchival = 1,
        PrivateKeyRequireStrongKeyProtection = 0x20
    }

    public enum X509CertificateTemplateSubjectNameFlag
    {
        SubjectAlternativeNameEnrolleeSupplies = 0x10000,
        SubjectAlternativeNameRequireDirectoryGUID = 0x1000000,
        SubjectAlternativeNameRequireDNS = 0x8000000,
        SubjectAlternativeNameRequireDomainDNS = 0x400000,
        SubjectAlternativeNameRequireEmail = 0x4000000,
        SubjectAlternativeNameRequireSPN = 0x800000,
        SubjectAlternativeNameRequireUPN = 0x2000000,
        SubjectNameAndAlternativeNameOldCertSupplies = 8,
        SubjectNameEnrolleeSupplies = 1,
        SubjectNameRequireCommonName = 0x40000000,
        SubjectNameRequireDirectoryPath = -2147483648,
        SubjectNameRequireDNS = 0x10000000,
        SubjectNameRequireEmail = 0x20000000
    }

    public enum X509EnrollmentAuthFlags
    {
        X509AuthAnonymous = 1,
        X509AuthCertificate = 8,
        X509AuthKerberos = 2,
        X509AuthNone = 0,
        X509AuthUsername = 4
    }

    public enum X509EnrollmentPolicyExportFlags
    {
        ExportCAs = 4,
        ExportOIDs = 2,
        ExportTemplates = 1
    }

    public enum X509EnrollmentPolicyLoadOption
    {
        LoadOptionCacheOnly = 1,
        LoadOptionDefault = 0,
        LoadOptionRegisterForADChanges = 4,
        LoadOptionReload = 2
    }

    public enum X509KeySpec
    {
        XCN_AT_NONE,
        XCN_AT_KEYEXCHANGE,
        XCN_AT_SIGNATURE
    }

    public enum X509KeyUsageFlags
    {
        XCN_CERT_CRL_SIGN_KEY_USAGE = 2,
        XCN_CERT_DATA_ENCIPHERMENT_KEY_USAGE = 0x10,
        XCN_CERT_DECIPHER_ONLY_KEY_USAGE = 0x8000,
        XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE = 0x80,
        XCN_CERT_ENCIPHER_ONLY_KEY_USAGE = 1,
        XCN_CERT_KEY_AGREEMENT_KEY_USAGE = 8,
        XCN_CERT_KEY_CERT_SIGN_KEY_USAGE = 4,
        XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE = 0x20,
        XCN_CERT_NO_KEY_USAGE = 0,
        XCN_CERT_NON_REPUDIATION_KEY_USAGE = 0x40,
        XCN_CERT_OFFLINE_CRL_SIGN_KEY_USAGE = 2
    }

    public enum X509PrivateKeyExportFlags
    {
        XCN_NCRYPT_ALLOW_ARCHIVING_FLAG = 4,
        XCN_NCRYPT_ALLOW_EXPORT_FLAG = 1,
        XCN_NCRYPT_ALLOW_EXPORT_NONE = 0,
        XCN_NCRYPT_ALLOW_PLAINTEXT_ARCHIVING_FLAG = 8,
        XCN_NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG = 2
    }

    public enum X509PrivateKeyProtection
    {
        XCN_NCRYPT_UI_NO_PROTECTION_FLAG,
        XCN_NCRYPT_UI_PROTECT_KEY_FLAG,
        XCN_NCRYPT_UI_FORCE_HIGH_PROTECTION_FLAG
    }

    public enum X509PrivateKeyUsageFlags
    {
        XCN_NCRYPT_ALLOW_ALL_USAGES = 0xffffff,
        XCN_NCRYPT_ALLOW_DECRYPT_FLAG = 1,
        XCN_NCRYPT_ALLOW_KEY_AGREEMENT_FLAG = 4,
        XCN_NCRYPT_ALLOW_SIGNING_FLAG = 2,
        XCN_NCRYPT_ALLOW_USAGES_NONE = 0
    }

    public enum X509PrivateKeyVerify
    {
        VerifyNone,
        VerifySilent,
        VerifySmartCardNone,
        VerifySmartCardSilent,
        VerifyAllowUI
    }

    public enum X509ProviderType
    {
        XCN_PROV_DH_SCHANNEL = 0x12,
        XCN_PROV_DSS = 3,
        XCN_PROV_DSS_DH = 13,
        XCN_PROV_EC_ECDSA_FULL = 0x10,
        XCN_PROV_EC_ECDSA_SIG = 14,
        XCN_PROV_EC_ECNRA_FULL = 0x11,
        XCN_PROV_EC_ECNRA_SIG = 15,
        XCN_PROV_FORTEZZA = 4,
        XCN_PROV_INTEL_SEC = 0x16,
        XCN_PROV_MS_EXCHANGE = 5,
        XCN_PROV_NONE = 0,
        XCN_PROV_REPLACE_OWF = 0x17,
        XCN_PROV_RNG = 0x15,
        XCN_PROV_RSA_AES = 0x18,
        XCN_PROV_RSA_FULL = 1,
        XCN_PROV_RSA_SCHANNEL = 12,
        XCN_PROV_RSA_SIG = 2,
        XCN_PROV_SPYRUS_LYNKS = 20,
        XCN_PROV_SSL = 6
    }

    public enum X509RequestInheritOptions
    {
        InheritDefault = 0,
        InheritExtensionsFlag = 0x100,
        InheritKeyMask = 15,
        InheritNewDefaultKey = 1,
        InheritNewSimilarKey = 2,
        InheritNone = 0x10,
        InheritPrivateKey = 3,
        InheritPublicKey = 4,
        InheritRenewalCertificateFlag = 0x20,
        InheritSubjectAltNameFlag = 0x200,
        InheritSubjectFlag = 0x80,
        InheritTemplateFlag = 0x40,
        InheritValidityPeriodFlag = 0x400
    }

    public enum X509RequestType
    {
        TypeAny,
        TypePkcs10,
        TypePkcs7,
        TypeCmc,
        TypeCertificate
    }
}

 

 
