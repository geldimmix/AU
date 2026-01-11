using AlgoritmaUzmani.Models.Entities;

namespace AlgoritmaUzmani.Models.ViewModels.Public;

public class CategoryDetailViewModel
{
    public Category Category { get; set; } = null!;
    public List<Guide> Guides { get; set; } = new();
    public List<Category> AllCategories { get; set; } = new();
    public string Language { get; set; } = "tr";
}

