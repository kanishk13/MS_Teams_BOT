using MS.GTA.BOTService.Common.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MS.GTA.BOTService.Common.Entities
{
    [DataContract]
    public class CandidateSkill
    {
        [DataMember(Name = "CandidateSkillID", EmitDefaultValue = false, IsRequired = false)]
        public string CandidateSkillID { get; set; }

        [DataMember(Name = "Skill", EmitDefaultValue = false, IsRequired = false)]
        public string Skill { get; set; }
    }
}
