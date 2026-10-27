using LibraryApp.Data;

namespace LibraryApp;

public partial class App : Application
{
    /// <summary>Единый экземпляр доступа к локальной базе данных.</summary>
    public static BookDatabase Database { get; } = new();

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell())
        {
            Title = "Библиотека"
        };
    }
}
