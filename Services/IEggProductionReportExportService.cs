namespace FarmManagement.API.Services
{
    public interface IEggProductionReportExportService
    {
        Task<byte[]> ExportReportExcelAsync(int cycleId);
        Task<byte[]> ExportReportPdfAsync(int cycleId);
    }
}