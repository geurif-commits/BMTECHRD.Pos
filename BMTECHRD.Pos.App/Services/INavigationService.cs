using System.Threading.Tasks;

namespace BMTECHRD.Pos.App.Services;

public interface INavigationService
{
    Task GoToLoginAsync();
    Task GoToShellAsync();
}