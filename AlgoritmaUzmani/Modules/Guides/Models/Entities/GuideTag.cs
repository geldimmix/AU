namespace AlgoritmaUzmani.Modules.Guides.Models.Entities;

public class GuideTag
{
    public int GuideId { get; set; }
    public int TagId { get; set; }

    public Guide Guide { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}





