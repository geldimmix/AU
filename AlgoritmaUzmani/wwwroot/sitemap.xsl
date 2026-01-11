<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:sitemap="http://www.sitemaps.org/schemas/sitemap/0.9"
    xmlns:xhtml="http://www.w3.org/1999/xhtml">
<xsl:output method="html" version="1.0" encoding="UTF-8" indent="yes"/>

<xsl:template match="/">
<html xmlns="http://www.w3.org/1999/xhtml" lang="tr">
<head>
    <title>XML Sitemap - Algoritma Uzmanı</title>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <meta name="robots" content="noindex, follow" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="" />
    <link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700&amp;display=swap" rel="stylesheet" />
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: 'Plus Jakarta Sans', -apple-system, BlinkMacSystemFont, sans-serif;
            background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
            min-height: 100vh;
            color: #1e293b;
            line-height: 1.6;
        }
        
        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 2rem 1.5rem;
        }
        
        .header {
            background: linear-gradient(135deg, #0066ff 0%, #0052cc 100%);
            color: white;
            padding: 3rem 0;
            margin-bottom: 2rem;
            box-shadow: 0 4px 20px rgba(0, 102, 255, 0.2);
        }
        
        .header-content {
            max-width: 1200px;
            margin: 0 auto;
            padding: 0 1.5rem;
        }
        
        .header h1 {
            font-size: 2.5rem;
            font-weight: 700;
            margin-bottom: 0.5rem;
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }
        
        .header-icon {
            font-size: 2rem;
        }
        
        .header p {
            opacity: 0.9;
            font-size: 1.1rem;
        }
        
        .stats {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 1.25rem;
            margin-bottom: 2rem;
        }
        
        .stat-card {
            background: white;
            border-radius: 12px;
            padding: 1.5rem;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
            border: 1px solid #e2e8f0;
            text-align: center;
        }
        
        .stat-number {
            font-size: 2.5rem;
            font-weight: 700;
            color: #0066ff;
            line-height: 1;
        }
        
        .stat-label {
            color: #64748b;
            font-size: 0.9rem;
            margin-top: 0.5rem;
        }
        
        .info-box {
            background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
            border: 1px solid #93c5fd;
            border-radius: 12px;
            padding: 1.25rem 1.5rem;
            margin-bottom: 2rem;
            display: flex;
            align-items: flex-start;
            gap: 1rem;
        }
        
        .info-icon {
            font-size: 1.5rem;
            flex-shrink: 0;
        }
        
        .info-text {
            font-size: 0.95rem;
            color: #1e40af;
        }
        
        .info-text strong {
            display: block;
            margin-bottom: 0.25rem;
        }
        
        .table-wrapper {
            background: white;
            border-radius: 12px;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
            border: 1px solid #e2e8f0;
            overflow: hidden;
        }
        
        .table-header {
            background: #f8fafc;
            padding: 1.25rem 1.5rem;
            border-bottom: 1px solid #e2e8f0;
            display: flex;
            justify-content: space-between;
            align-items: center;
            flex-wrap: wrap;
            gap: 1rem;
        }
        
        .table-title {
            font-weight: 600;
            font-size: 1.1rem;
            color: #1e293b;
        }
        
        .search-box {
            position: relative;
        }
        
        .search-box input {
            padding: 0.5rem 1rem 0.5rem 2.5rem;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            font-size: 0.9rem;
            width: 250px;
            font-family: inherit;
        }
        
        .search-box input:focus {
            outline: none;
            border-color: #0066ff;
            box-shadow: 0 0 0 3px rgba(0, 102, 255, 0.1);
        }
        
        .search-icon {
            position: absolute;
            left: 0.75rem;
            top: 50%;
            transform: translateY(-50%);
            color: #94a3b8;
        }
        
        table {
            width: 100%;
            border-collapse: collapse;
        }
        
        th {
            background: #f1f5f9;
            padding: 1rem 1.5rem;
            text-align: left;
            font-weight: 600;
            font-size: 0.8rem;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            color: #64748b;
            border-bottom: 1px solid #e2e8f0;
        }
        
        td {
            padding: 1rem 1.5rem;
            border-bottom: 1px solid #f1f5f9;
            font-size: 0.9rem;
        }
        
        tr:hover {
            background: #f8fafc;
        }
        
        tr:last-child td {
            border-bottom: none;
        }
        
        .url-cell {
            max-width: 500px;
        }
        
        .url-link {
            color: #0066ff;
            text-decoration: none;
            word-break: break-all;
            transition: color 0.15s;
        }
        
        .url-link:hover {
            color: #0052cc;
            text-decoration: underline;
        }
        
        .badge {
            display: inline-block;
            padding: 0.25rem 0.625rem;
            border-radius: 9999px;
            font-size: 0.75rem;
            font-weight: 500;
        }
        
        .badge-high {
            background: #dcfce7;
            color: #166534;
        }
        
        .badge-medium {
            background: #fef3c7;
            color: #92400e;
        }
        
        .badge-low {
            background: #f1f5f9;
            color: #64748b;
        }
        
        .freq-daily { color: #059669; }
        .freq-weekly { color: #d97706; }
        .freq-monthly { color: #6366f1; }
        
        .lang-badges {
            display: flex;
            gap: 0.375rem;
        }
        
        .lang-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.25rem;
            padding: 0.25rem 0.5rem;
            background: #f1f5f9;
            border-radius: 4px;
            font-size: 0.75rem;
            color: #64748b;
            text-decoration: none;
            transition: all 0.15s;
        }
        
        .lang-badge:hover {
            background: #e2e8f0;
            color: #475569;
        }
        
        .footer {
            text-align: center;
            padding: 2rem;
            color: #64748b;
            font-size: 0.875rem;
        }
        
        .footer a {
            color: #0066ff;
            text-decoration: none;
        }
        
        .footer a:hover {
            text-decoration: underline;
        }
        
        @media (max-width: 768px) {
            .header h1 {
                font-size: 1.75rem;
            }
            
            .stats {
                grid-template-columns: repeat(2, 1fr);
            }
            
            .table-header {
                flex-direction: column;
                align-items: stretch;
            }
            
            .search-box input {
                width: 100%;
            }
            
            th, td {
                padding: 0.75rem 1rem;
            }
            
            .url-cell {
                max-width: 200px;
            }
            
            .hide-mobile {
                display: none;
            }
        }
    </style>
</head>
<body>
    <header class="header">
        <div class="header-content">
            <h1><span class="header-icon">🗺️</span> XML Sitemap</h1>
            <p>Algoritma Uzmanı - Site Haritası</p>
        </div>
    </header>
    
    <div class="container">
        <div class="stats">
            <div class="stat-card">
                <div class="stat-number"><xsl:value-of select="count(sitemap:urlset/sitemap:url)"/></div>
                <div class="stat-label">Toplam URL</div>
            </div>
            <div class="stat-card">
                <div class="stat-number"><xsl:value-of select="count(sitemap:urlset/sitemap:url[sitemap:priority='1.0'])"/></div>
                <div class="stat-label">Yüksek Öncelik</div>
            </div>
            <div class="stat-card">
                <div class="stat-number"><xsl:value-of select="count(sitemap:urlset/sitemap:url[sitemap:changefreq='daily'])"/></div>
                <div class="stat-label">Günlük Güncellenen</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">2</div>
                <div class="stat-label">Desteklenen Dil</div>
            </div>
        </div>
        
        <div class="info-box">
            <span class="info-icon">ℹ️</span>
            <div class="info-text">
                <strong>Bu bir XML Sitemap dosyasıdır</strong>
                Bu sitemap, arama motorlarının siteyi daha verimli taramasına yardımcı olmak için oluşturulmuştur. 
                Her URL için öncelik, güncelleme sıklığı ve çok dilli alternatifler tanımlanmıştır.
            </div>
        </div>
        
        <div class="table-wrapper">
            <div class="table-header">
                <span class="table-title">📄 URL Listesi</span>
                <div class="search-box">
                    <span class="search-icon">🔍</span>
                    <input type="text" id="searchInput" placeholder="URL ara..." onkeyup="filterTable()" />
                </div>
            </div>
            <table id="sitemapTable">
                <thead>
                    <tr>
                        <th>#</th>
                        <th>URL</th>
                        <th class="hide-mobile">Diller</th>
                        <th>Öncelik</th>
                        <th class="hide-mobile">Sıklık</th>
                        <th class="hide-mobile">Son Güncelleme</th>
                    </tr>
                </thead>
                <tbody>
                    <xsl:for-each select="sitemap:urlset/sitemap:url">
                        <xsl:sort select="sitemap:priority" order="descending"/>
                        <tr>
                            <td style="color: #94a3b8; font-size: 0.85rem;"><xsl:value-of select="position()"/></td>
                            <td class="url-cell">
                                <a href="{sitemap:loc}" class="url-link" target="_blank">
                                    <xsl:value-of select="sitemap:loc"/>
                                </a>
                            </td>
                            <td class="hide-mobile">
                                <div class="lang-badges">
                                    <xsl:for-each select="xhtml:link[@rel='alternate']">
                                        <a href="{@href}" class="lang-badge" target="_blank">
                                            <xsl:choose>
                                                <xsl:when test="@hreflang='tr'">🇹🇷 TR</xsl:when>
                                                <xsl:when test="@hreflang='en'">🇬🇧 EN</xsl:when>
                                                <xsl:otherwise><xsl:value-of select="@hreflang"/></xsl:otherwise>
                                            </xsl:choose>
                                        </a>
                                    </xsl:for-each>
                                </div>
                            </td>
                            <td>
                                <xsl:choose>
                                    <xsl:when test="sitemap:priority >= 0.9">
                                        <span class="badge badge-high"><xsl:value-of select="sitemap:priority"/></span>
                                    </xsl:when>
                                    <xsl:when test="sitemap:priority >= 0.7">
                                        <span class="badge badge-medium"><xsl:value-of select="sitemap:priority"/></span>
                                    </xsl:when>
                                    <xsl:otherwise>
                                        <span class="badge badge-low"><xsl:value-of select="sitemap:priority"/></span>
                                    </xsl:otherwise>
                                </xsl:choose>
                            </td>
                            <td class="hide-mobile">
                                <xsl:choose>
                                    <xsl:when test="sitemap:changefreq='daily'">
                                        <span class="freq-daily">📅 Günlük</span>
                                    </xsl:when>
                                    <xsl:when test="sitemap:changefreq='weekly'">
                                        <span class="freq-weekly">📆 Haftalık</span>
                                    </xsl:when>
                                    <xsl:when test="sitemap:changefreq='monthly'">
                                        <span class="freq-monthly">🗓️ Aylık</span>
                                    </xsl:when>
                                    <xsl:otherwise>
                                        <xsl:value-of select="sitemap:changefreq"/>
                                    </xsl:otherwise>
                                </xsl:choose>
                            </td>
                            <td class="hide-mobile">
                                <xsl:if test="sitemap:lastmod">
                                    <xsl:value-of select="sitemap:lastmod"/>
                                </xsl:if>
                                <xsl:if test="not(sitemap:lastmod)">
                                    <span style="color: #94a3b8;">-</span>
                                </xsl:if>
                            </td>
                        </tr>
                    </xsl:for-each>
                </tbody>
            </table>
        </div>
        
        <footer class="footer">
            <p>
                Bu sitemap <a href="https://www.sitemaps.org/" target="_blank">sitemaps.org</a> standardına uygundur.
                <br/>
                © 2024 <a href="/">Algoritma Uzmanı</a> - Tüm hakları saklıdır.
            </p>
        </footer>
    </div>
    
    <script>
        function filterTable() {
            var input = document.getElementById('searchInput');
            var filter = input.value.toLowerCase();
            var table = document.getElementById('sitemapTable');
            var rows = table.getElementsByTagName('tr');
            
            for (var i = 1; i &lt; rows.length; i++) {
                var urlCell = rows[i].getElementsByClassName('url-cell')[0];
                if (urlCell) {
                    var url = urlCell.textContent || urlCell.innerText;
                    if (url.toLowerCase().indexOf(filter) > -1) {
                        rows[i].style.display = '';
                    } else {
                        rows[i].style.display = 'none';
                    }
                }
            }
        }
    </script>
</body>
</html>
</xsl:template>
</xsl:stylesheet>

