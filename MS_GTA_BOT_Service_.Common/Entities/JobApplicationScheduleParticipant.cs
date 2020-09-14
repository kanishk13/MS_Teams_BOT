//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//----------------------------------------------------------------------------

namespace MS.GTA.BOTService.Common.Entities
{
    using System;
    using System.Runtime.Serialization;
    using MS.GTA.BOTService.Common.Enum;

    [DataContract]
    public class JobApplicationScheduleParticipant
    {
        [DataMember(Name = "OID", EmitDefaultValue = false, IsRequired = false)]
        public string OID { get; set; }

        [DataMember(Name = "ParticipantMetadata", EmitDefaultValue = false, IsRequired = false)]
        public string ParticipantMetadata { get; set; }

        [DataMember(Name = "ParticipantResponseStatus", EmitDefaultValue = false, IsRequired = false)]
        public InvitationResponseStatus ParticipantStatus { get; set; }

        [DataMember(Name = "ParticipantResponseComments", EmitDefaultValue = false, IsRequired = false)]
        public string ParticipantComments { get; set; }

        [DataMember(Name = "IsAssessmentCompleted", EmitDefaultValue = false, IsRequired = false)]
        public bool IsAssessmentCompleted { get; set; }

        [DataMember(Name = "Role", EmitDefaultValue = false, IsRequired = false)]
        public JobParticipantRole? Role { get; set; }

        [DataMember(Name = "ParticipantResponseDateTime", EmitDefaultValue = false, IsRequired = false)]
        public DateTime? ParticipantResponseDateTime { get; set; }
    }
}
