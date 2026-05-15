using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Helpers;

namespace FarmManagement.API.Services
{
    public class DailyRecordExportService
        : IDailyRecordExportService
    {
        private readonly FarmDbContext _context;

        public DailyRecordExportService(FarmDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> ExportReportExcelAsync(int cycleId)
        {
            var records = await _context.DailyRecords
                .Include(d => d.Cycle)
                .Include(d => d.FeedConsumptions)
                .Include(d => d.MedicineConsumptions)
                .Where(d => d.CycleId == cycleId)
                .OrderBy(d => d.ChickAge)
                .ToListAsync();

            if (!records.Any())
                throw new Exception("No records found.");

            var breedId = records.First().Cycle.BreedId;

            var targetSettings = await _context.TargetMortalitySettings
                .Where(t => t.BreedId == breedId)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var sheet = workbook.Worksheets.Add("Daily Records");

            // ================= HEADERS =================

            string[] headers =
            {
                "التاريخ",
                "اليوم",
                "عمر الفراخ",
                "النافق اليومي",
                "النافق التراكمي",
                "العدد المتبقي",
                "نسبة النافق %",
                "نسبة النافق المتوقعة %",
                "استهلاك وزن العلف (طن)",
                "سعر استهلاك العلف (جنيه)",
                "استهلاك الأدوية",
                "سعر استهلاك الأدوية (جنيه)"
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

            // ================= DATA =================

            foreach (var d in records)
            {
                var egyptDate = TimeZoneHelper.ToEgyptTime(d.Date);

                var week = (d.ChickAge / 7) + 1;

                var targetMortality = targetSettings
                    .Where(t =>
                        t.WeekStart <= week &&
                        t.WeekEnd >= week)
                    .Select(t => t.ExpectedMortalityRate)
                    .FirstOrDefault();

                var mortalityRate = d.Cycle.ChickCount == 0
                    ? 0
                    : Math.Round(
                        (decimal)d.DeadCumulative
                        / d.Cycle.ChickCount * 100,
                        2);

                var totalFeedQuantity =
                    d.FeedConsumptions.Sum(f => f.Quantity);

                var totalFeedCost =
                    d.FeedConsumptions.Sum(f => f.Cost);

                var totalMedicineQuantity =
                    d.MedicineConsumptions.Sum(m => m.Quantity);

                var totalMedicineCost =
                    d.MedicineConsumptions.Sum(m => m.Cost);

                sheet.Cell(row, 1).Value =
                    egyptDate.ToString("yyyy-MM-dd");

                sheet.Cell(row, 2).Value =
                    TimeZoneHelper.GetArabicDayName(egyptDate);

                sheet.Cell(row, 3).Value = d.ChickAge;

                sheet.Cell(row, 4).Value = d.DeadCount;

                sheet.Cell(row, 5).Value = d.DeadCumulative;

                sheet.Cell(row, 6).Value = d.RemainingChicks;

                sheet.Cell(row, 7).Value = mortalityRate;

                sheet.Cell(row, 8).Value = targetMortality;

                sheet.Cell(row, 9).Value =
                    Math.Round(totalFeedQuantity, 3);

                sheet.Cell(row, 10).Value =
                    Math.Round(totalFeedCost, 2);

                sheet.Cell(row, 11).Value =
                    Math.Round(totalMedicineQuantity, 3);

                sheet.Cell(row, 12).Value =
                    Math.Round(totalMedicineCost, 2);

                row++;
            }

            // ================= STYLE =================

            sheet.Columns().AdjustToContents();

            sheet.RangeUsed().Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            sheet.RangeUsed().Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            sheet.RangeUsed().Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            sheet.RangeUsed().Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

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