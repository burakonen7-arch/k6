# Doğrulama — 26 Ekim 2026

## Çalışan kontroller

- Release derleme: 0 uyarı, 0 hata. .NET 10 SDK.
- Swagger JSON, metrik endpoint'i ve DB hazır olma kontrolü erişilebilir.
- Geçersiz sayfa boyutu ve sipariş miktarı: 400.
- Bulunmayan ürün: 404; sipariş verilemeyen ürün: 409.
- Sipariş oluşturma: 201, oluşan siparişi okuma: 200.
- PostgreSQL pg_stat_statements ve EXPLAIN sorguları çalıştırıldı.

| k6 senaryosu | Sonuç | p95 | p99 | HTTP hata | Dropped iteration |
|---|---|---|---|---|---|
| Smoke, 2 VU, 10 sn | Geçti | 30,01 ms | 107,94 ms | %0 | 0 |
| Karma trafik, 100 işlem/sn, 20 sn | Geçti | 51,17 ms | 120,78 ms | %0 | 0 |
| Tek ürün, 50 VU, 200 sipariş denemesi | Doğruluk geçti; gecikme hedefi geçmedi | 2,44 sn | 2,68 sn | %0 (409 beklenen) | 0 |

Stok testi sonrası SQL invariant kontrolü geçti: ürün 1 stok = 0, sipariş sayısı = 100,
toplam sipariş miktarı = 100. Fazla satış gözlenmedi. Gecikme hedefi aşıldığı için
k6 stock komutu 99 çıkış kodu döndürdü; bu başarısızlık saklanmadı.

Aynı ürün satırındaki atomik güncellemeler birbirini bekleyebilir. Toplanan SQL
istatistiklerinde stok UPDATE maliyeti öne çıktı. Kalıcı çözüm seçmeden önce
tekrarlı deney ve test anında lock/connection wait gözlemi gerekir.

## Ortam sınırı

Docker Compose config kontrolü geçti. PostgreSQL ve k6 Docker'da çalıştırıldı.
API imajının MCR katman indirmesi, yönlendirilen westus.data.mcr.microsoft.com
adresinde ortam ağ politikasının 403 engeline takıldı. Bu nedenle API host üzerinde
.NET SDK ile, Docker PostgreSQL'e bağlanarak test edildi. Tam Compose API image
build/start akışı bu ortamda doğrulanamadı. Dockerfile ve Compose yerel çalıştırma
akışı için sağlandı; bu rapor tam Docker doğrulaması iddiası içermez.

Bu kısa testler bir kapasite garantisi değildir. Spike senaryosu hazırlandı fakat
çalıştırılmadı; uzun süreli soak testi yapılmadı. Sonuçlar paylaşımlı ortamın anlık
kaynak koşullarına bağlıdır. Ham raporlar results/*.json içindedir.
