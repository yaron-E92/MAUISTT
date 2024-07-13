using Yaref92.MAUISTT.FunctionalTestingGUI.ViewModels;

namespace Yaref92.MAUISTT.FunctionalTestingGUI;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

