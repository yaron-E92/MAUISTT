using Yaref92.MAUISTT.FunctionalTestingGUI.ViewModels;

namespace Yaref92.MAUISTT.FunctionalTestingGUI.Pages;

public partial class STTPage : ContentPage
{
    public STTPage(STTViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void Editable_TextChanged(object sender, TextChangedEventArgs e)
    {
        (BindingContext as STTViewModel)!.ReplaceEditable(e.NewTextValue);
    }
}
