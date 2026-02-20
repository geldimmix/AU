# Algoritma Uzmanı - Proje Özeti

## 📋 Genel Bilgiler

| Özellik | Değer |
|---------|-------|
| **Proje Adı** | Algoritma Uzmanı |
| **Framework** | ASP.NET Core 8.0 MVC |
| **Veritabanı** | PostgreSQL |
| **Sunucu** | 164.92.214.116 (Linux) |
| **Domain** | algoritmauzmani.com |
| **GitHub** | https://github.com/geldimmix/AU |

---

## 🏗️ Mimari ve Teknolojiler

### Backend
- **ASP.NET Core 8.0 MVC** - Web framework
- **Entity Framework Core 8.0** - ORM
- **Npgsql** - PostgreSQL driver
- **BCrypt.Net** - Şifre hashleme
- **UAParser** - User-Agent parsing (ziyaretçi analizi için)

### Frontend
- **Vanilla CSS** - Custom responsive tasarım
- **Vanilla JavaScript** - LiveSearch ve interaktif özellikler
- **Plus Jakarta Sans** - Font ailesi

### Deployment
- **Nginx** - Reverse proxy
- **Systemd** - Service yönetimi
- **GitHub Actions** - CI/CD pipeline
- **Let's Encrypt** - SSL sertifikası (yapılandırılacak)

---

## 📁 Proje Yapısı

```
AlgoritmaUzmani/
├── Controllers/
│   ├── AdminController.cs      # Admin panel işlemleri
│   ├── GuidesController.cs     # Rehber görüntüleme + Search API
│   ├── HomeController.cs       # Ana sayfa, sitemap, robots.txt
│   └── PageController.cs       # Statik sayfalar
├── Data/
│   ├── ApplicationDbContext.cs # EF Core DbContext
│   └── DbSeeder.cs             # Seed data (kategoriler, admin, statik sayfalar)
├── Helpers/
│   └── SlugHelper.cs           # URL-friendly slug üretici
├── Middleware/
│   └── VisitorTrackingMiddleware.cs # Ziyaretçi takibi
├── Models/
│   ├── Entities/
│   │   ├── Category.cs         # Kategori entity
│   │   ├── Guide.cs            # Rehber entity
│   │   ├── Tag.cs              # Kullanıcı etiketi
│   │   ├── SeoTag.cs           # SEO etiketi
│   │   ├── GuideTag.cs         # Many-to-many junction
│   │   ├── GuideSeoTag.cs      # Many-to-many junction
│   │   ├── RelatedGuide.cs     # İlişkili rehberler
│   │   ├── AdminUser.cs        # Admin kullanıcı
│   │   ├── StaticPage.cs       # Statik sayfalar
│   │   ├── VisitorLog.cs       # Ziyaretçi logları
│   │   └── SiteSetting.cs      # Site ayarları
│   └── ViewModels/
│       ├── Admin/              # Admin panel view modelleri
│       └── Public/             # Public site view modelleri
├── Services/
│   ├── Interfaces/             # Service interface'leri
│   └── [Implementations]       # Service implementasyonları
├── Views/
│   ├── Admin/                  # Admin panel view'ları
│   ├── Guides/                 # Rehber view'ları
│   ├── Home/                   # Ana sayfa view'ları
│   ├── Page/                   # Statik sayfa view'ları
│   └── Shared/                 # Layout ve paylaşılan view'lar
├── wwwroot/
│   └── sitemap.xsl             # Sitemap XSL stylesheet
├── AppData/                    # Yüklenen görseller
├── appsettings.json            # Uygulama ayarları
└── Program.cs                  # Uygulama başlangıç noktası
```

---

## ✨ Özellikler

### 🌐 Public Site

#### Ana Sayfa
- Öne çıkan rehberler
- Son eklenen rehberler
- Kategori sidebar'ı
- Responsive tasarım

#### Rehberler
- Kategorilere göre listeleme
- 2 sütunlu grid görünümü
- Detay sayfası (içerik, etiketler, ilişkili rehberler)
- Breadcrumb navigasyonu
- "Kategoriye dön" butonu

#### Arama (LiveSearch)
- Navbar'da anlık arama kutusu
- 300ms debounce ile API çağrısı
- Kategori ikonu, başlık, özet gösterimi
- Türkçe/İngilizce dil desteği
- `/api/search?q=query&lang=tr` endpoint'i

#### Çok Dilli Destek
- Türkçe (varsayılan) ve İngilizce
- Otomatik çeviri (DeepInfra API)
- hreflang etiketleri
- Dil değiştirme butonu

#### SEO
- Dinamik sitemap.xml (XSL stylesheet ile şık görünüm)
- robots.txt
- Meta description ve keywords
- Canonical URL'ler
- Alternate hreflang
- Schema.org yapılandırması

#### Footer
- Modern gradient tasarım
- Site açıklaması
- Sosyal medya linkleri (X, YouTube, Instagram)
- Hızlı bağlantılar
- Yasal bağlantılar
- Dil değiştirme

#### Cookie Consent
- GDPR uyumlu pop-up
- Türkçe/İngilizce metin
- LocalStorage ile tercih saklama

### 🔐 Admin Panel

#### Giriş
- URL: `/admin/login`
- Varsayılan: `admin` / `Admin123!`
- Cookie tabanlı authentication
- 30 gün oturum süresi

#### Dashboard
- Toplam kategori sayısı
- Toplam rehber sayısı
- Toplam etiket sayısı
- Toplam görüntülenme

#### Kategori Yönetimi
- CRUD işlemleri
- İkon seçimi (emoji)
- Türkçe/İngilizce içerik
- Otomatik slug üretimi
- Otomatik İngilizce çeviri

#### Rehber Yönetimi
- CRUD işlemleri
- Zengin metin editörü
- Görsel yükleme ve konumlandırma
- Kategori seçimi
- Etiket seçimi (çoklu)
- SEO etiket seçimi (çoklu)
- İlişkili rehber seçimi
- Öne çıkan işaretleme
- Aktif/Pasif durumu
- AI SEO önerileri (meta description + keywords)

#### Etiket Yönetimi
- Kullanıcı etiketleri (Tag)
- SEO etiketleri (SeoTag)
- Renk seçimi
- Otomatik İngilizce çeviri

#### Statik Sayfalar
- Hakkında
- Gizlilik Politikası
- Çerez Politikası
- Türkçe/İngilizce içerik

#### Ziyaretçi Analizi
- Toplam/Tekil ziyaretçi sayısı
- Günlük ziyaretçi grafiği (Chart.js)
- Tarayıcı dağılımı
- İşletim sistemi dağılımı
- Cihaz türü dağılımı
- Popüler sayfalar
- Son ziyaretçi listesi

#### Site Ayarları
- Google Analytics ID
- Google Tag Manager ID
- Header Scripts (özel meta etiketleri)
- Footer Scripts (özel izleme kodları)

#### Önbellek Yönetimi
- Tüm önbelleği temizleme butonu

---

## 🗄️ Veritabanı Şeması

### Categories
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | int | Primary key |
| NameTr | string(200) | Türkçe ad |
| NameEn | string(200) | İngilizce ad |
| DescriptionTr | string(500) | Türkçe açıklama |
| DescriptionEn | string(500) | İngilizce açıklama |
| SlugTr | string(200) | Türkçe URL slug |
| SlugEn | string(200) | İngilizce URL slug |
| Icon | string(50) | Emoji ikon |
| SortOrder | int | Sıralama |
| IsActive | bool | Aktif durumu |
| CreatedAt | DateTime | Oluşturulma tarihi |

### Guides
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | int | Primary key |
| TitleTr | string(300) | Türkçe başlık |
| TitleEn | string(300) | İngilizce başlık |
| SummaryTr | string(500) | Türkçe özet |
| SummaryEn | string(500) | İngilizce özet |
| ContentTr | text | Türkçe içerik (HTML) |
| ContentEn | text | İngilizce içerik (HTML) |
| SlugTr | string(300) | Türkçe URL slug |
| SlugEn | string(300) | İngilizce URL slug |
| MetaDescriptionTr | string(160) | Türkçe meta description |
| MetaDescriptionEn | string(160) | İngilizce meta description |
| MetaKeywordsTr | string(500) | Türkçe keywords |
| MetaKeywordsEn | string(500) | İngilizce keywords |
| CategoryId | int | Foreign key |
| FeaturedImage | string(500) | Öne çıkan görsel URL |
| ViewCount | int | Görüntülenme sayısı |
| IsFeatured | bool | Öne çıkan mı |
| IsActive | bool | Aktif durumu |
| CreatedAt | DateTime | Oluşturulma tarihi |
| UpdatedAt | DateTime | Güncellenme tarihi |

### Tags
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | int | Primary key |
| NameTr | string(100) | Türkçe ad |
| NameEn | string(100) | İngilizce ad |
| SlugTr | string(100) | Türkçe slug |
| SlugEn | string(100) | İngilizce slug |
| Color | string(20) | Renk kodu |
| CreatedAt | DateTime | Oluşturulma tarihi |

### SeoTags
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | int | Primary key |
| NameTr | string(100) | Türkçe ad |
| NameEn | string(100) | İngilizce ad |
| CreatedAt | DateTime | Oluşturulma tarihi |

### StaticPages
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | int | Primary key |
| Slug | string(100) | URL slug |
| TitleTr | string(200) | Türkçe başlık |
| TitleEn | string(200) | İngilizce başlık |
| ContentTr | text | Türkçe içerik |
| ContentEn | text | İngilizce içerik |
| MetaDescriptionTr | string(160) | Meta description TR |
| MetaDescriptionEn | string(160) | Meta description EN |
| IsActive | bool | Aktif durumu |
| CreatedAt | DateTime | Oluşturulma tarihi |
| UpdatedAt | DateTime | Güncellenme tarihi |

### VisitorLogs
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | long | Primary key |
| IpAddress | string(45) | IP adresi |
| UserAgent | string(500) | User-Agent |
| Browser | string(100) | Tarayıcı adı |
| BrowserVersion | string(100) | Tarayıcı versiyonu |
| OperatingSystem | string(100) | İşletim sistemi |
| DeviceType | string(50) | Cihaz türü |
| PageUrl | string(500) | Ziyaret edilen URL |
| Referrer | string(1000) | Referrer URL |
| Language | string(10) | Dil |
| SessionId | string(100) | Oturum ID |
| IsNewVisitor | bool | Yeni ziyaretçi mi |
| VisitedAt | DateTime | Ziyaret zamanı |

### SiteSettings
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | int | Primary key |
| Key | string(100) | Ayar anahtarı |
| Value | text | Ayar değeri |
| Description | string(200) | Açıklama |
| Category | string(50) | Kategori |
| IsActive | bool | Aktif durumu |
| CreatedAt | DateTime | Oluşturulma tarihi |
| UpdatedAt | DateTime | Güncellenme tarihi |

### AdminUsers
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | int | Primary key |
| Username | string(100) | Kullanıcı adı |
| PasswordHash | string(200) | BCrypt hash |
| FullName | string(200) | Tam ad |
| Email | string(200) | E-posta |
| IsActive | bool | Aktif durumu |
| LastLoginAt | DateTime | Son giriş |
| CreatedAt | DateTime | Oluşturulma tarihi |

---

## 🚀 Deployment

### Sunucu Bilgileri
- **IP:** 164.92.214.116
- **OS:** Ubuntu/Debian Linux
- **User:** deploy
- **App Path:** /var/www/algoritmauzmani

### Veritabanı
- **Host:** 164.92.214.116
- **Database:** algoritma_uzmani
- **User:** algoritma_user
- **Password:** AlgoritmaUzmani2024

### GitHub Actions Workflow
```yaml
name: Deploy ASP.NET Core to Linux Server

on:
  push:
    branches: [main]
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - Checkout
      - Setup .NET 8
      - Restore & Build & Publish
      - SCP to server
      - SSH: Extract, set permissions, restart service
```

### Secrets Gerekli
- `SERVER_IP` - Sunucu IP adresi
- `SSH_PRIVATE_KEY` - SSH private key

### Systemd Service
```ini
[Unit]
Description=Algoritma Uzmani ASP.NET Core App

[Service]
WorkingDirectory=/var/www/algoritmauzmani
ExecStart=/usr/bin/dotnet AlgoritmaUzmani.dll
Restart=always
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

---

## 🔧 Yapılandırma

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=164.92.214.116;Database=algoritma_uzmani;Username=algoritma_user;Password=AlgoritmaUzmani2024"
  },
  "DeepInfra": {
    "ApiKey": "O4j1EWYs15ZiGAStc8HzPf0T91ZNkb16",
    "BaseUrl": "https://api.deepinfra.com/v1/openai/chat/completions",
    "Model": "deepseek-ai/DeepSeek-V3"
  },
  "AppSettings": {
    "SiteName": "Algoritma Uzmanı",
    "SiteNameEn": "Algorithm Expert",
    "DefaultLanguage": "tr",
    "AppDataPath": "AppData"
  }
}
```

---

## 📱 Sosyal Medya

| Platform | Link |
|----------|------|
| X (Twitter) | https://x.com/algoritmauzmani |
| YouTube | https://www.youtube.com/@AlgoritmaUzman |
| Instagram | https://instagram.com/algoritmauzman |

---

## 📝 Seed Data (Kategoriler)

1. 🔢 Veri Yapıları
2. ⚡ Algoritmalar
3. 🏗️ Yazılım Mimarisi
4. 🎨 Tasarım Kalıpları
5. 🗄️ Veritabanı
6. 🌐 Web Geliştirme
7. 📱 Mobil Geliştirme
8. ☁️ Bulut ve DevOps
9. 🔒 Güvenlik
10. 🧪 Test ve Kalite

---

## 🔜 Yapılacaklar (TODO)

- [ ] Let's Encrypt SSL sertifikası kurulumu
- [ ] Google Analytics entegrasyonu
- [ ] Google Search Console kaydı
- [ ] İçerik ekleme (rehberler)
- [ ] Performance optimizasyonu
- [ ] CDN entegrasyonu (isteğe bağlı)

---

## 📅 Geliştirme Tarihi

**Başlangıç:** Ocak 2026

---

*Bu döküman proje geliştirme sürecinde otomatik olarak oluşturulmuştur.*










