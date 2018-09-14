using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

[assembly: Guid("7212bcd2-6b84-415c-a12f-b49a77dee393")]

namespace Microsoft.Hpc.Scheduler.Properties
{
    static class ComGuids
    {
        //COM GUIDS for shared classes
        internal const string GuidINameValue = "FB04AB87-FC05-4374-8DAE-59D84633379B";
        internal const string GuidNameValueClass = "6DC2D910-42DB-4C6A-B4EC-49A66761F739";

        internal const string GuidINameValueCollection = "BB14B74B-0BB7-4A01-8E05-A988D636B5EA";
        internal const string GuidNameValueCollectionClass = "456DE413-6E74-49BF-8334-DAFB7D81D473";

        internal const string GuidIFilterProperty = "FA5EDE58-69F3-4229-8EE4-8CCD777B5662";
        internal const string GuidFilterPropertyClass = "EA602C99-B32A-4A48-AD45-988F2914B2C7";

        internal const string GuidISortProperty = "8037A4AD-AD6E-4522-A24A-43A23422739F";
        internal const string GuidSortPropertyClass = "CD2EF552-A818-4BDB-A4E6-B30A821CDCDD";

        internal const string GuidFilterOperatorClass = "923DF81C-82BE-4D3B-BBC0-D01DF5EDC510";
        internal const string GuidSortOrderClass = "6283E28A-C02C-40D0-A688-BE537C779953";

        internal const string GuidUserPrivilege = "FC865149-5068-4561-AB00-9BF8CEE7EC40";
        internal const string GuidUserRoles = "06B19F0A-74E8-42f5-BD36-E21538A4F3A5";

        //scheduler related COM GUIDs
        internal const string GuidIScheduler = "D0ED926A-6E0C-11DC-A924-ABC756D89593";
        internal const string GuidSchedulerClass = "3C376723-5FF9-4C55-89BB-E9E7A31577E1";
        internal const string GuidConnectMethod = "E0EA2C08-C020-41A3-8E17-4E96F7563EE8";

        //job related COM GUIDs
        internal const string GuidIJob = "9348E87E-5917-48CE-9889-D666EA4E9937";
        internal const string GuidJobClass = "B2934CF1-FBD1-42A0-8767-8AC5DDE1618D";

        internal const string GuidJobPriority = "784AD2AF-E429-4CA3-8B0D-355AEBC02F1C";
        internal const string GuidJobState = "BEDCD0AF-C2E1-40DB-96A8-65478CE50C6C";
        internal const string GuidJobUnitType = "90A57885-5870-4326-9EF2-6EFD7BFCEE97";
        internal const string GuidJobType = "E80BA82F-E71A-4D93-B9DB-B1F5B6E3E9AA";
        internal const string GuidJobNodeGroupOp = "835117EE-32FB-4F57-92B6-478D8AFCF14B";

        //task related COM GUIDs
        internal const string GuidITask = "8A16A4A9-95A9-40CB-9027-C84BB27646DC";
        internal const string GuidTaskClass = "3F3E4C6A-4400-4E60-A974-0F4D6F34CE53";

        internal const string GuidTaskState = "A78267A1-029C-4B05-8317-20576D4B782E";
        internal const string GuidTaskType = "E0165E48-568A-43bd-BE2D-C2FE864F2C67";
        internal const string GuidTaskId = "905104BE-C738-481c-AE52-285C3253870B";
        internal const string GuidITaskId = "D38CDF30-B8EB-42aa-A9AB-B6176CCD565F";

        //node related COM GUIDs
        internal const string GuidINode = "07C12C04-FB39-437F-B242-691B802E30A3";
        internal const string GuidNodeClass = "F9D5914B-AC7A-4A77-8512-2073BB993B36";

        internal const string GuidNodeState = "534F6FEC-A629-40C8-8AC0-4BEB0667B305";
        internal const string GuidNodeAvailability = "13EBEC8D-908C-48b9-9817-0CC0B524698B";

        internal const string GuidNodeLocation = "758A900A-372D-4534-9464-34235C80F23C";

        //Job message related COM GUIDs
        internal const string GuidJobMessageType = "42A48FB8-C934-4e27-9A8E-6DB995C7BA27";
    }
}
