using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedClasses.Events.Reports;
using SharedClasses.Models;

namespace APIGateway.Reports
{
    public static class ReportPortionsMerger
    {

        public static IEnumerable<OverallReportPortion> MergePortions(OverallReportPortion[] portions)
        {
            var grouped = portions.GroupBy(p => p.Aggregation + "_" + p.Period);
            foreach (var group in grouped)
            {
                var aggregation = group.First().Aggregation;
                var period = group.First().Period;
                var portion = new OverallReportPortion { Aggregation = aggregation, Period = period };
                switch (aggregation)
                {
                    case Aggregation.Avg:
                        portion.Value = group.Average(p => p.Value);
                        break;
                    case Aggregation.Min:
                        portion.Value = group.Min(p => p.Value);
                        break;
                    case Aggregation.Max:
                        portion.Value = group.Max(p => p.Value);
                        break;
                    case Aggregation.Sum:
                    case Aggregation.Count:
                        portion.Value = group.Sum(v => v.Value);
                        break;
                }
                yield return portion;
            }
        }

        public static IEnumerable<UserReportPortion> MergePortions(UserReportPortion[] portions)
        {
            var grouped = portions.GroupBy(p => p.Element + "_" + p.Period);
            foreach (var group in grouped)
            {
                var element = group.First().Element;
                var period = group.First().Period;
                yield return new UserReportPortion
                {
                    Element = element,
                    Period = period,
                    Debits = group.Sum(p => p.Debits),
                    Incomes = group.Sum(p => p.Incomes),
                };
            }
        }

    }
}