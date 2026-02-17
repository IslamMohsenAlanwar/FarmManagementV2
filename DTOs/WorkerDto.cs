namespace FarmManagement.API.DTOs
{
    public class WorkerDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Phone { get; set; }
        public required string Role { get; set; } 
          public decimal Salary { get; set; }
          public int VacationDays { get; set; }
    }

    public class CreateWorkerDto
    {
        public required string Name { get; set; }
        public string? Phone { get; set; }
        public required int Role { get; set; } 
          public required decimal Salary { get; set; } 
          public required int VacationDays { get; set; }
    }

    public class UpdateWorkerDto
    {
        public required string Name { get; set; }
        public string? Phone { get; set; }
        public required int Role { get; set; }
          public required decimal Salary { get; set; }   
          public required int VacationDays { get; set; } 
    }

public class CreateVacationDto
{
    public required int WorkerId { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
}

public class CreateAdvanceDto
{
    public required int WorkerId { get; set; }
    public required decimal Amount { get; set; }
    public required DateTime Date { get; set; }
}


}
