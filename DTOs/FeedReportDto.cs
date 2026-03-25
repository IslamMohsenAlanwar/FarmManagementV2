namespace FarmManagement.API.DTOs
{
    public class FeedReportDto
{
    public int DayNumber { get; set; }

    // ===== لكل طائر =====
    public decimal TargetFeedPerBirdGram { get; set; }    // استهلاك مستهدف / طائر
    public decimal ActualFeedPerBirdGram { get; set; }    // استهلاك فعلي / طائر
    public decimal AchievementPerBirdPercent { get; set; } // التحقيق %

    public decimal CumulativeTargetFeedPerBirdKg { get; set; } // تراكمي مستهدف / طائر
    public decimal CumulativeActualFeedPerBirdKg { get; set; } // تراكمي فعلي / طائر
    public decimal CumulativeAchievementPerBirdPercent { get; set; } // تراكمي %

    // ===== للعنبر =====
    public decimal TargetFeedPerHouseTon { get; set; }   // استهلاك مستهدف / العنبر
    public decimal ActualFeedPerHouseTon { get; set; }   // استهلاك فعلي / العنبر
    public decimal AchievementHousePercent { get; set; } // التحقيق %

    public decimal CumulativeTargetFeedHouseTon { get; set; } // تراكمي مستهدف / العنبر
    public decimal CumulativeActualFeedHouseTon { get; set; } // تراكمي فعلي / العنبر
    public decimal CumulativeAchievementHousePercent { get; set; } // تراكمي %
}
}
