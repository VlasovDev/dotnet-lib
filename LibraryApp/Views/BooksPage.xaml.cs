using System.Collections.ObjectModel;
using LibraryApp.Models;

namespace LibraryApp.Views;

public partial class BooksPage : ContentPage
{
    private readonly ObservableCollection<Book> _books = new();
    private List<Book> _allBooks = new();

    public BooksPage()
    {
        InitializeComponent();
        BooksView.ItemsSource = _books;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadBooksAsync();
    }

    private async Task LoadBooksAsync()
    {
        try
        {
            _allBooks = await App.Database.GetBooksAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", $"Не удалось загрузить книги: {ex.Message}", "OK");
        }
    }

    private void ApplyFilter()
    {
        var query = SearchBar.Text?.Trim() ?? string.Empty;

        IEnumerable<Book> filtered = _allBooks;
        if (query.Length > 0)
        {
            filtered = _allBooks.Where(b =>
                b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Genre.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Isbn.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        _books.Clear();
        foreach (var book in filtered)
            _books.Add(book);

        TotalLabel.Text = $"Всего книг: {_allBooks.Count}";
        AvailableLabel.Text = $"В наличии: {_allBooks.Sum(b => b.AvailableCopies)} экз.";
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

    private async void OnAddClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(BookEditPage), new Dictionary<string, object>
        {
            ["Book"] = new Book()
        });

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not Book book)
            return;

        await Shell.Current.GoToAsync(nameof(BookEditPage), new Dictionary<string, object>
        {
            ["Book"] = book
        });
    }

    private async void OnCheckoutClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not Book book)
            return;

        if (!await App.Database.CheckoutAsync(book))
        {
            await DisplayAlertAsync("Нет в наличии", $"Все экземпляры книги «{book.Title}» сейчас выданы.", "OK");
            return;
        }

        await LoadBooksAsync();
    }

    private async void OnReturnClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not Book book)
            return;

        if (!await App.Database.ReturnAsync(book))
        {
            await DisplayAlertAsync("Все экземпляры в фонде", $"Все экземпляры книги «{book.Title}» уже находятся в библиотеке.", "OK");
            return;
        }

        await LoadBooksAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not Book book)
            return;

        var confirmed = await DisplayAlertAsync(
            "Удалить книгу?",
            $"«{book.Title}» будет удалена из каталога.",
            "Удалить",
            "Отмена");

        if (!confirmed)
            return;

        await App.Database.DeleteBookAsync(book);
        await LoadBooksAsync();
    }
}
