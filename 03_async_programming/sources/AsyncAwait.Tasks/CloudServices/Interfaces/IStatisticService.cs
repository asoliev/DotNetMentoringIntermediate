namespace CloudServices.Interfaces;

public interface IStatisticService
{
    Task RegisterVisitAsync(string url);

    Task<long> GetVisitsCountAsync(string url);
}