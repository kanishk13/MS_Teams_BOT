namespace MS.GTA.BOTService.BusinessLibrary.Business
{
    using Microsoft.Extensions.Logging;
    using MS.GTA.BOTService.Common.Models;
    using MS.GTA.BOTService.Data.Interfaces;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using MS.GTA.BOTService.BusinessLibrary.Interfaces;

    public class JobApplicationManager: IJobApplicationManager
    {
        /// <summary>Job Application query client.</summary>
        private readonly IJobApplicationQuery jobApplicationQuery;

        /// <summary>
        /// The instance for <see cref="ILogger{JobApplicationManager}"/>
        /// </summary>
        private readonly ILogger<JobApplicationManager> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobApplicationManager" /> class.
        /// </summary>
        /// <param name="jobApplicationQuery">Job Application Query Accessor</param>
        /// <param name="falconQuery">query accessor</param>
        /// <param name="feedbackQuery">The instance for <see cref="IFeedbackQuery"/>.</param>
        /// <param name="logger">The instance for <see cref="ILogger{JobApplicationManager}"/></param>
        public JobApplicationManager(
            IJobApplicationQuery jobApplicationQuery,
            ILogger<JobApplicationManager> logger)
        {
            this.jobApplicationQuery = jobApplicationQuery;
            this.logger = logger;
        }

        public async Task<IList<UpcomingInterviews>> GetUpcomingInterviews(string userOid, DateTime startDateTime)
        {
            if (string.IsNullOrEmpty(userOid))
            {
                throw new InvalidOperationException("Invalid user oid");
            }

            if (startDateTime == null || startDateTime == default(DateTime))
            {
                startDateTime = DateTime.UtcNow;
            }

            if (!DateTime.TryParse(startDateTime.ToString(CultureInfo.InvariantCulture), out startDateTime))
            {
                throw new InvalidOperationException("Invalid date");
            }

            List<string> jobApplicationIds = new List<string>();

            var jobOpenings = await this.jobApplicationQuery.GetActiveJobApplications(userOid);

            jobOpenings?.ForEach(jo =>
            {
                var jaIds = jo?.JobApplications?.Select(applications => applications.JobApplicationID).ToList();
                jobApplicationIds?.AddRange(jaIds);
            });

            var jobApplicationSchedules = await this.jobApplicationQuery.GetSchedulesForJobApplications(jobApplicationIds, startDateTime);

            IList<UpcomingInterviews> upcomingInterviews = new List<UpcomingInterviews>();

            jobOpenings?.ForEach(jo =>
            {
                var jaIdsForJobOpening = jo?.JobApplications?.Where(applications => applications?.JobOpening?.ExternalJobOpeningID == jo.ExternalJobOpeningID)?
                .Select(applications => applications.JobApplicationID).ToList();

                var scheduleSummaries = jobApplicationSchedules?.Where(jas => jaIdsForJobOpening.Contains(jas.JobApplicationId)).ToList();

                if(scheduleSummaries.Any())
                {
                    scheduleSummaries?.ForEach(ss =>
                    {
                        ss.CandidateName = jo?.JobApplications?.FirstOrDefault(app => app.JobApplicationID == ss.JobApplicationId)?
                        .Candidate?.FullName?.GivenName;
                    });

                    var upcomingInterviewForJobOpening = new UpcomingInterviews
                    {
                        ExternalJobOpeningId = jo.ExternalJobOpeningID,
                        PositionTitle = jo.PositionTitle,
                        ScheduleSummaries = scheduleSummaries?.OrderBy(ss => ss.ScheduleStartDateTime).ToList(),
                    };
                    upcomingInterviews.Add(upcomingInterviewForJobOpening);
                }
            });

            return upcomingInterviews;
        }
    }
}