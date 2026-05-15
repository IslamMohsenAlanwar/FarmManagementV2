namespace FarmManagement.API.Services
{
    public interface IDailyRecordExportService
    {
        Task<byte[]> ExportReportExcelAsync(int cycleId);
        Task<byte[]> ExportReportPdfAsync(int cycleId);
    }
}