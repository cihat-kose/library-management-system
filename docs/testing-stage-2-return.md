# Aşama 2 / ikinci küçük adım — İade kaydının doğrulanması

`ReturnMedia`, medya ve kullanıcı kontrollerinden sonra aynı medya/kullanıcı için
etkin ödünç kaydını arar. Kayıt yoksa `InvalidOperationException` fırlatır;
medyanın uygunluk durumu, kullanıcı listeleri ve ödünç geçmişi değişmez.
Kayıt varsa tarih okunur, ardından mevcut iade adımları uygulanır.
Önceki `activeLoan?.MarkReturned(...)` sessiz geçişi kaldırılmıştır.

Üç regresyon vakası eklendi: hiç kayıt olmaması, yalnızca kapanmış kayıt olması
ve etkin kaydın başka kullanıcıya ait olması. Testler, hâlen açık olan durum
değiştirme metotları/listeleriyle tutarsızlık oluşturur. Bu desteklenen bir ödünç
akışı değil, bozulmuş duruma karşı savunma testidir. Reflection kullanılmaz.
Her reddedilen işlemde önceki durumun değer kopyalarıyla değişmezlik doğrulanır.

Üç vaka düzeltme öncesinde exception fırlatılmadığı için başarısız oldu.
Düzeltme sonrası `dotnet test library-management-system.sln --configuration Release --no-restore`
sonucu: önceki 17 ve yeni 3 vaka, **20 başarılı, 0 başarısız, 0 atlanan**.
`git diff --check` içerik/boşluk hatası vermedi.

Bu değişiklik tutarsız veriyi onarmaz veya dışarıdan bozulmasını engellemez;
eşzamanlı işlemler için atomiklik garantisi değildir. Koleksiyonların kapatılması,
konsol ayrımı, API ve veritabanı kapsam dışında tutuldu. Menü ve başarılı iade
mesajları değişmedi; başarılı iade/yeniden ödünç alma testleri geçmeye devam eder.
