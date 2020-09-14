namespace MS.GTA.BOTService.Common.Models
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This represents the summary of Job Application schedule
    /// </summary>
    [DataContract]
    public class ScheduleSummary
    {
        [DataMember(Name = "JobApplicationId", EmitDefaultValue = false, IsRequired = false)]
        public string JobApplicationId { get; set; }

        [DataMember(Name = "CandidateName", EmitDefaultValue = false, IsRequired = false)]
        public string CandidateName { get; set; }

        [DataMember(Name = "ScheduleStartDateTime", EmitDefaultValue = false, IsRequired = false)]
        public DateTime? ScheduleStartDateTime { get; set; }
    }

}
