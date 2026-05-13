namespace OnlineExamPortal.API.DTOs;

public class LeaderboardDto
{
    public string UserName { get; set; } = string.Empty;
    public double TotalScore { get; set; }
    public int SolvedExams { get; set; }
}

public class StatisticsDto
{
    public GeneralStatisticsDto General { get; set; } = new();
    public ScoreGroupDto ScoreGroup { get; set; } = new();
    public List<ExamBasedStatisticsDto> ExamBased { get; set; } = new();
}

public class GeneralStatisticsDto
{
    public int TotalExams { get; set; }
    public int TotalUsers { get; set; }
    public int TotalParticipation { get; set; }
    public double AverageScore { get; set; }
}

public class ScoreGroupDto
{
    public int Successful { get; set; }
    public int Average { get; set; }
    public int Unsuccessful { get; set; }
}

public class ExamBasedStatisticsDto
{
    public string ExamName { get; set; } = string.Empty;
    public int ParticipationCount { get; set; }
    public double AverageScore { get; set; }
}
