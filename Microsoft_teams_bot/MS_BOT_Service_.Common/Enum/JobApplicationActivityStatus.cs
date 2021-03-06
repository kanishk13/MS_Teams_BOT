//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//----------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace MS.GTA.BOTService.Common.Enum
{
    [DataContract(Namespace = "MS.GTA.TalentEngagement")]
    public enum JobApplicationActivityStatus
    {
        [EnumMember(Value = "planned")]
        Planned = 0,
        [EnumMember(Value = "started")]
        Started = 1,
        [EnumMember(Value = "completed")]
        Completed = 2,
        [EnumMember(Value = "cancelled")]
        Cancelled = 3,
        [EnumMember(Value = "skipped")]
        Skipped = 4
    }
}
