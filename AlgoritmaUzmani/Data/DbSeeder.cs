using AlgoritmaUzmani.Models.Entities;
using AlgoritmaUzmani.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AlgoritmaUzmani.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Seed Categories - check each one individually
        var defaultCategories = new List<(string NameTr, string NameEn, string DescTr, string DescEn, string Icon, int Order)>
        {
            ("Veri Yapıları", "Data Structures", "Temel ve ileri düzey veri yapıları hakkında rehberler", "Guides about fundamental and advanced data structures", "🗂️", 1),
            ("Algoritmalar", "Algorithms", "Sıralama, arama ve optimizasyon algoritmaları", "Sorting, searching and optimization algorithms", "⚡", 2),
            ("API ve Web Geliştirme", "API and Web Development", "REST, GraphQL ve web teknolojileri", "REST, GraphQL and web technologies", "🌐", 3),
            ("Veri Tabanı", "Database", "SQL, NoSQL ve veritabanı optimizasyonu", "SQL, NoSQL and database optimization", "💾", 4),
            ("Yazılım Mimarileri", "Software Architecture", "Mimari desenler ve sistem tasarımı", "Architectural patterns and system design", "🏗️", 5),
            ("Deep Learning", "Deep Learning", "Derin öğrenme ve sinir ağları", "Deep learning and neural networks", "🧠", 6),
            ("Machine Learning", "Machine Learning", "Makine öğrenmesi algoritmaları ve uygulamaları", "Machine learning algorithms and applications", "🤖", 7),
            ("Caching", "Caching", "Önbellekleme stratejileri ve performans optimizasyonu", "Caching strategies and performance optimization", "⚡", 8),
            ("Nasıl Çalışır?", "How it Works?", "Sistemlerin ve teknolojilerin çalışma prensipleri", "Working principles of systems and technologies", "❓", 9),
            ("DevOps", "DevOps", "CI/CD, konteynerizasyon ve altyapı yönetimi", "CI/CD, containerization and infrastructure management", "🔧", 10)
        };

        foreach (var cat in defaultCategories)
        {
            var slugTr = SlugHelper.GenerateSlug(cat.NameTr);
            var exists = await context.Categories.AnyAsync(c => c.SlugTr == slugTr);
            
            if (!exists)
            {
                context.Categories.Add(new Category
                {
                    NameTr = cat.NameTr,
                    NameEn = cat.NameEn,
                    SlugTr = slugTr,
                    SlugEn = SlugHelper.GenerateSlug(cat.NameEn),
                    DescriptionTr = cat.DescTr,
                    DescriptionEn = cat.DescEn,
                    Icon = cat.Icon,
                    DisplayOrder = cat.Order,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Seed Admin User
        if (!await context.AdminUsers.AnyAsync())
        {
            var admin = new AdminUser
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                DisplayName = "Administrator",
                IsActive = true
            };

            await context.AdminUsers.AddAsync(admin);
        }

        // Seed Static Pages
        var staticPages = new List<(string Slug, string TitleTr, string TitleEn, string ContentTr, string ContentEn, string MetaTr, string MetaEn)>
        {
            ("hakkinda", "Hakkımızda", "About Us", 
                @"<h2>Algoritma Uzmanı Hakkında</h2>
<p>Algoritma Uzmanı, yazılım geliştiricilere veri yapıları, algoritmalar ve yazılım mimarisi konularında kapsamlı rehberler sunan bir platformdur.</p>
<h3>Misyonumuz</h3>
<p>Karmaşık teknik konuları anlaşılır ve uygulanabilir şekilde sunarak yazılım geliştirme topluluğuna katkıda bulunmak.</p>
<h3>İletişim</h3>
<p>Sorularınız için bizimle iletişime geçebilirsiniz.</p>",
                @"<h2>About Algorithm Expert</h2>
<p>Algorithm Expert is a platform that provides comprehensive guides on data structures, algorithms, and software architecture for software developers.</p>
<h3>Our Mission</h3>
<p>To contribute to the software development community by presenting complex technical topics in an understandable and applicable way.</p>
<h3>Contact</h3>
<p>Feel free to contact us for any questions.</p>",
                "Algoritma Uzmanı hakkında bilgi edinin. Veri yapıları, algoritmalar ve yazılım mimarisi konularında kapsamlı rehberler.",
                "Learn about Algorithm Expert. Comprehensive guides on data structures, algorithms, and software architecture."),

            ("gizlilik", "Gizlilik Politikası", "Privacy Policy",
                @"<h2>Gizlilik Politikası</h2>
<p>Son güncelleme: " + DateTime.UtcNow.ToString("dd.MM.yyyy") + @"</p>
<h3>Toplanan Bilgiler</h3>
<p>Sitemizi ziyaret ettiğinizde, tarayıcınız tarafından gönderilen bazı bilgileri otomatik olarak topluyoruz. Bu bilgiler IP adresi, tarayıcı türü, ziyaret edilen sayfalar ve ziyaret süresini içerebilir.</p>
<h3>Çerezler</h3>
<p>Sitemizde kullanıcı deneyimini iyileştirmek için çerezler kullanılmaktadır. Çerezler hakkında daha fazla bilgi için Çerez Politikamızı inceleyebilirsiniz.</p>
<h3>Bilgi Güvenliği</h3>
<p>Kişisel bilgilerinizin güvenliğini sağlamak için uygun teknik ve organizasyonel önlemler alıyoruz.</p>
<h3>Üçüncü Taraf Hizmetleri</h3>
<p>Google Analytics gibi üçüncü taraf analiz hizmetleri kullanabiliriz. Bu hizmetlerin kendi gizlilik politikaları vardır.</p>
<h3>Haklarınız</h3>
<p>GDPR ve KVKK kapsamında kişisel verilerinize erişme, düzeltme ve silme hakkına sahipsiniz.</p>",
                @"<h2>Privacy Policy</h2>
<p>Last updated: " + DateTime.UtcNow.ToString("MM/dd/yyyy") + @"</p>
<h3>Information We Collect</h3>
<p>When you visit our site, we automatically collect certain information sent by your browser. This may include IP address, browser type, pages visited, and duration of visit.</p>
<h3>Cookies</h3>
<p>We use cookies to improve user experience on our site. For more information about cookies, please review our Cookie Policy.</p>
<h3>Information Security</h3>
<p>We take appropriate technical and organizational measures to ensure the security of your personal information.</p>
<h3>Third-Party Services</h3>
<p>We may use third-party analytics services such as Google Analytics. These services have their own privacy policies.</p>
<h3>Your Rights</h3>
<p>Under GDPR, you have the right to access, correct, and delete your personal data.</p>",
                "Algoritma Uzmanı gizlilik politikası. Kişisel verilerinizin nasıl korunduğunu öğrenin.",
                "Algorithm Expert privacy policy. Learn how your personal data is protected."),

            ("cerez-politikasi", "Çerez Politikası", "Cookie Policy",
                @"<h2>Çerez Politikası</h2>
<p>Son güncelleme: " + DateTime.UtcNow.ToString("dd.MM.yyyy") + @"</p>
<h3>Çerez Nedir?</h3>
<p>Çerezler, web sitelerinin bilgisayarınızda veya mobil cihazınızda sakladığı küçük metin dosyalarıdır.</p>
<h3>Kullandığımız Çerez Türleri</h3>
<h4>Zorunlu Çerezler</h4>
<p>Bu çerezler sitenin düzgün çalışması için gereklidir. Dil tercihi ve oturum bilgileri bu kategoridedir.</p>
<h4>Analitik Çerezler</h4>
<p>Ziyaretçilerin siteyi nasıl kullandığını anlamamıza yardımcı olur. Google Analytics bu amaçla kullanılmaktadır.</p>
<h3>Çerezleri Yönetme</h3>
<p>Tarayıcı ayarlarınızdan çerezleri devre dışı bırakabilirsiniz. Ancak bu, site deneyiminizi etkileyebilir.</p>
<h3>İletişim</h3>
<p>Çerez politikamız hakkında sorularınız için bizimle iletişime geçebilirsiniz.</p>",
                @"<h2>Cookie Policy</h2>
<p>Last updated: " + DateTime.UtcNow.ToString("MM/dd/yyyy") + @"</p>
<h3>What Are Cookies?</h3>
<p>Cookies are small text files that websites store on your computer or mobile device.</p>
<h3>Types of Cookies We Use</h3>
<h4>Essential Cookies</h4>
<p>These cookies are necessary for the site to function properly. Language preferences and session information fall into this category.</p>
<h4>Analytics Cookies</h4>
<p>These help us understand how visitors use the site. Google Analytics is used for this purpose.</p>
<h3>Managing Cookies</h3>
<p>You can disable cookies from your browser settings. However, this may affect your site experience.</p>
<h3>Contact</h3>
<p>For questions about our cookie policy, please contact us.</p>",
                "Algoritma Uzmanı çerez politikası. Çerezlerin nasıl kullanıldığını öğrenin.",
                "Algorithm Expert cookie policy. Learn how cookies are used.")
        };

        foreach (var page in staticPages)
        {
            var exists = await context.StaticPages.AnyAsync(p => p.Slug == page.Slug);
            if (!exists)
            {
                context.StaticPages.Add(new StaticPage
                {
                    Slug = page.Slug,
                    TitleTr = page.TitleTr,
                    TitleEn = page.TitleEn,
                    ContentTr = page.ContentTr,
                    ContentEn = page.ContentEn,
                    MetaDescriptionTr = page.MetaTr,
                    MetaDescriptionEn = page.MetaEn,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
