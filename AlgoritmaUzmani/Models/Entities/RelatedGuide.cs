namespace AlgoritmaUzmani.Models.Entities;

public class RelatedGuide
{
    public int GuideId { get; set; }
    public int RelatedGuideId { get; set; }

    public int DisplayOrder { get; set; }

    public Guide Guide { get; set; } = null!;
    public Guide Related { get; set; } = null!;
}

