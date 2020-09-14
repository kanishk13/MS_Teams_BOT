namespace MS.GTA.BOTService.Common.Models
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// This represents the summary of Job Application schedule
    /// </summary>
    [DataContract]
    public class UpcomingInterviews
    {
        [DataMember(Name = "ExternalJobOpeningId", EmitDefaultValue = false, IsRequired = false)]
        public string ExternalJobOpeningId { get; set; }

        [DataMember(Name = "PositionTitle", EmitDefaultValue = false, IsRequired = false)]
        public string PositionTitle { get; set; }

        [DataMember(Name = "ScheduleSummaries", EmitDefaultValue = false, IsRequired = false)]
        public IList<ScheduleSummary> ScheduleSummaries { get; set; }
    }

}
