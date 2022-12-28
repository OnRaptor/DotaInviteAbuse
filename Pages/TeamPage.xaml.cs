using SteamBot.Framework;
using SteamBot.ViewModels;

namespace SteamBot.Pages;

public partial class TeamPage : BasePage<TeamViewModel>
{
	public TeamPage(TeamViewModel viewModel) : base(viewModel)
	{
		InitializeComponent();
	}
}