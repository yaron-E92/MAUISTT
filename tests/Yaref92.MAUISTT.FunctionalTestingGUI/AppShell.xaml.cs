using Yaref92.MAUISTT.FunctionalTestingGUI.Pages;

namespace Yaref92.MAUISTT.FunctionalTestingGUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(STTPage), typeof(STTPage));
    }
}
