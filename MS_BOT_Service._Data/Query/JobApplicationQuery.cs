//----------------------------------------------------------------------------
// <copyright file="JobApplicationQuery.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using MS.GTA.BOTService.Data.Interfaces;
using MS.GTA.BOTService.Data.Contracts;
using Microsoft.Extensions.Options;
using MS.GTA.BOTService.Common.Entities;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using MS.GTA.BOTService.Common.Enum;
using MS.GTA.BOTService.Common.Utility;
using System.Linq;
using System.Threading.Tasks;
using MS.GTA.BOTService.Common.Models;
using System.Globalization;

namespace MS.GTA.BOTService.Data.Query
{
    /// <summary>
    /// Job application query class.
    /// </summary>
    public class JobApplicationQuery : IJobApplicationQuery
    {
        /// <summary>
        /// The instance for <see cref="ILogger{JobApplicationQuery}"/>.
        /// </summary>
        private readonly ILogger<JobApplicationQuery> logger;

        /// <summary>
        /// The instance for <see cref="CosmosDBConfiguration"/>.
        /// </summary>
        private readonly CosmosDBConfiguration cosmosDBConfiguration;

        /// <summary>
        /// Gets or sets the CDS query client.
        /// </summary>
        internal ICosmosQueryClientProvider cosmosQueryClientProvider { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobApplicationQuery"/> class.
        /// constructor
        /// </summary>
        /// <param name="falconQueryClient">falcon query client</param>
        /// <param name="graphProvider">graph provider</param>
        /// <param name="configManager">config manager</param>
        /// <param name="pilotInterceptor">pilot interceptor</param>
        /// <param name="redisCacheIV">Redis Cache for Caching the response</param>
        /// <param name="logger">The instance for <see cref="ILogger{JobApplicationQuery}"/>.</param>
        public JobApplicationQuery(
            ICosmosQueryClientProvider cosmosQueryClientProvider,
            IOptions<CosmosDBConfiguration> cosmosDBConfiguration,
            ILogger<JobApplicationQuery> logger)
        {
            this.cosmosQueryClientProvider = cosmosQueryClientProvider;
            this.logger = logger;
            this.cosmosDBConfiguration = cosmosDBConfiguration?.Value;
        }

        /// <summary>
        /// Gets the Job Application Summary
        /// </s ummary>
        public async Task<List<JobOpeningSummary>> GetActiveJobApplications(string userOID)
        {
            this.logger.LogInformation($"Started {nameof(this.GetActiveJobApplications)} method in {nameof(JobApplicationQuery)}, User OID: {userOID}");

            if(string.IsNullOrWhiteSpace(userOID))
            {
                throw new InvalidOperationException("User OID is null/Empty");
            }

            var client = this.cosmosQueryClientProvider.GetCosmosQueryClient("GTACommon", "GTA");

            List<JobOpeningSummary> jobOpeningSummaries = new List<JobOpeningSummary>();
            List<JobApplication> jobApplications = new List<JobApplication>();

            Expression<Func<JobOpening, bool>> hiringManagerJobs = jo => jo.Status == JobOpeningStatus.Active &&
            (jo.JobOpeningParticipants.Any(jop => jop.OID == userOID && jop.Role != JobParticipantRole.Interviewer && jop.Role != JobParticipantRole.AA));

            // Get top 5 Job Openings for the User, add paging strategy later
            var jobOpenings = await client.GetWithPagination<JobOpening>(hiringManagerJobs, skip: 0, take: 5).ConfigureAwait(false);
            
            if(jobOpenings == null)
            {
                return null;
            }

            //Expression<Func<JobApplication, bool>> expression = ja => ja.JobApplicationID != null && ja.JobApplicationActivities != null;

            //expression = expression.AndAlso(ja => ja.JobApplicationParticipants.Any(jap => jap.OID == userOID
            //&& jap.Role != JobParticipantRole.Interviewer && jap.Role != JobParticipantRole.AA));

            var taskList = new List<Task<IEnumerable<JobApplication>>>();
            jobOpenings?.ToList()?.ForEach((jo) =>
            {
                Expression<Func<JobApplication, bool>> applicationFilter = ja => ja.JobOpening.ExternalJobOpeningID == jo.ExternalJobOpeningID;
                //applicationFilter = applicationFilter.AndAlso(expression);

                //Need to Improve for jobs with more applications
                taskList.Add(client.GetWithPagination<JobApplication>(applicationFilter, skip: 0, take: 50));
            });

            await Task.WhenAll(taskList);

            taskList?.ForEach((task) =>
            {
                if(task.Result?.ToList()?.Any() == true)
                  jobApplications.AddRange(task.Result.ToList());
             });

            jobOpenings?.ToList()?.ForEach((jo) =>
            {
                var applicationsForJobOpening = jobApplications.Where(ja => ja.ExternalJobApplicationID == jo.ExternalJobOpeningID);
                jobOpeningSummaries.Add(new JobOpeningSummary()
                {
                    ExternalJobOpeningID = jo.ExternalJobOpeningID,
                    PositionTitle = jo.PositionTitle,
                    JobOpeningStatus = jo.Status,
                    JobOpeningStatusReason = jo.StatusReason,

                    JobApplications = jobApplications.Where(ja => ja.JobOpening.ExternalJobOpeningID == jo.ExternalJobOpeningID && ja.Status == JobApplicationStatus.Active)?.ToList(),

                    ReviewApplications = jobApplications.Where(ja => ja.JobOpening.ExternalJobOpeningID == jo.ExternalJobOpeningID && ja.CurrentJobOpeningStage == JobStage.Screening)?.ToList(),

                    TotalApplications = jobApplications.Where(ja => ja. JobOpening.ExternalJobOpeningID == jo.ExternalJobOpeningID)?.Count(),                    
                    DispositionedApplications = jobApplications.Where(ja => ja.JobOpening.ExternalJobOpeningID == jo.ExternalJobOpeningID && ja.CurrentJobOpeningStage == JobStage.Dispositioned)?.Count(),
                    InterviewApplications = jobApplications.Where(ja => ja.JobOpening.ExternalJobOpeningID == jo.ExternalJobOpeningID && ja.CurrentJobOpeningStage == JobStage.Interview)?.Count(),
                    AssessmentApplications = jobApplications.Where(ja => ja.JobOpening.ExternalJobOpeningID == jo.ExternalJobOpeningID && ja.CurrentJobOpeningStage == JobStage.Assessment)?.Count(),
                });
            });

            return jobOpeningSummaries;
        }

        public async Task<IList<ScheduleSummary>> GetSchedulesForJobApplications(IList<string> jobApplicationIds, DateTime startDate)
        {
            if (startDate == null || startDate == default(DateTime))
            {
                startDate = DateTime.UtcNow;
            }

            if (!DateTime.TryParse(startDate.ToString(CultureInfo.InvariantCulture), out startDate))
            {
                throw new InvalidOperationException("Invalid date");
            }

            var scheduleClient = this.cosmosQueryClientProvider.GetCosmosQueryClient("GTAIVSchedule", "GTA");

            var jobApplicationSchedules = await scheduleClient.Get<JobApplicationSchedule>(jas => jobApplicationIds.Contains(jas.JobApplicationID) && jas.ScheduleStatus != ScheduleStatus.Delete && jas.StartDateTime >= startDate && jas.StartDateTime < startDate.AddDays(7));

            jobApplicationSchedules = jobApplicationSchedules?.OrderBy(jas => jas.StartDateTime);

            IList<ScheduleSummary> scheduleSummary = new List<ScheduleSummary>();

            jobApplicationSchedules?.ToList().ForEach(jas =>
            {
                var schedule = new ScheduleSummary
                {
                    JobApplicationId = jas.JobApplicationID,
                    ScheduleStartDateTime = jas.StartDateTime,
                };
                scheduleSummary.Add(schedule);
            });

            return scheduleSummary;
        }
    }
}