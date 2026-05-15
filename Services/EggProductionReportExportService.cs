using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;

namespace FarmManagement.API.Services
{
    public class EggProductionReportExportService
        : IEggProductionReportExportService
    {
        private readonly FarmDbContext _context;

        public EggProductionReportExportService(FarmDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // EXCEL REPORT
        // =====================================================

        public async Task<byte[]> ExportReportExcelAsync(int cycleId)
        {
            var cycle = await _context.Cycles
                .Include(c => c.DailyRecords)
                .Include(c => c.EggProductionRecords)
                    .ThenInclude(e => e.Details)
                .FirstOrDefaultAsync(c => c.Id == cycleId);

            if (cycle == null)
                throw new Exception("Cycle not found");

            var settings = await _context.EggProductionSettings
                .Where(x => x.BreedId == cycle.BreedId)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var sheet = workbook.Worksheets.Add("Egg Report");

            // ================= HEADERS =================

            string[] headers =
            {
                "اليوم",
                "الانتاج الفعلي كسر (طبق)",
                "الانتاج الفعلي دبل (طبق)",
                "الانتاج الفعلي سليم (طبق)",
                "الانتاج الفعلي فرزة (طبق)",
                "اجمالي الإنتاج الفعلي (طبق)",
                "الانتاج المستهدف (طبق)",
                "الانتاج الفعلي%",
                "الانتاج المستهدف %",
                "المحقق %",
                "متوسط انتاج البيض المستهدف/طائر (بيضة)",
                "متوسط انتاج البيض الفعلي/طائر (بيضة)",
                "المحقق %",
                "تراكمي الانتاج الفعلي (طبق)",
                "تراكمي الانتاج المستهدف (طبق)",
                "المحقق التراكمي %"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = sheet.Cell(1, i + 1);

                cell.Value = headers[i];

                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;

                cell.Style.Fill.BackgroundColor =
                    XLColor.DarkBlue;

                cell.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                cell.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;
            }

            sheet.SheetView.FreezeRows(1);

            sheet.Row(1).Height = 25;

            int row = 2;

            decimal cumulativeActual = 0;
            decimal cumulativeTarget = 0;

            foreach (var day in cycle.DailyRecords.OrderBy(x => x.ChickAge))
            {
                var week =
                    (int)Math.Ceiling(day.ChickAge / 7.0);

                // ================= ALL RECORDS OF SAME DAY =================

                var eggRecords = cycle.EggProductionRecords
                    .Where(e => e.Date.Date == day.Date.Date)
                    .ToList();

                decimal broken = 0;
                decimal dbl = 0;
                decimal normal = 0;
                decimal farza = 0;

                if (eggRecords.Any())
                {
                    var allDetails =
                        eggRecords.SelectMany(e => e.Details);

                    broken = allDetails
                        .Where(x =>
                            x.EggQuality == EggQualityType.Broken)
                        .Sum(x => x.CartonsCount);

                    dbl = allDetails
                        .Where(x =>
                            x.EggQuality == EggQualityType.Double)
                        .Sum(x => x.CartonsCount);

                    normal = allDetails
                        .Where(x =>
                            x.EggQuality == EggQualityType.Normal)
                        .Sum(x => x.CartonsCount);

                    farza = allDetails
                        .Where(x =>
                            x.EggQuality == EggQualityType.Farza)
                        .Sum(x => x.CartonsCount);
                }

                // ================= FIXED TOTAL =================

                var actualTotal =
                    broken + dbl + normal + farza;

                var setting = settings.FirstOrDefault(s =>
                    s.WeekStart <= week &&
                    s.WeekEnd >= week);

                var targetPercent =
                    setting?.TargetProductionPercent ?? 0m;

                var liveBirds = day.RemainingChicks;

                var targetPerBird =
                    targetPercent / 100m;

                var targetEggs =
                    liveBirds * targetPerBird;

                var targetCartons =
                    targetEggs / 30m;

                var actualEggs =
                    actualTotal * 30m;

                var actualPercent = liveBirds == 0
                    ? 0
                    : (actualEggs / liveBirds) * 100m;

                var achievementPercent =
                    targetPercent == 0
                    ? 0
                    : (actualPercent / targetPercent) * 100m;

                var actualPerBird = liveBirds == 0
                    ? 0
                    : actualEggs / liveBirds;

                var achievementBird =
                    targetPerBird == 0
                    ? 0
                    : (actualPerBird / targetPerBird) * 100m;

                cumulativeActual += actualTotal;

                cumulativeTarget += targetCartons;

                var cumulativeAchievement =
                    cumulativeTarget == 0
                    ? 0
                    : (cumulativeActual / cumulativeTarget) * 100m;

                // ================= DATA =================

                sheet.Cell(row, 1).Value = day.ChickAge;
                sheet.Cell(row, 2).Value = broken;
                sheet.Cell(row, 3).Value = dbl;
                sheet.Cell(row, 4).Value = normal;
                sheet.Cell(row, 5).Value = farza;
                sheet.Cell(row, 6).Value = actualTotal;
                sheet.Cell(row, 7).Value = Math.Round(targetCartons, 2);
                sheet.Cell(row, 8).Value = Math.Round(actualPercent, 2);
                sheet.Cell(row, 9).Value = Math.Round(targetPercent, 2);
                sheet.Cell(row, 10).Value = Math.Round(achievementPercent, 2);
                sheet.Cell(row, 11).Value = Math.Round(targetPerBird, 4);
                sheet.Cell(row, 12).Value = Math.Round(actualPerBird, 4);
                sheet.Cell(row, 13).Value = Math.Round(achievementBird, 2);
                sheet.Cell(row, 14).Value = Math.Round(cumulativeActual, 2);
                sheet.Cell(row, 15).Value = Math.Round(cumulativeTarget, 2);
                sheet.Cell(row, 16).Value = Math.Round(cumulativeAchievement, 2);

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