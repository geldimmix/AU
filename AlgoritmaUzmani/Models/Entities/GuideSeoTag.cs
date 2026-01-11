namespace AlgoritmaUzmani.Models.Entities;

public class GuideSeoTag
{
    public int GuideId { get; set; }
    public int SeoTagId { get; set; }

    public Guide Guide { get; set; } = null!;
    public SeoTag SeoTag { get; set; } = null!;
}

