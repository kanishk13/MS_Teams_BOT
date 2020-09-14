//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//----------------------------------------------------------------------------

namespace MS.GTA.BOTService.Common.Entities
{
    using System;
    using System.Runtime.Serialization;
    using MS.GTA.BOTService.Common.Enum;

    [DataContract]
    public class JobApplicationParticipant
    {
        [DataMember(Name = "OID", EmitDefaultValue = false, IsRequired = false)]
        public string OID { get; set; }

        [DataMember(Name = "Role", EmitDefaultValue = false, IsRequired = false)]
        public JobParticipantRole? Role { get; set; }

        /// <summary>Gets or sets the AddedOnDate </summary>
        [DataMember(Name = "AddedOnDate", EmitDefaultValue = false, IsRequired = false)]
        public DateTime? AddedOnDate { get; set; }
    }
}
