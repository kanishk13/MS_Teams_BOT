//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//----------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace MS.GTA.BOTService.Common.Enum
{
    [DataContract(Namespace = "MS.GTA.TalentEngagement")]
    public enum CandidateStatus
    {
        [EnumMember(Value = "available")]
        Available = 0,
        [EnumMember(Value = "notAvailable")]
        NotAvailable = 1
    }
}
