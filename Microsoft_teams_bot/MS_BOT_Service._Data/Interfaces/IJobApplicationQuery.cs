//----------------------------------------------------------------------------
// <copyright file="IJobApplicationQuery.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------

namespace MS.GTA.BOTService.Data.Interfaces
{
    using MS.GTA.BOTService.Common.Entities;
    using MS.GTA.BOTService.Common.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// interface for job application query
    /// </summary>
    public interface IJobApplicationQuery
    {
        /// <summary>
        /// Gets the Job Application Summary
        /// </s ummary>
        Task<List<JobOpeningSummary>> GetActiveJobApplications(string userOID);


        Task<IList<ScheduleSummary>> GetSchedulesForJobApplications(IList<string> jobApplicationIds, DateTime startDate);
    }
}
