using Yaref92.MAUISTT.FunctionalTestingGUI.ViewModels;

namespace Yaref92.MAUISTT.FunctionalTestingGUI;

public partial class MainPage : ContentPage
{
#if WINDOWS
    bool _isRecorderInitialized = false;
#endif
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

#if WINDOWS
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_isRecorderInitialized)
        {
            await (BindingContext as MainViewModel)?.InitializeAudioRecorder()!;
            _isRecorderInitialized = true;
        }
    }
#endif
}

