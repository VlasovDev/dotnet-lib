using LibraryApp.Views;

namespace LibraryApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(BookEditPage), typeof(BookEditPage));
    }
}
