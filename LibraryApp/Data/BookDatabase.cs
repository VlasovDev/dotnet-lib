using LibraryApp.Models;
using SQLite;

namespace LibraryApp.Data;

/// <summary>
/// Простой слой доступа к локальной базе данных SQLite.
/// </summary>
public class BookDatabase
{
    private const string DatabaseFileName = "library.db3";

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _connection;

    public static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
            return _connection;

        await _initLock.WaitAsync();
        try
        {
            if (_connection is null)
            {
                var connection = new SQLiteAsyncConnection(
                    DatabasePath,
                    SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

                await connection.CreateTableAsync<Book>();
                await SeedAsync(connection);

                _connection = connection;
            }
        }
        finally
        {
            _initLock.Release();
        }

        return _connection!;
    }

    public async Task<List<Book>> GetBooksAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<Book>()
            .OrderByDescending(b => b.Id)
            .ToListAsync();
    }

    public async Task<Book?> GetBookAsync(int id)
    {
        var db = await GetConnectionAsync();
        return await db.FindAsync<Book>(id);
    }

    public async Task<int> SaveBookAsync(Book book)
    {
        var db = await GetConnectionAsync();

        book.Title = book.Title?.Trim() ?? string.Empty;
        book.Author = book.Author?.Trim() ?? string.Empty;
        book.Genre = book.Genre?.Trim() ?? string.Empty;
        book.Isbn = book.Isbn?.Trim() ?? string.Empty;
        book.Description = book.Description?.Trim() ?? string.Empty;

        if (book.TotalCopies < 1)
            book.TotalCopies = 1;
        if (book.AvailableCopies < 0)
            book.AvailableCopies = 0;
        if (book.AvailableCopies > book.TotalCopies)
            book.AvailableCopies = book.TotalCopies;

        return book.Id == 0
            ? await db.InsertAsync(book)
            : await db.UpdateAsync(book);
    }

    public async Task<int> DeleteBookAsync(Book book)
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAsync(book);
    }

    /// <summary>Выдать один экземпляр читателю.</summary>
    public async Task<bool> CheckoutAsync(Book book)
    {
        if (book.AvailableCopies <= 0)
            return false;

        book.AvailableCopies--;
        await SaveBookAsync(book);
        return true;
    }

    /// <summary>Вернуть один экземпляр в фонд.</summary>
    public async Task<bool> ReturnAsync(Book book)
    {
        if (book.AvailableCopies >= book.TotalCopies)
            return false;

        book.AvailableCopies++;
        await SaveBookAsync(book);
        return true;
    }

    private static async Task SeedAsync(SQLiteAsyncConnection connection)
    {
        if (await connection.Table<Book>().CountAsync() > 0)
            return;

        var books = new List<Book>
        {
            new()
            {
                Title = "Мастер и Маргарита",
                Author = "Михаил Булгаков",
                Genre = "Роман",
                Year = 1967,
                Isbn = "978-5-389-01686-6",
                TotalCopies = 3,
                AvailableCopies = 2,
                Description = "Мистический роман о добре и зле, любви и трусости, написанный одним из самых загадочных русских писателей XX века."
            },
            new()
            {
                Title = "Преступление и наказание",
                Author = "Фёдор Достоевский",
                Genre = "Роман",
                Year = 1866,
                Isbn = "978-5-04-116347-1",
                TotalCopies = 2,
                AvailableCopies = 2,
                Description = "История бывшего студента Родиона Раскольникова, решившегося на преступление ради проверки собственной теории."
            },
            new()
            {
                Title = "Война и мир",
                Author = "Лев Толстой",
                Genre = "Роман-эпопея",
                Year = 1869,
                Isbn = "978-5-17-090172-2",
                TotalCopies = 1,
                AvailableCopies = 0,
                Description = "Масштабное полотно о жизни русского общества в эпоху наполеоновских войн."
            },
            new()
            {
                Title = "Пикник на обочине",
                Author = "Аркадий и Борис Стругацкие",
                Genre = "Фантастика",
                Year = 1972,
                Isbn = "978-5-17-135483-1",
                TotalCopies = 2,
                AvailableCopies = 1,
                Description = "Повесть о сталкере, который водит людей в Зону — загадочную территорию с внеземными артефактами."
            },
            new()
            {
                Title = "Дюна",
                Author = "Фрэнк Герберт",
                Genre = "Фантастика",
                Year = 1965,
                Isbn = "978-5-17-118784-2",
                TotalCopies = 4,
                AvailableCopies = 3,
                Description = "Классика научной фантастики о пустынной планете Арракис, специи и судьбе Пола Атрейдеса."
            },
            new()
            {
                Title = "Маленький принц",
                Author = "Антуан де Сент-Экзюпери",
                Genre = "Повесть-сказка",
                Year = 1943,
                Isbn = "978-5-699-88772-6",
                TotalCopies = 5,
                AvailableCopies = 5,
                Description = "Философская сказка о дружбе, любви и ответственности за тех, кого мы приручили."
            }
        };

        await connection.InsertAllAsync(books);
    }
}
