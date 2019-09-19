// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Data
{

    // Important!: this enum is only used in session API and subject to change
    public enum JobState
    {
        Configuring = 0x001,
        
        Submitted = 0x002,
        
        Validating = 0x004,
        
        ExternalValidation = 0x008,
        
        Queued = 0x010,
        
        Running = 0x020,
       
        Finishing = 0x040,
        
        Finished = 0x080,
       
        Failed = 0x100,
        
        Canceled = 0x200,
        
        Canceling = 0x400,
        
        All = Configuring | Submitted | Validating | ExternalValidation | Queued | Running | Finishing | Finished | Failed | Canceling | Canceled,
    }
}
