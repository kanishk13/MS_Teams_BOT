namespace MS.GTA.BOTService.BusinessLibrary.Interfaces
{
    using MS.GTA.BOTService.Common.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IJobApplicationManager
    {
        Task<IList<UpcomingInterviews>> GetUpcomingInterviews(string userOid, DateTime startDateTime);
    }
}