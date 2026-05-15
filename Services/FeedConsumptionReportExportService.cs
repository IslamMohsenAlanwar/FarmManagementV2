using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;

namespace FarmManagement.API.Services
{
    public class FeedConsumptionReportExportService
        : IFeedConsumptionReportExportService
    {
        private readonly FarmDbContext _context;

        public FeedConsumptionReportExportService(FarmDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> ExportReportExcelAsync(int cycleId)
        {
            var cycle = await _context.Cycles
                .Include(c => c.DailyRecords)
                .ThenInclude(d => d.FeedConsumptions)
                .FirstOrDefaultAsync(c => c.Id == cycleId);

            if (cycle == null)
                throw new Exception("Cycle not found.");

            var settings = await _context.FeedConsumptionSettings
                .Where(t => t.BreedId == cycle.BreedId)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var sheet = workbook.Worksheets.Add("Feed Report");

            // ================= HEADERS =================

            string[] headers =
            {
                "اليوم",
                "استهلاك العلف المستهدف لكل طائر (جرام)",
                "استهلاك العلف الفعلي لكل طائر (جرام)",
                "نسبة تحقيق استهلاك العلف لكل طائر (%)",
                "تراكمي استهلاك العلف المستهدف لكل طائر (كجم)",
                "تراكمي استهلاك العلف الفعلي لكل طائر (كجم)",
                "نسبة المحقق التراكمي لاستهلاك العلف لكل طائر (%)",
                "كمية العلف المستهدفة للعنبر (طن)",
                "كمية العلف الفعلية للعنبر (طن)",
                "نسبة تحقيق استهلاك العلف للعنبر (%)",
                "تراكمي كمية العلف المستهدفة للعنبر (طن)",
                "تراكمي كمية العلف الفعلية للعنبر (طن)",
                "نسبة المحقق التراكمي لاستهلاك العلف للعنبر (%)"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = sheet.Cell(1, i + 1);

                cell.Value = headers[i];

                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;

                cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;

                cell.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                cell.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;
            }

            sheet.SheetView.FreezeRows(1);

            sheet.Row(1).Height = 25;

            int row = 2;

            // ================= CUMULATIVE =================

            decimal cumulativeTargetPerBirdGram = 0;
            decimal cumulativeActualPerBirdGram = 0;

            decimal cumulativeTargetHouseTon = 0;
            decimal cumulativeActualHouseTon = 0;

            foreach (var record in cycle.DailyRecords.OrderBy(d => d.ChickAge))
            {
                var weekNumber = (record.ChickAge / 7) + 1;

                var targetPerBirdGram = settings
                    .FirstOrDefault(s =>
                        s.WeekStart <= weekNumber &&
                        s.WeekEnd >= weekNumber)
                    ?.TargetPerBirdGram ?? 0;

                // ================= FEED =================

                var actualFeedTon =
                    record.FeedConsumptions.Sum(f => f.Quantity);

                // ================= PER BIRD =================

                var actualPerBirdGram = record.RemainingChicks == 0
                    ? 0
                    : (actualFeedTon * 1_000_000m)
                        / record.RemainingChicks;

                cumulativeTargetPerBirdGram += targetPerBirdGram;

                cumulativeActualPerBirdGram += actualPerBirdGram;

                var achievementPerBird =
                    targetPerBirdGram == 0
                    ? 0
                    : (actualPerBirdGram / targetPerBirdGram) * 100;

                var cumulativeAchievementPerBird =
                    cumulativeTargetPerBirdGram == 0
                    ? 0
                    : (cumulativeActualPerBirdGram
                        / cumulativeTargetPerBirdGram) * 100;

                // ================= HOUSE =================

                var targetHouseTon =
                    (targetPerBirdGram * record.RemainingChicks)
                    / 1_000_000m;

                var actualHouseTon = actualFeedTon;

                cumulativeTargetHouseTon += targetHouseTon;

                cumulativeActualHouseTon += actualHouseTon;

                var achievementHouse =
                    targetHouseTon == 0
                    ? 0
                    : (actualHouseTon / targetHouseTon) * 100;

                var cumulativeAchievementHouse =
                    cumulativeTargetHouseTon == 0
                    ? 0
                    : (cumulativeActualHouseTon
                        / cumulativeTargetHouseTon) * 100;

                // ================= DATA =================

                sheet.Cell(row, 1).Value = record.ChickAge;

                sheet.Cell(row, 2).Value =
                    Math.Round(targetPerBirdGram, 2);

                sheet.Cell(row, 3).Value =
                    Math.Round(actualPerBirdGram, 2);

                sheet.Cell(row, 4).Value =
                    Math.Round(achievementPerBird, 2);

                sheet.Cell(row, 5).Value =
                    Math.Round(cumulativeTargetPerBirdGram / 1000m, 3);

                sheet.Cell(row, 6).Value =
                    Math.Round(cumulativeActualPerBirdGram / 1000m, 3);

                sheet.Cell(row, 7).Value =
                    Math.Round(cumulativeAchievementPerBird, 2);

                sheet.Cell(row, 8).Value =
                    Math.Round(targetHouseTon, 3);

                sheet.Cell(row, 9).Value =
                    Math.Round(actualHouseTon, 3);

                sheet.Cell(row, 10).Value =
                    Math.Round(achievementHouse, 2);

                sheet.Cell(row, 11).Value =
                    Math.Round(cumulativeTargetHouseTon, 3);

                sheet.Cell(row, 12).Value =
                    Math.Round(cumulativeActualHouseTon, 3);

                sheet.Cell(row, 13).Value =
                    Math.Round(cumulativeAchievementHouse, 2);

                row++;
            }

            // ================= STYLE =================

            sheet.Columns().AdjustToContents();

            sheet.RangeUsed().Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            sheet.RangeUsed().Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            sheet.RangeUsed().Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            sheet.RangeUsed().Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        public async Task<byte[]> ExportReportPdfAsync(int cycleId)
        {
            throw new NotImplementedException();
        }
    }
}