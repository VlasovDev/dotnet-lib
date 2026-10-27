using SQLite;

namespace LibraryApp.Models;

[Table("books")]
public class Book
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(200), NotNull]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Genre { get; set; } = string.Empty;

    public int Year { get; set; }

    [MaxLength(32)]
    public string Isbn { get; set; } = string.Empty;

    /// <summary>Общее количество экземпляров в фонде.</summary>
    public int TotalCopies { get; set; } = 1;

    /// <summary>Сколько экземпляров сейчас доступно читателям.</summary>
    public int AvailableCopies { get; set; } = 1;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public DateTime AddedAt { get; set; } = DateTime.Now;

    [Ignore]
    public bool IsAvailable => AvailableCopies > 0;

    [Ignore]
    public bool CanReturn => AvailableCopies < TotalCopies;

    [Ignore]
    public string AvailabilityText => AvailableCopies <= 0
        ? "Нет в наличии"
        : AvailableCopies == TotalCopies
            ? $"В наличии: {AvailableCopies}"
            : $"В наличии: {AvailableCopies} из {TotalCopies}";

    [Ignore]
    public string Subtitle
    {
        get
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(Author))
                parts.Add(Author);
            if (!string.IsNullOrWhiteSpace(Genre))
                parts.Add(Genre);
            if (Year > 0)
                parts.Add(Year.ToString());

            return string.Join("  •  ", parts);
        }
    }

    [Ignore]
    public string IsbnText => string.IsNullOrWhiteSpace(Isbn) ? string.Empty : $"ISBN {Isbn}";
}
