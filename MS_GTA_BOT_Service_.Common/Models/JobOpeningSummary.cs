namespace MS.GTA.BOTService.Common.Models
{
    using MS.GTA.BOTService.Common.Entities;
    using MS.GTA.BOTService.Common.Enum;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// This represents the summary of Job Applications
    /// </summary>
    [DataContract]
    public class JobOpeningSummary
    {
        [DataMember(Name = "ExternalJobOpeningID", EmitDefaultValue = false, IsRequired = false)]
        public string ExternalJobOpeningID { get; set; }

        [DataMember(Name = "PositionTitle", EmitDefaultValue = false, IsRequired = false)]
        public string PositionTitle { get; set; }

        [DataMember(Name = "JobApplications", EmitDefaultValue = false, IsRequired = false)]
        public List<JobApplication> JobApplications { get; set; }

        [DataMember(Name = "ReviewApplications", EmitDefaultValue = false, IsRequired = false)]
        public List<JobApplication> ReviewApplications { get; set; }
        

        [DataMember(Name = "JobOpeningStatus", EmitDefaultValue = false, IsRequired = false)]
        public JobOpeningStatus? JobOpeningStatus { get; set; }

        [DataMember(Name = "JobOpeningStatusReason", EmitDefaultValue = false, IsRequired = false)]
        public JobOpeningStatusReason? JobOpeningStatusReason { get; set; }

        [DataMember(Name = "TotalApplications", EmitDefaultValue = false, IsRequired = false)]
        public int? TotalApplications { get; set; }

        [DataMember(Name = "DispositionedApplications", EmitDefaultValue = false, IsRequired = false)]
        public int? DispositionedApplications { get; set; }

        [DataMember(Name = "InterviewApplications", EmitDefaultValue = false, IsRequired = false)]
        public int? InterviewApplications { get; set; }

        [DataMember(Name = "AssessmentApplications", EmitDefaultValue = false, IsRequired = false)]
        public int? AssessmentApplications { get; set; }
    }

}