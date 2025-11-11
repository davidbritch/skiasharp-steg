using ImageSteg.ViewModels;

namespace ImageSteg.Views;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
