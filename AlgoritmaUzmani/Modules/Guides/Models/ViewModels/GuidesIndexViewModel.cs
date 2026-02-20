using AlgoritmaUzmani.Modules.Guides.Models.Entities;

namespace AlgoritmaUzmani.Modules.Guides.Models.ViewModels;

public class GuidesIndexViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<Guide> FeaturedGuides { get; set; } = new();
    public List<Guide> RecentGuides { get; set; } = new();
    public string Language { get; set; } = "tr";
}





