# Workplan Manuel Uçtan Uca Test Senaryoları

Bu doküman uygulamayı tarayıcıdan elle test etmek için hazırlandı. Ana adres Docker ile çalıştırmada `http://localhost:5276`, API dokümantasyonu `http://localhost:5291/scalar`.

## Teste Hazırlık

Uygulamayı başlat:

```bash
docker compose up --build
```

Alternatif lokal çalıştırma:

```bash
./run-all.sh http
```

Temiz demo veriyle başlamak istersen Docker volume'u silerek yeniden başlatabilirsin:

```bash
docker compose down -v
docker compose up --build
```

## Demo Kullanıcılar

| Kullanıcı | Şifre | Rol | Not |
| --- | --- | --- | --- |
| `admin@workplan.local` | `ChangeMe123!` | SystemAdmin | Kullanıcı ve master data yönetimi |
| `to1@workplan.local` | `Demo123!` | TechnicalOfficeEngineer | A bölgesi teknik ofis |
| `hom1@workplan.local` | `Demo123!` | HeadOfMaster | A bölgesindeki iş/lokasyonlar |
| `hom2@workplan.local` | `Demo123!` | HeadOfMaster | A bölgesindeki diğer lokasyon |
| `sc1@workplan.local` | `Demo123!` | SiteChief | A bölgesi saha şefi |
| `pm1@workplan.local` | `Demo123!` | ProjectManager | Projenin PM'i |

Diğer seed kullanıcıları aynı şifreyle gelir: `to2@...`, `sc2@...`, `hom3@...` vb.

## Durum Etiketleri

| İş akışı durumu | Ekranda beklenen etiket |
| --- | --- |
| Assigned | Atandı |
| InProgress | Devam Ediyor |
| Submitted | Site Chief Onayında |
| ApprovedBySiteChief | Saha Şefi Onayladı |
| ApprovedByPM | Tamamlandı |

## E2E-01 Giriş ve Menü Yetkileri

Amaç: Her rolün doğru menüleri görmesini ve çıkışın çalışmasını kontrol etmek.

1. `http://localhost:5276/login` sayfasını aç.
2. `admin@workplan.local` / `ChangeMe123!` ile giriş yap.
3. Ana sayfada `Günlük Plan Oluştur`, `Projeler`, `İş Kalemi Tipleri`, `Kullanıcı Yönetimi` kartlarını gör.
4. Sol menüde `Kullanıcı Yönetimi` linkinin görünür olduğunu doğrula.
5. `Çıkış yap` ile çık.
6. `hom1@workplan.local` / `Demo123!` ile tekrar giriş yap.
7. `Bana Atanan İşler` ve `Beklenen Onaylar` kartlarını gör.
8. `Kullanıcı Yönetimi` ve `Günlük Plan Oluştur` linklerinin görünmediğini doğrula.

Beklenen sonuç: Kullanıcı rolüne göre menü/kart görünürlüğü değişir, çıkıştan sonra login sayfasına yönlenilir.

## E2E-02 Yeni Günlük Plan Oluşturma

Amaç: Technical Office rolüyle yeni plan oluşturup Head of Master'a atamak.

1. `to1@workplan.local` ile giriş yap.
2. `Günlük Plan Oluştur` sayfasına git.
3. `Proje` alanında `Silopi Kombine Çevrim Doğalgaz Santrali İnşaatı` seç.
4. `Saha bölgesi` alanında `A Bölgesi - Türbin Binası ve Jeneratör Sahası` seç.
5. `Lokasyon` alanında `Türbin Binası - Zemin Kat` seç.
6. `Ustabaşı` alanında `Ali Kaya` veya `hom1@workplan.local` seç.
7. İş kalemi seçicide yaprak iş kalemi seç: örnek `Betonarme İşleri` -> `Kalıp İşleri` -> `Temel Kalıbı`.
8. `İş tarihi` için bugünün veya yarının tarihini seç.
9. `Planlanan miktar` değerini `12`, `Planlanan adam-gün` değerini `3` gir.
10. `Birim` alanının seçilen iş kalemine göre otomatik dolduğunu doğrula.
11. `Oluştur` butonuna bas.
12. Onay modalinde işlemi onayla.

Beklenen sonuç: `Günlük plan oluşturuldu.` mesajı görünür ve form temizlenir.

## E2E-03 Atanan İş Bildirimi ve Okunduya Çekme

Amaç: Oluşturulan planın Head of Master ekranına ve bildirimlere düşmesini kontrol etmek.

1. `Çıkış yap`.
2. `hom1@workplan.local` ile giriş yap.
3. Ana sayfadaki `Bildirimlerim` bölümünde yeni atanan işe ait bildirim olduğunu doğrula.
4. Bildirim linkine tıkla.
5. `İş Detayı` sayfasında proje, saha bölgesi, lokasyon, tarih, planlanan miktar ve durum bilgilerini kontrol et.
6. Sayfadan ana sayfaya dön.

Beklenen sonuç: Bildirime tıklanınca ilgili iş detayına gidilir. Detay açıldıktan sonra aynı bildirim okunmuş sayılmalı ve ana sayfadaki okunmamış bildirimlerden düşmelidir.

## E2E-04 Ekip Oluşturma ve Üye Ekleme

Amaç: Head of Master'ın kendi lokasyonu için ekip oluşturabilmesini kontrol etmek.

1. `hom1@workplan.local` ile giriş yapmış halde `Bana Atanan İşler` sayfasına git.
2. E2E-02'de oluşturduğun `Atandı` durumlu kaydın `Detay` butonuna bas.
3. `İşi başlat` bölümünde `Yeni ekip oluştur` linkine tıkla.
4. `İsim` alanına `Manuel Test Ekibi` yaz ve `Oluştur` butonuna bas.
5. Onay modalini onayla.
6. Oluşan ekip kartında `Üye ekle` butonuna bas.
7. `Rol` seçimini örnek `GeneralLabor` bırak.
8. `Personel Ref.` alanına `TEST-001 - Manuel Kullanıcı` yaz.
9. `Ekle` butonuna bas ve onayla.

Beklenen sonuç: Ekip listede görünür, eklenen üye ekip tablosuna gelir.

## E2E-05 İşi Başlatma

Amaç: Atandı durumundaki planı InProgress durumuna geçirmek.

1. E2E-02'de oluşturulan işin `İş Detayı` sayfasına dön.
2. `İşi başlat` bölümünde `Ekip` alanından `Manuel Test Ekibi` veya mevcut demo ekiplerden birini seç.
3. `Başlat` butonuna bas.
4. Onay modalini onayla.

Beklenen sonuç: Başarılı mesaj görünür, durum `Devam Ediyor` olur, `Gün sonu ilerleme gir` formu görünür. `Durum Geçmişi` bölümünde `Atandı` -> `Devam Ediyor` geçişi görünür.

## E2E-06 Gün Sonu İlerleme Girme

Amaç: InProgress iş için gerçekleşen değerleri girip Site Chief onayına göndermek.

1. `Gün sonu ilerleme gir` formunda `Gerçek Miktar` alanına `10` yaz.
2. `Gerçek Adam-Gün` alanına `2.5` yaz.
3. `Fazla Mesai` alanına `1` yaz.
4. `Yorum` alanına `Manuel E2E gerçekleşme girişi` yaz.
5. `Kaydet ve Onaya Sun` butonuna bas.
6. Onay modalini onayla.

Beklenen sonuç: Başarılı mesaj görünür, durum `Site Chief Onayında` olur. Detayda gerçekleşen miktar, adam-gün, fazla mesai ve yorum görünür.

## E2E-07 Site Chief Onayı

Amaç: Site Chief'in kendi bölgesindeki Submitted işi onaylamasını kontrol etmek.

1. `Çıkış yap`.
2. `sc1@workplan.local` ile giriş yap.
3. Ana sayfada `Onay Bekleyen İşlerim` bölümünde E2E-06'da gönderilen işi gör.
4. İş linkine veya `Beklenen Onaylar` -> `Detay` akışına gir.
5. `Onay` bölümünde `Onayla` butonuna bas.
6. Onay modalini onayla.

Beklenen sonuç: Durum `Saha Şefi Onayladı` olur. `Durum Geçmişi` bölümünde Site Chief onayı görünür.

## E2E-08 Project Manager Son Onayı ve Raporlama

Amaç: PM'in son onayı vermesini ve işin raporlara düşmesini kontrol etmek.

1. `Çıkış yap`.
2. `pm1@workplan.local` ile giriş yap.
3. Ana sayfada `Onay Bekleyen İşlerim` bölümünde E2E-07'de onaylanan işi gör.
4. İş detayına gir.
5. `Onayla` butonuna bas ve modalde onayla.
6. Durumun `Tamamlandı` olduğunu doğrula.
7. `Raporlar` sayfasına git.
8. `Onaylanmış İşler` tablosunda tamamlanan işi gör.
9. `Yönetici İzleme` bölümünde `Ali Kaya` veya `hom1@workplan.local` seç.
10. Aynı işin ustabaşı bazlı listede de göründüğünü doğrula.

Beklenen sonuç: PM onayından sonra iş `Tamamlandı` olur ve rapor ekranlarında görünür.

## E2E-09 Site Chief Red Akışı

Amaç: Site Chief reddedince işin Head of Master'a geri dönmesini kontrol etmek.

1. E2E-02 ile yeni bir plan oluştur.
2. `hom1@workplan.local` ile plana gir, ekip seçip `Başlat`.
3. Gün sonu formunda geçerli gerçekleşme değerleri girip onaya sun.
4. `sc1@workplan.local` ile giriş yap.
5. İlgili işin detayında `Reddet` butonuna bas.
6. `Red nedeni` alanına `Metraj kontrolü bekleniyor` yaz.
7. `Reddi Onayla` butonuna bas ve modalde onayla.

Beklenen sonuç: İş durumu tekrar `Devam Ediyor` olur. Yorum/Geçmiş bölümünde red gerekçesi görünür. `hom1@workplan.local` ile giriş yapıldığında iş yeniden ilerleme girilebilir durumdadır.

## E2E-10 PM Red Akışı

Amaç: PM reddedince Site Chief onaylı işin Head of Master'a geri dönmesini kontrol etmek.

1. E2E-02 ile yeni bir plan oluştur.
2. HoM ile başlat ve geçerli gerçekleşme gir.
3. Site Chief ile onayla.
4. `pm1@workplan.local` ile giriş yap.
5. İş detayında `Reddet` butonuna bas.
6. `Red nedeni` alanına `PM revizyon talebi` yaz.
7. `Reddi Onayla` butonuna bas ve modalde onayla.

Beklenen sonuç: İş durumu `Devam Ediyor` olur, red nedeni durum geçmişine yazılır ve HoM aynı iş için yeniden gün sonu ilerleme girebilir.

## E2E-11 İlerleme Validasyonları

Amaç: Gün sonu formundaki iş kurallarını elle kontrol etmek.

Ön koşul: `hom1@workplan.local` ile `Devam Ediyor` durumunda bir iş detayı açık olsun.

1. `Gerçek Miktar` alanına `5` yaz, `Gerçek Adam-Gün` alanını boş bırak, `Kaydet ve Onaya Sun` butonuna bas.
2. Hata mesajı olarak gerçek miktar ve adam-günün birlikte girilmesi gerektiğini doğrula.
3. Formu boşalt, `Yorum` alanını da boş bırakıp tekrar gönder.
4. Hata mesajı olarak ilerleme yoksa gerekçe yazılması gerektiğini doğrula.
5. `Gerçek Miktar`, `Gerçek Adam-Gün` veya `Fazla Mesai` alanına negatif değer girip gönder.
6. Negatif değer hatası alındığını doğrula.
7. Son olarak `Gerçek Miktar` ve `Gerçek Adam-Gün` alanlarını pozitif değerlerle gönder.

Beklenen sonuç: Hatalı kombinasyonlar reddedilir, geçerli kombinasyon `Site Chief Onayında` durumuna geçer.

## E2E-12 Plan Oluşturma Validasyonları

Amaç: Günlük plan formunun eksik/hatalı veriyle işlem yapmamasını kontrol etmek.

1. `to1@workplan.local` ile `Günlük Plan Oluştur` sayfasına git.
2. Hiç alan doldurmadan `Oluştur` butonunun pasif olduğunu doğrula.
3. Proje seçmeden saha bölgesi/lokasyon seçimlerinin dolmadığını doğrula.
4. Yaprak iş kalemi seçmeden `Birim` alanının dolmadığını veya `-` kaldığını doğrula.
5. `Planlanan miktar` veya `Planlanan adam-gün` alanına negatif değer girildiğinde `Oluştur` butonunun pasif kaldığını doğrula.
6. Tüm zorunlu alanları geçerli doldur ve planın oluştuğunu doğrula.

Beklenen sonuç: Eksik ya da negatif veriyle plan oluşturulamaz.

## E2E-13 Yetki ve Scope Kontrolleri

Amaç: Doğru rolün bile sadece kendi kapsamındaki işleri görebildiğini/işleyebildiğini doğrulamak.

1. `to1@workplan.local` ile A bölgesi, `hom1@workplan.local` üstüne bir plan oluştur.
2. `hom2@workplan.local` ile giriş yap.
3. `Bana Atanan İşler` sayfasında bu planın görünmediğini doğrula.
4. Plan detay URL'sini biliyorsan doğrudan açmayı dene.
5. İşe başlatma veya ilerleme gönderme işlemini yapamadığını doğrula.
6. `sc2@workplan.local` ile giriş yap.
7. `Beklenen Onaylar` sayfasında A bölgesine ait bu işin görünmediğini doğrula.
8. `pm1@workplan.local` ile giriş yap.
9. Site Chief onayı bekleyen bir işi PM'in kendi aşamasında olmadığı için onaylayamadığını doğrula.

Beklenen sonuç: Kayıtlar rol ve kapsam filtresine göre görünür. Yanlış aşamadaki onay denemeleri hata verir.

## E2E-14 Kullanıcı Yönetimi

Amaç: SystemAdmin'in kullanıcı oluşturma, rol güncelleme, aktiflik ve parola sıfırlama işlemlerini test etmek.

1. `admin@workplan.local` ile giriş yap.
2. `Kullanıcı Yönetimi` sayfasına git.
3. `E-posta` alanına `manual.qa@workplan.local` yaz.
4. `Ad Soyad` alanına `Manual QA` yaz.
5. `Şifre` alanına `Manual123!` yaz.
6. `Roller` içinden `HeadOfMaster` seç.
7. `Oluştur` butonuna bas ve onayla.
8. Kullanıcının tabloda `Aktif` olarak göründüğünü doğrula.
9. `Rolleri Düzenle` ile `HeadOfMaster` yerine `TechnicalOfficeEngineer` seç, `Kaydet` ve onayla.
10. `Pasif Yap` butonuna bas ve onayla.
11. `Şifre Sıfırla` butonuna bas, `Manual456!` gir, `Kaydet` ve onayla.

Beklenen sonuç: Kullanıcı oluşturulur, rolleri güncellenir, pasif yapılır ve parola sıfırlama başarılı mesajı verir.

## E2E-15 Master Data Duman Testi

Amaç: Proje, lokasyon ve iş kalemi ekranlarının en azından listeleme ve temel gezinme açısından çalıştığını doğrulamak.

1. `admin@workplan.local` veya `to1@workplan.local` ile giriş yap.
2. `Projeler` sayfasına git.
3. Demo projenin listede göründüğünü doğrula.
4. Proje detay/ilgili linklerden saha bölgeleri ve lokasyonlara git.
5. A bölgesi, `Türbin Binası`, `Türbin Binası - Zemin Kat` gibi demo kayıtların göründüğünü doğrula.
6. `İş Kalemi Tipleri` sayfasına git.
7. `Betonarme İşleri`, `Kalıp İşleri`, `Temel Kalıbı` hiyerarşisinin göründüğünü doğrula.

Beklenen sonuç: Master data ekranları yüklenir, demo kayıtlar hiyerarşik ve okunabilir şekilde görünür.

## E2E-16 Mobil Görünüm Duman Testi

Amaç: Mobil saha akışının kullanılabilir olduğunu kontrol etmek.

1. Tarayıcı geliştirici araçlarında mobil viewport aç.
2. `hom1@workplan.local` ile giriş yap.
3. Üstteki menü butonuyla menüyü aç/kapat.
4. `Bana Atanan İşler` sayfasına git.
5. Bir işin `Detay` sayfasına gir.
6. Ekip seçimi, `Başlat`, gün sonu formu ve `Kaydet ve Onaya Sun` butonlarının taşmadan kullanılabildiğini doğrula.

Beklenen sonuç: Mobilde menü, tablolar ve formlar kullanılabilir; kritik butonlar ekrandan taşmaz.

## Hızlı Regresyon Kontrol Listesi

- Login başarısız denemede hata gösteriyor.
- Login başarılı denemede ana sayfaya dönüyor.
- Çıkış yap token'ları temizleyip login sayfasına götürüyor.
- Bildirimler okunmamışken ana sayfada görünüyor, detay açılınca okunmuş oluyor.
- DailyPlan mutlu yol sırası bozulmuyor: `Atandı` -> `Devam Ediyor` -> `Site Chief Onayında` -> `Saha Şefi Onayladı` -> `Tamamlandı`.
- Red sonrası iş `Devam Ediyor` durumuna dönüyor.
- Raporlarda sadece tamamlanan işler listeleniyor.
- Yanlış rol/kapsam kendi dışındaki kaydı işleyemiyor.
- Kullanıcı yönetiminde en az bir rol olmadan kaydetme yapılamıyor.
- Tema değiştirme butonu açık/koyu temayı değiştiriyor.
