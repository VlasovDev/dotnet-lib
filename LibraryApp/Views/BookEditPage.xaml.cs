using LibraryApp.Models;

namespace LibraryApp.Views;

[QueryProperty(nameof(Book), "Book")]
public partial class BookEditPage : ContentPage
{
    private Book _book = new();

    public Book Book
    {
        get => _book;
        set
        {
            _book = value ?? new Book();
            BindingContext = _book;
        }
    }

    public BookEditPage()
    {
        InitializeComponent();
        BindingContext = _book;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var isNew = _book.Id == 0;
        HeaderLabel.Text = isNew ? "Новая книга" : "Редактирование книги";
        Title = isNew ? "Новая книга" : "Редактирование";
        DeleteButton.IsVisible = !isNew;
    }

    private void OnFormSizeChanged(object? sender, EventArgs e)
    {
        if (FormScroll.Width <= 0)
            return;

        var width = Math.Min(880, FormScroll.Width);
        if (Math.Abs(FormContent.WidthRequest - width) > 1)
            FormContent.WidthRequest = width;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(Book.Title))
        {
            ShowError("Укажите название книги.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Book.Author))
        {
            ShowError("Укажите автора книги.");
            return;
        }

        if (Book.TotalCopies < 1)
        {
            ShowError("Количество экземпляров должно быть не меньше 1.");
            return;
        }

        if (Book.AvailableCopies < 0 || Book.AvailableCopies > Book.TotalCopies)
        {
            ShowError("«В наличии» не может быть меньше 0 или больше общего количества экземпляров.");
            return;
        }

        try
        {
            await App.Database.SaveBookAsync(Book);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ShowError($"Не удалось сохранить книгу: {ex.Message}");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (Book.Id == 0)
            return;

        var confirmed = await DisplayAlertAsync(
            "Удалить книгу?",
            $"«{Book.Title}» будет удалена из каталога.",
            "Удалить",
            "Отмена");

        if (!confirmed)
            return;

        await App.Database.DeleteBookAsync(Book);
        await Shell.Current.GoToAsync("..");
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}
