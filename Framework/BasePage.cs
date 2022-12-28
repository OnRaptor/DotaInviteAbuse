using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBot.Framework
{
	public abstract class BasePage<TViewModel> : BasePage where TViewModel : BaseViewModel
	{
		protected BasePage(TViewModel viewModel) : base(viewModel)
		{
		}

		public new TViewModel BindingContext => (TViewModel)base.BindingContext;
	}

	public abstract class BasePage : ContentPage
	{
		protected BasePage(object? viewModel = null)
		{
			BindingContext = viewModel;
			Padding = 12;

			if (string.IsNullOrWhiteSpace(Title))
			{
				Title = GetType().Name;
			}
		}
	}
}
