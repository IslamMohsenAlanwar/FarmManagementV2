namespace FarmManagement.API.Services
{
    public interface IFeedConsumptionReportExportService
    {
        Task<byte[]> ExportReportExcelAsync(int cycleId);
        Task<byte[]> ExportReportPdfAsync(int cycleId);
    }
}