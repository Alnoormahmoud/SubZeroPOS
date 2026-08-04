using System;
using System.Threading.Tasks;
using SubZeroPOS.Core.DTOs;

namespace SubZeroPOS.Core.Interfaces
{
    public interface IReportService
    {
        Task<ReportSummaryDto> GetSummaryAsync(DateTime from, DateTime to);
    }
}
