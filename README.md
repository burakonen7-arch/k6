# FlashSale performans laboratuvarı

Tek ASP.NET Core 10 API, PostgreSQL 17 ve isteğe bağlı k6 yük üreticisi.
Swagger: http://localhost:8080/swagger — metrikler: http://localhost:8080/metrics

## Çalıştırma

Docker Engine + Compose v2 yeterli; bilgisayara .NET veya PostgreSQL kurulması gerekmez.

```bash
docker compose up -d --build
# Seed işlemi tamamlanana kadar hazır olma kontrolü:
curl --fail http://localhost:8080/health/ready
```

İlk açılışta 100.000 ürün üretilir. Ürün 1 kampanya ürünü ve 100 stokludur.
Diğer ürünler 100.000 stokludur. Veriler volume içinde kalıcıdır.
Şema disposable laboratuvar için EF EnsureCreated ile oluşturulur; sürümlü üretim
şeması için EF migrations eklenmelidir. Aynı DB üzerinde paralel initializer çalıştırmayın.

## Mimari

```text
Api (Controllers) -> Application (Services, DTO, repository arayüzleri) -> Domain
Infrastructure (EF Core, repository, PostgreSQL) -> Application -> Domain
Api -> Infrastructure (yalnızca composition root ve operasyon controller'ı)
```

İş endpoint'leri controller içindedir. Program.cs DI ve middleware kurulumu yapar.
Domain ve Application EF Core veya ASP.NET bağımlılığı içermez. Service iş akışını,
repository veri erişimini yönetir. Generic repository ve ayrı mikroservis eklenmedi.

- GET /api/products?afterId=0&limit=20: keyset pagination, en fazla 100 kayıt.
- GET /api/products/{id}: ürün detayı.
- POST /api/orders: `{ "productId": 1, "quantity": 1 }`; 201 veya stok yetersizse/ürün yoksa 409.
- GET /api/orders/{id}: sipariş detayı; 404 desteklenir.
- GET /health/live ve /health/ready: süreç ve DB bağlantı kontrolü.

Sipariş stok düşümü `WHERE Stock >= quantity` koşullu atomik SQL güncellemesidir.
Sipariş kaydı aynı transaction içindedir; hata olduğunda stok düşümü rollback olur.
İstek iptali DB'ye taşınır. Kör retry yoktur. Tekrar gönderilen POST yeni sipariş
oluşturabilir; bu sürümde idempotency anahtarı yoktur.

## Yük testleri

API hazır olduktan sonra, proje dizininde:

```bash
# 2 kullanıcı, 10 saniye
docker compose --profile load run --rm k6 run --summary-export=/results/smoke.json /scripts/load.js
# Saniyede 100 işlem, 60 saniye; %60 liste, %30 detay, %10 sipariş
docker compose --profile load run --rm -e MODE=load -e RATE=100 k6 run --summary-export=/results/load.json /scripts/load.js
# Ani yükseliş, 500 işlem/s hedef
docker compose --profile load run --rm -e MODE=spike k6 run --summary-export=/results/spike.json /scripts/load.js
# 100 stok için 200 eşzamanlı satın alma denemesi
docker compose --profile load run --rm -e MODE=stock k6 run --summary-export=/results/stock.json /scripts/load.js
docker compose exec -T db psql -v ON_ERROR_STOP=1 -U lab -d flashsale < scripts/verify-stock.sql
```

Stock testi ürün 1'in başlangıç stoğunun 100 olmasını gerektirir. Tam olarak 100
sipariş ve sıfır kalan stok SQL ile doğrulanır. Beklenen 409'lar transport hatası
sayılmaz; karma trafik senaryosunda siparişin 201 olması ayrıca check edilir.
Hedefler p95 < 500 ms, p99 < 1500 ms, hata <%1 ve dropped iteration = 0.
Bunlar eğitim hedefidir, ölçülmüş kapasite iddiası değildir. Donanımınıza göre
RATE'i 50, 100, 200 şeklinde artırın. Yük üreticisi de aynı makinenin kaynaklarını kullanır.

## Darboğaz inceleme

```bash
docker stats
docker compose logs --tail=100 api db
docker compose exec -T db psql -U lab -d flashsale < db/diagnostics.sql
curl http://localhost:8080/metrics
```

Metrikler HTTP süre/hata histogramlarını ve süreç metriklerini içerir; Prometheus
veya Grafana servisi kurulmadı. pg_stat_statements pahalı sorguları gösterir,
200 ms üzerindeki sorgular PostgreSQL loguna yazılır. diagnostics.sql ayrıca
bekleme olaylarını ve EXPLAIN (ANALYZE, BUFFERS) çıktısını verir.

Önce smoke, ardından load testi yapın. p95/p99, throughput, hata, dropped_iterations,
CPU/bellek ve DB beklemelerini birlikte kaydedin. Bir seferde tek değişiklik yapıp
aynı yükle karşılaştırın. İlk deneyler: API DB bağlantı havuzunu 50'den 10'a düşürme,
sayfa boyutunu 20/100 karşılaştırma, popüler ürüne yoğun stok rekabeti.
AsNoTracking, DTO projection, keyset pagination ve DB context pooling başlangıçta
etkindir. Kasıtlı hatalı endpoint yoktur. Cache eklenmedi; stok verisi günceldir.

## Yaşam döngüsü ve sınırlar

```bash
docker compose down # Verileri korur
# TÜM laboratuvar verilerini siler; yalnızca yeniden başlamak istediğinizde:
docker compose down -v
```

Portlar yalnız localhost'a bağlıdır. Kimlik doğrulama yoktur; parolalar yerel
laboratuvar içindir. İnternete yayınlamak için tasarlanmamıştır. Swagger ve metrikler
açıktır. Ek Redis, kuyruk, ödeme servisi veya izleme sunucusu yoktur.

Yönetilen bulut ortamında NuGet TLS güveni gerekirse build'e ortamın CA dosyasını
secret olarak verin: `docker build --secret id=proxy_ca,src="$CODEX_PROXY_CERT" -t flashsale-lab-api .`.
Yerel bilgisayarda standart Compose build yeterlidir.

## Ek doğrulama ve kampanya sıfırlama

```bash
python3 scripts/check-api.py
# Kampanya siparişlerini SİLER; stok testini yeniden başlatmadan önce:
docker compose exec -T db psql -v ON_ERROR_STOP=1 -U lab -d flashsale < scripts/reset-campaign.sql
```

Bu sıfırlama sadece ürün 1'i etkiler. Test raporu: VALIDATION.md.
