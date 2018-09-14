using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

[assembly: Guid("c45d10a1-54e8-420b-a052-719d47ec7c16")]

namespace Microsoft.Hpc.Scheduler
{
    static class ComGuids
    {
        //COM GUIDS for shared classes
        internal const string GuidINameValue = "FB04AB87-FC05-4374-8DAE-59D84633379B";
        internal const string GuidNameValueClass = "6DC2D910-42DB-4C6A-B4EC-49A66761F739";

        internal const string GuidINameValueCollection = "BB14B74B-0BB7-4A01-8E05-A988D636B5EA";
        internal const string GuidNameValueCollectionClass = "456DE413-6E74-49BF-8334-DAFB7D81D473";

        internal const string GuidISchedulerCollection = "1e892b76-ac3f-4732-9735-7d7b246d9e31";
        internal const string GuidSchedulerCollectionClass = "f9549a2b-17f8-4b46-8851-5be41880f9f7";

        internal const string GuidIFilterProperty = "FA5EDE58-69F3-4229-8EE4-8CCD777B5662";
        internal const string GuidFilterPropertyClass = "EA602C99-B32A-4A48-AD45-988F2914B2C7";

        internal const string GuidISortProperty = "8037A4AD-AD6E-4522-A24A-43A23422739F";
        internal const string GuidSortPropertyClass = "CD2EF552-A818-4BDB-A4E6-B30A821CDCDD";

        internal const string GuidFilterOperatorClass = "923DF81C-82BE-4D3B-BBC0-D01DF5EDC510";
        internal const string GuidSortOrderClass = "6283E28A-C02C-40D0-A688-BE537C779953";

        internal const string GuidIFilterCollection = "ef89423b-f79b-466f-8127-bdab910ae6c1";
        internal const string GuidFilterCollectionClass = "392f3fe6-cc93-4f88-a692-735875ccba97";

        internal const string GuidISortCollection = "3f5253c6-64a1-4148-aa2c-13aa66040a80";
        internal const string GuidSortCollectionClass = "39e4e48e-7f51-433d-ac20-8ac7c8eaa228";

        internal const string GuidIIntCollection = "bf495dab-3237-4f58-babe-ed51217bd665";
        internal const string GuidIntCollectionClass = "46fe2eba-f456-4284-a3bd-d3fdf6521012";

        internal const string GuidIStringCollection = "0449fa77-10bd-4113-9e3e-2dc712eba36c";
        internal const string GuidStringCollectionClass = "59fea435-088a-4459-be0e-9d081a673317";

        internal const string GuidIPropertyIdCollection = "FB6D2A92-3C48-4237-B300-FC3E31C2FACC";
        internal const string GuidPropertyIdCollectionClass = "0224915A-84AF-4863-9316-30377139F236";

        internal const string GuidISchedulerRowEnumerator = "FE420183-93C4-46e0-BA66-31DEC84571DB";
        internal const string GuidSchedulerRowEnumerator = "7F5CBCC7-60E2-46ec-ADC2-AE2FBEBE0384";

        internal const string GuidISchedulerRowSet = "6A579152-4C55-4dc1-8BBB-FDEC44B5D2FC";
        internal const string GuidSchedulerRowSet = "736E34D9-F331-4625-B870-3F2F307FF15A";

        internal const string GuidJobTemplateInfo = "61CAF097-8566-49d2-AC56-4B4FDAD531B3";

        //scheduler related COM GUIDs
        internal const string GuidISchedulerV2 = "D0ED926A-6E0C-11DC-A924-ABC756D89593";
        internal const string GuidSchedulerClass = "3C376723-5FF9-4C55-89BB-E9E7A31577E1";
        internal const string GuidISchedulerCounters = "1FD3409E-4B7C-4eeb-9BD9-867238EB191F";
        internal const string GuidSchedulerCounters = "256241B2-42E8-41fc-AEFB-365CAC3EF7A8";
        internal const string GuidISchedulerV3 = "386C7C44-AA54-4683-B9C0-3653163BE130";
        internal const string GuidISchedulerV3SP2 = "1B4E1239-5BF8-4447-98B7-826943D15FA5";
        internal const string GuidISchedulerV3SP3 = "6C92F1B5-8CF0-444B-94C7-F1EF8D89F023";
        internal const string GuidISchedulerV3SP4 = "2B2E7861-777D-4C00-9CAE-38F83E1C7B53";
        internal const string GuidISchedulerV4 = "21BF1489-8526-4D0E-8524-7AC6D9D4A629";
        internal const string GuidISchedulerV4SP1 = "96BA5B90-1CD1-471f-8E1D-5AF63D36007F";
        internal const string GuidISchedulerV4SP3 = "CD27EFF2-DF22-458B-B181-1563BE5527C5";
        internal const string GuidISchedulerV5 = "F10017E8-B9D3-4171-A974-406C6A7E62EE";
        internal const string GuidISchedulerV5SP2 = "E1E4FFD9-DC7F-49D2-BFA2-0F02D9FB3558";

        //job related COM GUIDs
        internal const string GuidIJobV2 = "9348E87E-5917-48CE-9889-D666EA4E9937";
        internal const string GuidJobClass = "B2934CF1-FBD1-42A0-8767-8AC5DDE1618D";
        internal const string GuidIJobV3 = "0AEA8629-72FB-4afa-AE88-419273D3DB50";
        internal const string GuidIJobV3SP1 = "D7449A23-D29D-4414-B72D-C582DBABDA14";
        internal const string GuidIJobV3SP2 = "065BE3AD-7981-49C2-B7E4-B34E3E01F384";
        internal const string GuidIJobV3SP3 = "7b713f15-8de0-45cc-8d8c-f090b887616b";
        internal const string GuidIJobV4 = "71979B0A-F6FB-4D59-B6B2-EEDC5784A36D";
        internal const string GuidIJobV4SP1 = "9296C11C-C08F-42FA-BCB1-D2FECCF3BA7B";
        internal const string GuidIJobV4SP2 = "8C0575D5-B294-4FD1-AC86-1AEB6A07D9D9";
        internal const string GuidIJobV4SP3 = "A74CC64C-0D52-435C-A9C2-409841877B12";
        internal const string GuidIJobV4SP5 = "467218B0-24B8-4B6C-AE7E-AD8A5C1D6C87";
        internal const string GuidIJobV4SP6 = "846E919F-C7AA-434D-B3EC-A84C7D15E0DC";
        internal const string GuidIJobV5 = "DF0408D3-94C5-4F06-A6F8-FD4FAE4CCB45";
        internal const string GuidIJobV5SP2 = "34397BEC-64E1-4B2B-877F-E057D4D26BA2";

        internal const string GuidJobPriority = "784AD2AF-E429-4CA3-8B0D-355AEBC02F1C";
        internal const string GuidJobState = "BEDCD0AF-C2E1-40DB-96A8-65478CE50C6C";
        internal const string GuidJobUnitType = "90A57885-5870-4326-9EF2-6EFD7BFCEE97";
        internal const string GuidJobType = "E80BA82F-E71A-4D93-B9DB-B1F5B6E3E9AA";

        internal const string GuidIJobCounters = "6D3F0397-E8AC-4369-A048-E70286E575B8";
        internal const string GuidJobCountersClass = "7B2F3E5D-CA77-4ad5-9A5C-7292E684A790";

        //job events COM GUIDs
        internal const string GuidIJobStateChangeEventArgs = "337c8713-e312-4c69-b023-dec2dd6b381f";
        internal const string GuidJobStateChangeEventArgsClass = "c6f1db8b-1504-4ceb-a178-00e7bc9f0285";
        internal const string GuidITaskStateChangeEventArgs = "70d8316e-6dab-4e99-af3b-b39fb9afdc3e";
        internal const string GuidTaskStateChangeEventArgsClass = "6494eef8-f105-4ad6-90c5-9783b63a1f2c";
        internal const string GuidISchedulerJobEvents = "47956563-23c2-4270-80c8-91a3b369acb7";

        //Job message related COM GUIDs
        internal const string GuidJobMessageType = "42A48FB8-C934-4e27-9A8E-6DB995C7BA27";

        //task related COM GUIDs
        internal const string GuidITaskV2 = "8A16A4A9-95A9-40CB-9027-C84BB27646DC";
        internal const string GuidTaskClass = "3F3E4C6A-4400-4E60-A974-0F4D6F34CE53";
        internal const string GuidITaskV3 = "EF2125B9-9C03-4948-B4C1-859C9C94CDD4";
        internal const string GuidITaskV3SP1 = "9b7df4b9-18ed-42b0-8364-b1f7a034d492";
        internal const string GuidITaskV4 = "8EBF111B-9BE0-4EDD-834E-1AA238DAAE61";
        internal const string GuidITaskV4SP1 = "9AB241D9-A924-45E5-B05F-B94953F9D87F";
        internal const string GuidITaskV4SP3 = "6AE108D4-05C3-42E1-BDF5-3B9EB82D9976";
        internal const string GuidITaskV4SP5 = "EB8AC320-EEF6-4FD9-9505-79A46B1F0094";


        internal const string GuidTaskState = "A78267A1-029C-4B05-8317-20576D4B782E";
        internal const string GuidTaskType = "E0165E48-568A-43bd-BE2D-C2FE864F2C67";

        internal const string GuidITaskCounters = "985D81B2-64FD-4e98-8270-0CB99155199F";
        internal const string GuidTaskCountersClass = "A38EE29E-09D0-4b5a-9F4B-1CE778960946";

        //node related COM GUIDs
        internal const string GuidINodeV2 = "07C12C04-FB39-437F-B242-691B802E30A3";
        internal const string GuidNodeClass = "F9D5914B-AC7A-4A77-8512-2073BB993B36";
        internal const string GuidINodeV3 = "F3B0F4D7-FAE3-429d-B75F-10278A3BEC07";
        internal const string GuidINodeV3SP1 = "18E9EE1D-D223-46E1-8C58-E00F7D1D2ED4";
        internal const string GuidINodeV3SP2 = "DF63E52F-3D77-467C-848F-C96CD95CD0D9";
        internal const string GuidINodeV4SP3 = "551E917C-94E8-49CC-83F4-D60FC8E5640D";

        internal const string GuidNodeState = "534F6FEC-A629-40C8-8AC0-4BEB0667B305";
        internal const string GuidNodeAvailability = "13EBEC8D-908C-48b9-9817-0CC0B524698B";

        internal const string GuidINodeCounters = "4F4AF2FE-4EE2-4a7f-B546-398F6BAF40CA";
        internal const string GuidNodeCountersClass = "B9CCBC76-7756-42a9-90A1-DD6F750B9D60";

        //node event COM 
        internal const string GuidINodeStateChangeEventArgs = "36F87816-1FD7-48d0-B7E7-3FC5920E3CE1";
        internal const string GuidNodeStateChangeEventArgsClass = "373E9F7B-E11F-40bb-BFFA-728C8D32971A";
        internal const string GuidISchedulerNodeEvents = "8644EB2D-B177-43f7-8BB5-53C7D3ECB605";

        //Processor related COM GUIDs
        internal const string GuidISchedulerProcessor = "D13525E7-E7E9-4fcb-B97C-2FFAA3F09CCF";
        internal const string GuidSchedulerProcessor = "558F7CAA-C675-4824-A66A-BC286ADBB22D";
        internal const string GuidSchedulerProcessorState = "53689F11-CB13-4e80-8CF8-54B441C79FDD";

        //Core related COM GUIDs
        internal const string GuidSchedulerCoreState = "CEBF8417-6664-4c4a-A1EE-101F924CFE21";

        //Remote command COM GUIDs
        internal const string GuidIRemoteCommand = "f8d669bc-0579-41b9-ac26-384c3c8e06f8";
        internal const string GuidRemoteCommandClass = "c8c9c032-3c94-40e0-92f3-598920f00b57";
        internal const string GuidIRemoteComandNodeSelection = "31e24d7d-530c-4719-8ca1-cb79dd3a0dc0";
        internal const string GuidRemoteComandNodeSelectionClass = "071afadd-ef91-4dc7-8fda-80b9b2c1f08d";
        internal const string GuidIRemoteComandInfo = "03811174-809c-4143-b44c-bbaa1ad8de67";
        internal const string GuidRemoteComandInfoClass = "6b8fb453-9e59-4349-8116-f5d1a6c34550";
        internal const string GuidIRemoteComandEvents = "9ce35cdf-a433-4e02-b226-5d2c2143adbd";
        internal const string GuidCommandTaskStateEventArgClass = "86816512-60c4-42f9-add8-76abf894ed57";
        internal const string GuidICommandTaskStateEventArg = "2db7b40d-ae72-4b01-8846-82fb252b9e82";
        internal const string GuidCommandOutputEventArgClass = "c20f07e6-fff9-42bb-bee6-47a025c74822";
        internal const string GuidICommandOutputEventArg = "32dca0f1-8ab7-4496-93be-391defced2fd";
        internal const string GuidCommandRawOutputEventArgClass = "531988b0-70ba-4ed4-b40a-2a35516d19ea";
        internal const string GuidICommandRawOutputEventArg = "5a570dc3-8ec6-42a3-8dbb-4da8d98f39d0";

        //PropId COM GUID
        internal const string GuidPropIdEnum = "99994F5F-6106-4142-8C31-AFCBC8C0A326";

        //Version GUID

        internal const string GuidIServerVersion = "B8579C31-91F3-441c-8BB9-AC5E4E5D1200";
        internal const string GuidServerVersion = "352F26D5-F0AD-491e-8585-55B59D03CDAC";

        //Connection event GUID
        internal const string GuidConnectionEventArg = "36417EDA-62A1-4f0c-BD09-B7C1A7FC8F51";
        internal const string GuidConnectionEventArgClass = "36966996-CCF3-460c-8ADF-8958964EC5FC";
        internal const string GuidConnectionEventCode = "A46F67A3-9C6E-434b-A02A-31765BE668B1";

        //Node Reachability event GUIDs
        internal const string GuidINodeReachableChangeEventArgs = "8A2C30C9-A2B4-4A27-81CA-25089DAFD5AD";
        internal const string GuidNodeReachableChangeEventArgsClass = "BB7B872A-F358-4B52-996D-D40021968908";
        internal const string GuidISchedulerNodeReachableEvents = "DFAE8BED-89B4-4536-8A2A-208C84CDF87A";

        //Pool related Guids
        internal const string GuidISchedulerPool = "78D5F3F4-3562-4975-AAF0-8EC1CB1541F6";
        internal const string GuidSchedulerPoolClass = "351F693A-5C82-45F3-928F-70E31518ACCD";
    }
}

