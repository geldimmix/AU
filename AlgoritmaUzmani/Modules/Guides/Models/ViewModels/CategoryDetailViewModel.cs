using AlgoritmaUzmani.Modules.Guides.Models.Entities;

namespace AlgoritmaUzmani.Modules.Guides.Models.ViewModels;

public class CategoryDetailViewModel
{
    public Category Category { get; set; } = null!;
    public List<Guide> Guides { get; set; } = new();
    public List<Category> AllCategories { get; set; } = new();
    public string Language { get; set; } = "tr";
}






