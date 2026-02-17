using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Models
{
    public enum WorkerRole
    {
        FarmManager = 1,  // مدير مزرعة
        BarnManager = 2,  // مدير عنبر
        BarnWorker  = 3   // عامل عنبر
    }

    public class Worker
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Phone { get; set; }
        public WorkerRole Role { get; set; }  
            public decimal Salary { get; set; }  
          public int VacationDays { get; set; }
    }
}
