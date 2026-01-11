using AlgoritmaUzmani.Models.Entities;

namespace AlgoritmaUzmani.Models.ViewModels.Public;

public class HomeViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<Guide> FeaturedGuides { get; set; } = new();
    public List<Guide> RecentGuides { get; set; } = new();
    public string Language { get; set; } = "tr";
}

