
namespace EventPro.Services.RemovingDataService.Interface
{
    public interface IRemovingDataService
    {
        Task RemoveFromDirectoriesByPeriod(List<string> Directories, TimeSpan Period);
        Task RemoveFromDirectoriesDBCheckByPeriod(List<string> Directories, TimeSpan Period);
    }
}
