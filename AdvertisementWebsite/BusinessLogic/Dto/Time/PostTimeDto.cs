namespace BusinessLogic.Dto.Time;

public class PostTimeDto
{
    public int Days { get; set; } = 0;
    public int Weeks { get; set; } = 0;
    public int Months { get; set; } = 0;

    public int ToDays()
    {
        return Months * 30 + Weeks * 7 + Days;
    }

    public PostTimeDto()
    { }

    public PostTimeDto(int timePeriodInDays)
    {
        Months = timePeriodInDays / 30;
        timePeriodInDays %= 30;

        Weeks = timePeriodInDays / 7;
        timePeriodInDays %= 7;

        Days = timePeriodInDays;
    }
}
