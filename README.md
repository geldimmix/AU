# Algoritma Uzmanı

Algoritma ve veri yapıları hakkında kapsamlı Türkçe/İngilizce rehberler sunan web sitesi.

🌐 **Website:** [algoritmauzmani.com](https://algoritmauzmani.com)

## Teknolojiler

- **Backend:** ASP.NET Core 8.0 MVC
- **Database:** PostgreSQL
- **Caching:** In-Memory Cache
- **Translation:** DeepInfra API (DeepSeek-V3)
- **Deployment:** GitHub Actions + Linux Server

## Özellikler

- 📚 Kategori bazlı rehber sistemi
- 🌍 Otomatik Türkçe → İngilizce çeviri
- 🔍 SEO optimizasyonu (sitemap, meta tags, hreflang)
- 📊 Ziyaretçi analizi
- 🍪 GDPR uyumlu çerez politikası
- 📱 Mobil uyumlu tasarım
- 🔐 Admin paneli

## Kurulum

### Gereksinimler
- .NET 8.0 SDK
- PostgreSQL 14+

### Yerel Geliştirme

```bash
# Clone
git clone https://github.com/YOUR_USERNAME/algoritmauzmani.git
cd algoritmauzmani/AlgoritmaUzmani

# Veritabanı bağlantısını ayarla
# appsettings.json dosyasını düzenle

# Çalıştır
dotnet run
```

## Deployment

GitHub Actions ile otomatik deploy yapılır. `main` branch'e push yapıldığında:

1. Proje build edilir
2. Publish edilir
3. Sunucuya rsync ile kopyalanır
4. Servis yeniden başlatılır

## Lisans

MIT License



