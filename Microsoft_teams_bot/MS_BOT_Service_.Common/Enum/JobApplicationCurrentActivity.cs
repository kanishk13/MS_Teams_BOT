//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//----------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace MS.GTA.BOTService.Common.Enum
{
    [DataContract(Namespace = "MS.GTA.TalentEngagement")]
    public enum JobApplicationCurrentActivity
    {
        [EnumMember(Value = "default")]
        Default = 0
    }
}
