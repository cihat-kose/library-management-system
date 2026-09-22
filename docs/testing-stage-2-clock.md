# Aşama 2 / ilk küçük adım — Kontrol edilebilir tarih

Sonraki iade doğrulama değişikliği ve güncel test sonucu
[ikinci küçük adımın notunda](testing-stage-2-return.md) kayıtlıdır.

`Library(TimeProvider? timeProvider = null)` ve `Loan` yapıcısının isteğe bağlı
son parametresi aynı .NET 8 `TimeProvider` türünü kullanır. `null` verilirse
`TimeProvider.System` seçilir; eski çağrılar geçerlidir. Yeni üretim arayüzü,
bağımlılık paketi veya DI altyapısı eklenmedi.

`Library`, oluşturduğu `Loan` nesnesine aynı sağlayıcıyı aktarır. Bu iki sınıftaki
tüm `DateTime.Today` okumaları `GetLocalNow().Date` ile değiştirildi; UTC gününe
geçilmedi. Tarihler takvim günü olarak kullanılmaya devam eder. İade kaydı ve
log satırı aynı kez okunan tarihi kullanır. Menü ve demo başlangıç kodu değişmedi.
Java karşılığı `Clock.systemDefaultZone()` ve `LocalDate.now(clock)` yaklaşımıdır.

Testlerde küçük bir `TimeProvider` alt sınıfı `GetUtcNow()` ve `LocalTimeZone`
değerlerini kontrol eder. Gerçek saat, makinenin saat dilimi, bekleme ve ek test
paketi kullanılmadan beş yeni vaka çalışır:

- Son tarih günü 00:00: gecikmemiş.
- Son tarih günü 23:00: hâlâ gecikmemiş.
- Ertesi gün 00:00: gecikmiş.
- İade edilen kayıt: tarih ilerlese de gecikmiş sayılmaz.
- `Library` üzerinden ödünç/iade: UTC+02 yerel günü ile kesin ödünç, son iade ve
  gerçekleşen iade tarihleri; aynı saatin `Loan.IsOverdue()` içine aktarılması.

22 Eylül 2026 doğrulaması: önceki 12 test değiştirilmeden korundu; yeni 5 testle
Release çalıştırmasında **17 başarılı, 0 başarısız, 0 atlanan**.

## Quick start doğrulaması

Kaynak ve proje dosyaları `bin/obj` içermeyen ayrı bir geçici dizine kopyalandı.
Bu dizinin kökünde sırayla şu komutlar çalıştırıldı:

```text
dotnet restore
dotnet build
dotnet test
dotnet run --project LibraryManagementSystem
```

- Restore başarılı. Korumalı ortamdaki ilk restore/build denemeleri NuGet ağ
  engeline takıldı; ağ erişimli tekrarlar başarılı oldu.
- Build: 0 hata, 0 uyarı.
- Test (varsayılan Debug): 17 başarılı, 0 başarısız, 0 atlanan.
- Run: standart girdiye menü seçimleri gönderildi; menü açıldı. `1`, ardından
  `2/U002/M001`, `4/U002`, `3/U002/M001`, `7`, `1`, `0` akışı başarıyla tamamlandı.
  “1984” ödünç alındı, 14 günlük süre gösterildi, iade kaydı oluştu, kitap yeniden
  kullanılabilir listesinde göründü; süreç 0 çıkış koduyla kapandı.

Bu doğrulama temiz kaynak/derleme çıktısı içindir; sıfırdan işletim sistemi veya
boş NuGet önbelleği testi değildir. Mevcut Windows ortamındaki SDK 10.0.204 ve
.NET 8 runtime kullanıldı; doğrudan SDK 8 ile ayrıca çalıştırılmadı. Projeler
`net8.0` hedeflemeye devam eder. README, en kolay kurulum için .NET 8 SDK ister.

Konsol ayrımı, koleksiyonların kapatılması, API, veritabanı ve CI bu adımda
değiştirilmedi. `Media.PublicationYear` ve demo dergisinin yılı için kullanılan
sistem tarihi de kapsam dışında kaldı. Sonraki küçük refaktör adayı, iade işleminde
etkin ödünç kaydını durum değiştirmeden önce doğrulamaktır; burada uygulanmadı.
