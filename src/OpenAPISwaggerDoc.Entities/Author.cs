namespace OpenAPISwaggerDoc.Entities;

[Table("Authors")]
public class Author
{
    [Key] public Guid Id { get; set; }

    [Required] [MaxLength(150)] public string FirstName { get; set; }

    [Required] [MaxLength(150)] public string LastName { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}