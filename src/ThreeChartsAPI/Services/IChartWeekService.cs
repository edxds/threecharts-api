using System;
using System.Collections.Generic;
using ThreeChartsAPI.Models;

namespace ThreeChartsAPI.Services
{
    public interface IChartWeekService
    {
        List<ChartWeek> GetChartWeeksInDateRange(DateTime startDate, DateTime endDate);
    }
}