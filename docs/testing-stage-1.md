# Aşama 1 — İş kurallarının testlerle güvenceye alınması

Bu belge aşama 1'in tarihsel kaydıdır. Saat bağımlılığına ilişkin ilk bulgu daha
sonra [aşama 2'nin ilk adımında](testing-stage-2-clock.md) ele alınmıştır.

Bu proje, eğitim çalışmasından geliştirilen bir backend/QA portföy projesidir.
Bu testler üretim kullanımı, kimlik doğrulama, eşzamanlılık güvenliği veya eksiksiz
kapsama kanıtı değildir. Mevcut OOP modeli ve uygulama kodu bu aşamada korunmuştur.

## Çalıştırma ve sonuç

```powershell
dotnet test library-management-system.sln --configuration Release
```

22 Eylül 2026 yerel doğrulaması: .NET SDK 10.0.204, hedef/runtime ailesi .NET 8.
12 test vakası geçti; 0 başarısız, 0 atlanan. İlk paket indirme denemesi ortamın
ağ kısıtlamasına takıldı; ağ erişimiyle yeniden çalıştırmada restore ve testler başarılı oldu.
Bu sayı `[Theory]` veri satırlarını ayrı vakalar olarak sayar; kod kapsamı yüzdesi ölçülmedi.

| Senaryo | Vaka sayısı | Doğrulanan davranış |
| --- | ---: | --- |
| Üye/çalışan limitleri | 2 | 5/10 öğeye kadar başarı, sonraki isteğin reddi, iadeyle bir yer açılması |
| İkinci ödünç alma | 2 | Aynı veya farklı kullanıcıya tekrar ödünç verilememesi |
| Yanlış kişinin iadesi | 1 | Başkasının öğesini iade edememesi; iki kullanıcının durumunun korunması |
| İade ve yeniden ödünç alma | 2 | Aynı/farklı kullanıcı, geçmişin korunması, yalnızca etkin kaydın kapanması, ikinci iadenin reddi |
| Hiç ödünç alınmamış öğenin iadesi | 1 | İsteğin reddi ve durumun korunması |
| Medya türüne göre son tarih | 4 | Kitap 14, sesli kitap 7, e-kitap 21, dergi 3 gün |

Hatalı işlemler öncesinde katalog, kullanıcı kayıtları, ödünç geçmişi, medyanın
`IsLoaned` değeri, kullanıcıların ödünç listeleri ve kayıtların tarihleri kopyalanır.
Exception sonrasında bu değerlerin değişmediği doğrulanır. Testler normal
`Library` akışını kullanır; limitlere ulaşmak için listelere elle öğe eklemez.

## Tarih ve izolasyon kararları

- `Loan` yapıcısı tarih kabul ettiği için süre hesabı sabit tarihlerle doğrulanır:
  artık yıl/Şubat ve yıl sonu geçişleri, ayrıca saat bileşeninin korunması.
- `Library` üzerinden oluşturulan son tarih, kaydın kendi `LoanDate` değerine
  göre denetlenir. Testin başında/sonunda `DateTime.Today` okuyup gece yarısında
  kırılabilecek eşitlik kontrolleri yapılmaz.
- Başarılı iadede `ReturnedDate` doluluğu ve eski kaydın değişmemesi doğrulanır;
  iade tarihinin tam olarak beklenen gün olması bu aşamanın kanıtı değildir.
- Her vaka kendi nesnelerini oluşturur. Sabit `U001`, `M001`, `L001` beklentisi
  yoktur; üretim kodundaki statik sayaçlar sıfırlanmaz. Mevcut testler aynı xUnit
  koleksiyonundadır; ileride bu sayaçları kullanan test sınıfları aynı koleksiyona
  alınmalı veya sayaç tasarımı düzeltilmelidir. Bu, eşzamanlılık testi değildir.

## Sonraki küçük refaktör için somut bulgular

1. **Kontrol edilebilir saat:** `Library` ve `Loan.IsOverdue()` doğrudan
   `DateTime.Today` kullanıyor. Enjekte edilen `TimeProvider` veya küçük bir saat
   arayüzüyle tam ödünç/iade günü ve gecikme sınırı deterministik test edilmeli.
   Özellikle son tarihin kendisi (gecikmemiş), ertesi gün (gecikmiş) ve iade edilmiş
   kayıt testleri eklenmeli. Mevcut durumda saat değiştirme, bekleme veya reflection
   kullanılarak sahte güven üretilmedi. Bunlar atlanan testler değil, henüz eklenmeyen senaryolardır.
2. **Durumun korunması:** `User.BorrowedItems` dışarıdan değiştirilebilir;
   `Media.MarkAsLoaned/MarkAsAvailable` ve `Loan.MarkReturned` herkese açıktır.
   Servis dışından tutarsız durum yaratılabilir. Salt okunur görünüm ve kontrollü
   durum geçişleri değerlendirilmeli; bu testler böyle bir bozulmayı engellediğini iddia etmez.
3. **İade bütünlüğü:** `ReturnMedia`, etkin geçmiş kaydını bulmadan önce medya ve
   kullanıcı durumunu değiştirir; kayıt yoksa `activeLoan?.MarkReturned` sessizce geçer.
   Bozulmuş durum için beklenen davranış belirlenip önce doğrulama yapılmalı.
   Bu davranış zorla oluşturulup doğru bir iş kuralıymış gibi testlerle sabitlenmedi.
4. **Kimlik sayaçları:** Statik `++` sayaçları süreç çapında paylaşılır, eşzamanlı
   kullanıma güvence sağlamaz. Paralel kullanım gereksinimi belirlenmeden güvenli
   olduğu iddia edilmemeli.
5. **Konsol bağımlılığı:** İş kuralları doğrudan `Console.WriteLine` çağırıyor.
   Daha sonra sunum/çıktı ayrımı değerlendirilebilir; testler konsol metnine bağlanmaz.
6. **CI ve dürüst README:** Sonraki aşamada build/test otomasyonu ve test komutu
   README'ye eklenmeli. “Role-based access control” ifadesi gerçek kimlik doğrulama
   veya yetkilendirme altyapısı izlenimi veriyor; mevcut davranışın kullanıcı türüne
   göre iş kuralı denetimi olduğu açıklanmalı. Encapsulation iddiası da dışarıya açık
   değiştirilebilir durumla uyumlu hale getirilmeli. Üretimde kullanım iddiası eklenmemeli.

API ve veritabanı bu aşamanın kapsamında değildir. Temel sağlamlaştığında SQLite
ve küçük bir ASP.NET Core API ayrıca değerlendirilecektir.
