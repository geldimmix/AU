namespace AlgoritmaUzmani.Modules.Guides;

/// <summary>
/// Guides modülü için yapılandırma
/// Servisler paylaşılan olduğu için (Admin de kullanıyor) burada sadece routing tanımlanır
/// </summary>
public static class GuidesModule
{
    /// <summary>
    /// Guides modülü route'larını yapılandırır
    /// </summary>
    public static IEndpointRouteBuilder MapGuidesModule(this IEndpointRouteBuilder endpoints)
    {
        // Türkçe route'lar
        endpoints.MapControllerRoute(
            name: "guides_index",
            pattern: "rehberler",
            defaults: new { controller = "Guides", action = "Index" });

        endpoints.MapControllerRoute(
            name: "guides_category",
            pattern: "rehberler/{categorySlug}",
            defaults: new { controller = "Guides", action = "Category" });

        endpoints.MapControllerRoute(
            name: "guides_detail",
            pattern: "rehberler/{categorySlug}/{guideSlug}",
            defaults: new { controller = "Guides", action = "Detail" });

        // İngilizce route'lar
        endpoints.MapControllerRoute(
            name: "guides_index_en",
            pattern: "en/guides",
            defaults: new { controller = "Guides", action = "IndexEn" });

        endpoints.MapControllerRoute(
            name: "guides_category_en",
            pattern: "en/guides/{categorySlug}",
            defaults: new { controller = "Guides", action = "CategoryEn" });

        endpoints.MapControllerRoute(
            name: "guides_detail_en",
            pattern: "en/guides/{categorySlug}/{guideSlug}",
            defaults: new { controller = "Guides", action = "DetailEn" });

        return endpoints;
    }
}
