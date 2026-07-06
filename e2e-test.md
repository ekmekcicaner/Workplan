# Workplan Manuel E2E Test TODO Listesi

## 0. Teste Hazırlık

* [ ] Uygulamayı Docker ile başlat: `docker compose up --build`
* [ ] Alternatif lokal çalıştırma gerekiyorsa çalıştır: `./run-all.sh http`
* [ ] Temiz demo veri gerekiyorsa Docker volume’u sil:

  * [ ] `docker compose down -v`
  * [ ] `docker compose up --build`
* [ ] Web uygulamasını aç: `http://localhost:5276`
* [ ] API dokümantasyonunu kontrol et: `http://localhost:5291/scalar`

---

## 1. Demo Kullanıcılar

* [ ] Admin kullanıcısı hazır: `admin@workplan.local` / `ChangeMe123!`
* [ ] Technical Office kullanıcısı hazır: `to1@workplan.local` / `Demo123!`
* [ ] Head of Master 1 hazır: `hom1@workplan.local` / `Demo123!`
* [ ] Head of Master 2 hazır: `hom2@workplan.local` / `Demo123!`
* [ ] Site Chief hazır: `sc1@workplan.local` / `Demo123!`
* [ ] Project Manager hazır: `pm1@workplan.local` / `Demo123!`

---

## 2. Durum Etiketleri Kontrolü

* [ ] `Assigned` ekranda `Atandı` görünüyor
* [ ] `InProgress` ekranda `Devam Ediyor` görünüyor
* [ ] `Submitted` ekranda `Site Chief Onayında` görünüyor
* [ ] `ApprovedBySiteChief` ekranda `Saha Şefi Onayladı` görünüyor
* [ ] `ApprovedByPM` ekranda `Tamamlandı` görünüyor

---

# E2E Senaryoları

## E2E-01 — Giriş ve Menü Yetkileri

**Amaç:** Her rolün doğru menüleri görmesini ve çıkışın çalışmasını kontrol etmek.

* [ ] `http://localhost:5276/login` sayfasını aç
* [ ] `admin@workplan.local` / `ChangeMe123!` ile giriş yap
* [ ] Ana sayfada şu kartların göründüğünü doğrula:

  * [ ] `Günlük Plan Oluştur`
  * [ ] `Projeler`
  * [ ] `İş Kalemi Tipleri`
  * [ ] `Kullanıcı Yönetimi`
* [ ] Sol menüde `Kullanıcı Yönetimi` linkinin göründüğünü doğrula
* [ ] `Çıkış yap` ile çık
* [ ] `hom1@workplan.local` / `Demo123!` ile giriş yap
* [ ] Ana sayfada şu kartların göründüğünü doğrula:

  * [ ] `Bana Atanan İşler`
  * [ ] `Beklenen Onaylar`
* [ ] Şu linklerin görünmediğini doğrula:

  * [ ] `Kullanıcı Yönetimi`
  * [ ] `Günlük Plan Oluştur`

**Beklenen sonuç:**

* [ ] Kullanıcı rolüne göre menü/kart görünürlüğü değişiyor
* [ ] Çıkıştan sonra login sayfasına yönleniliyor

---

## E2E-02 — Yeni Günlük Plan Oluşturma

**Amaç:** Technical Office rolüyle yeni plan oluşturup Head of Master’a atamak.

* [ ] `to1@workplan.local` ile giriş yap
* [ ] `Günlük Plan Oluştur` sayfasına git
* [ ] `Proje` alanında `Silopi Kombine Çevrim Doğalgaz Santrali İnşaatı` seç
* [ ] `Saha bölgesi` alanında `A Bölgesi - Türbin Binası ve Jeneratör Sahası` seç
* [ ] `Lokasyon` alanında `Türbin Binası - Zemin Kat` seç
* [ ] `Ustabaşı` alanında `Ali Kaya` veya `hom1@workplan.local` seç
* [ ] İş kalemi seçicide yaprak iş kalemi seç:

  * [ ] `Betonarme İşleri`
  * [ ] `Kalıp İşleri`
  * [ ] `Temel Kalıbı`
* [ ] `İş tarihi` için bugünün veya yarının tarihini seç
* [ ] `Planlanan miktar` alanına `12` gir
* [ ] `Planlanan adam-gün` alanına `3` gir
* [ ] `Birim` alanının seçilen iş kalemine göre otomatik dolduğunu doğrula
* [ ] `Oluştur` butonuna bas
* [ ] Onay modalinde işlemi onayla

**Beklenen sonuç:**

* [ ] `Günlük plan oluşturuldu.` mesajı görünüyor
* [ ] Form temizleniyor

---

## E2E-03 — Atanan İş Bildirimi ve Okunduya Çekme

**Amaç:** Oluşturulan planın Head of Master ekranına ve bildirimlere düşmesini kontrol etmek.

* [ ] `Çıkış yap`
* [ ] `hom1@workplan.local` ile giriş yap
* [ ] Ana sayfadaki `Bildirimlerim` bölümünde yeni atanan işe ait bildirim olduğunu doğrula
* [ ] Bildirim linkine tıkla
* [ ] `İş Detayı` sayfasında şu bilgileri kontrol et:

  * [ ] Proje
  * [ ] Saha bölgesi
  * [ ] Lokasyon
  * [ ] Tarih
  * [ ] Planlanan miktar
  * [ ] Durum bilgisi
* [ ] Sayfadan ana sayfaya dön

**Beklenen sonuç:**

* [ ] Bildirime tıklanınca ilgili iş detayına gidiliyor
* [ ] Detay açıldıktan sonra bildirim okunmuş sayılıyor
* [ ] Ana sayfadaki okunmamış bildirimlerden düşüyor

---

## E2E-04 — Ekip Oluşturma ve Üye Ekleme

**Amaç:** Head of Master’ın kendi lokasyonu için ekip oluşturabilmesini kontrol etmek.

* [ ] `hom1@workplan.local` ile giriş yap
* [ ] `Bana Atanan İşler` sayfasına git
* [ ] E2E-02’de oluşturulan `Atandı` durumlu kaydın `Detay` butonuna bas
* [ ] `İşi başlat` bölümünde `Yeni ekip oluştur` linkine tıkla
* [ ] `İsim` alanına `Manuel Test Ekibi` yaz
* [ ] `Oluştur` butonuna bas
* [ ] Onay modalini onayla
* [ ] Oluşan ekip kartında `Üye ekle` butonuna bas
* [ ] `Rol` seçimini `GeneralLabor` olarak bırak
* [ ] `Personel Ref.` alanına `TEST-001 - Manuel Kullanıcı` yaz
* [ ] `Ekle` butonuna bas
* [ ] Onay modalini onayla

**Beklenen sonuç:**

* [ ] Ekip listede görünüyor
* [ ] Eklenen üye ekip tablosuna geliyor

---

## E2E-05 — İşi Başlatma

**Amaç:** Atandı durumundaki planı InProgress durumuna geçirmek.

* [ ] E2E-02’de oluşturulan işin `İş Detayı` sayfasına dön
* [ ] `İşi başlat` bölümünde `Ekip` alanından `Manuel Test Ekibi` veya mevcut demo ekiplerden birini seç
* [ ] `Başlat` butonuna bas
* [ ] Onay modalini onayla

**Beklenen sonuç:**

* [ ] Başarılı mesaj görünüyor
* [ ] Durum `Devam Ediyor` oluyor
* [ ] `Gün sonu ilerleme gir` formu görünüyor
* [ ] `Durum Geçmişi` bölümünde `Atandı` → `Devam Ediyor` geçişi görünüyor

---

## E2E-06 — Gün Sonu İlerleme Girme

**Amaç:** InProgress iş için gerçekleşen değerleri girip Site Chief onayına göndermek.

* [ ] `Gün sonu ilerleme gir` formunda `Gerçek Miktar` alanına `10` yaz
* [ ] `Gerçek Adam-Gün` alanına `2.5` yaz
* [ ] `Fazla Mesai` alanına `1` yaz
* [ ] `Yorum` alanına `Manuel E2E gerçekleşme girişi` yaz
* [ ] `Kaydet ve Onaya Sun` butonuna bas
* [ ] Onay modalini onayla

**Beklenen sonuç:**

* [ ] Başarılı mesaj görünüyor
* [ ] Durum `Site Chief Onayında` oluyor
* [ ] Detayda şu bilgiler görünüyor:

  * [ ] Gerçekleşen miktar
  * [ ] Adam-gün
  * [ ] Fazla mesai
  * [ ] Yorum

---

## E2E-07 — Site Chief Onayı

**Amaç:** Site Chief’in kendi bölgesindeki Submitted işi onaylamasını kontrol etmek.

* [ ] `Çıkış yap`
* [ ] `sc1@workplan.local` ile giriş yap
* [ ] Ana sayfada `Onay Bekleyen İşlerim` bölümünde E2E-06’da gönderilen işi gör
* [ ] İş linkine veya `Beklenen Onaylar` → `Detay` akışına gir
* [ ] `Onay` bölümünde `Onayla` butonuna bas
* [ ] Onay modalini onayla

**Beklenen sonuç:**

* [ ] Durum `Saha Şefi Onayladı` oluyor
* [ ] `Durum Geçmişi` bölümünde Site Chief onayı görünüyor

---

## E2E-08 — Project Manager Son Onayı ve Raporlama

**Amaç:** PM’in son onayı vermesini ve işin raporlara düşmesini kontrol etmek.

* [ ] `Çıkış yap`
* [ ] `pm1@workplan.local` ile giriş yap
* [ ] Ana sayfada `Onay Bekleyen İşlerim` bölümünde E2E-07’de onaylanan işi gör
* [ ] İş detayına gir
* [ ] `Onayla` butonuna bas
* [ ] Modalde onayla
* [ ] Durumun `Tamamlandı` olduğunu doğrula
* [ ] `Raporlar` sayfasına git
* [ ] `Onaylanmış İşler` tablosunda tamamlanan işi gör
* [ ] `Yönetici İzleme` bölümünde `Ali Kaya` veya `hom1@workplan.local` seç
* [ ] Aynı işin ustabaşı bazlı listede de göründüğünü doğrula

**Beklenen sonuç:**

* [ ] PM onayından sonra iş `Tamamlandı` oluyor
* [ ] İş rapor ekranlarında görünüyor

---

## E2E-09 — Site Chief Red Akışı

**Amaç:** Site Chief reddedince işin Head of Master’a geri dönmesini kontrol etmek.

* [ ] E2E-02 ile yeni bir plan oluştur
* [ ] `hom1@workplan.local` ile plana gir
* [ ] Ekip seç
* [ ] `Başlat` butonuna bas
* [ ] Gün sonu formunda geçerli gerçekleşme değerleri gir
* [ ] İşi onaya sun
* [ ] `sc1@workplan.local` ile giriş yap
* [ ] İlgili işin detayında `Reddet` butonuna bas
* [ ] `Red nedeni` alanına `Metraj kontrolü bekleniyor` yaz
* [ ] `Reddi Onayla` butonuna bas
* [ ] Modalde onayla

**Beklenen sonuç:**

* [ ] İş durumu tekrar `Devam Ediyor` oluyor
* [ ] Yorum/geçmiş bölümünde red gerekçesi görünüyor
* [ ] `hom1@workplan.local` ile giriş yapıldığında iş yeniden ilerleme girilebilir durumda oluyor

---

## E2E-10 — PM Red Akışı

**Amaç:** PM reddedince Site Chief onaylı işin Head of Master’a geri dönmesini kontrol etmek.

* [ ] E2E-02 ile yeni bir plan oluştur
* [ ] HoM ile işi başlat
* [ ] Geçerli gerçekleşme gir
* [ ] Site Chief ile onayla
* [ ] `pm1@workplan.local` ile giriş yap
* [ ] İş detayında `Reddet` butonuna bas
* [ ] `Red nedeni` alanına `PM revizyon talebi` yaz
* [ ] `Reddi Onayla` butonuna bas
* [ ] Modalde onayla

**Beklenen sonuç:**

* [ ] İş durumu `Devam Ediyor` oluyor
* [ ] Red nedeni durum geçmişine yazılıyor
* [ ] HoM aynı iş için yeniden gün sonu ilerleme girebiliyor

---

## E2E-11 — İlerleme Validasyonları

**Amaç:** Gün sonu formundaki iş kurallarını elle kontrol etmek.

**Ön koşul:**

* [ ] `hom1@workplan.local` ile `Devam Ediyor` durumunda bir iş detayı açık

**Test adımları:**

* [ ] `Gerçek Miktar` alanına `5` yaz
* [ ] `Gerçek Adam-Gün` alanını boş bırak
* [ ] `Kaydet ve Onaya Sun` butonuna bas
* [ ] Gerçek miktar ve adam-günün birlikte girilmesi gerektiğini belirten hata mesajını doğrula
* [ ] Formu boşalt
* [ ] `Yorum` alanını da boş bırak
* [ ] Tekrar gönder
* [ ] İlerleme yoksa gerekçe yazılması gerektiğini belirten hata mesajını doğrula
* [ ] `Gerçek Miktar`, `Gerçek Adam-Gün` veya `Fazla Mesai` alanına negatif değer gir
* [ ] Formu gönder
* [ ] Negatif değer hatası alındığını doğrula
* [ ] Son olarak `Gerçek Miktar` ve `Gerçek Adam-Gün` alanlarını pozitif değerlerle gönder

**Beklenen sonuç:**

* [ ] Hatalı kombinasyonlar reddediliyor
* [ ] Geçerli kombinasyon `Site Chief Onayında` durumuna geçiyor

---

## E2E-12 — Plan Oluşturma Validasyonları

**Amaç:** Günlük plan formunun eksik/hatalı veriyle işlem yapmamasını kontrol etmek.

* [ ] `to1@workplan.local` ile `Günlük Plan Oluştur` sayfasına git
* [ ] Hiç alan doldurmadan `Oluştur` butonunun pasif olduğunu doğrula
* [ ] Proje seçmeden saha bölgesi/lokasyon seçimlerinin dolmadığını doğrula
* [ ] Yaprak iş kalemi seçmeden `Birim` alanının dolmadığını veya `-` kaldığını doğrula
* [ ] `Planlanan miktar` alanına negatif değer gir
* [ ] `Oluştur` butonunun pasif kaldığını doğrula
* [ ] `Planlanan adam-gün` alanına negatif değer gir
* [ ] `Oluştur` butonunun pasif kaldığını doğrula
* [ ] Tüm zorunlu alanları geçerli doldur
* [ ] Planın oluştuğunu doğrula

**Beklenen sonuç:**

* [ ] Eksik ya da negatif veriyle plan oluşturulamıyor

---

## E2E-13 — Yetki ve Scope Kontrolleri

**Amaç:** Doğru rolün bile sadece kendi kapsamındaki işleri görebildiğini/işleyebildiğini doğrulamak.

* [ ] `to1@workplan.local` ile A bölgesinde `hom1@workplan.local` üstüne bir plan oluştur
* [ ] `hom2@workplan.local` ile giriş yap
* [ ] `Bana Atanan İşler` sayfasında bu planın görünmediğini doğrula
* [ ] Plan detay URL’sini biliyorsan doğrudan açmayı dene
* [ ] İşe başlatma veya ilerleme gönderme işlemini yapamadığını doğrula
* [ ] `sc2@workplan.local` ile giriş yap
* [ ] `Beklenen Onaylar` sayfasında A bölgesine ait bu işin görünmediğini doğrula
* [ ] `pm1@workplan.local` ile giriş yap
* [ ] Site Chief onayı bekleyen bir işi PM’in kendi aşamasında olmadığı için onaylayamadığını doğrula

**Beklenen sonuç:**

* [ ] Kayıtlar rol ve kapsam filtresine göre görünüyor
* [ ] Yanlış aşamadaki onay denemeleri hata veriyor

---

## E2E-14 — Kullanıcı Yönetimi

**Amaç:** SystemAdmin’in kullanıcı oluşturma, rol güncelleme, aktiflik ve parola sıfırlama işlemlerini test etmek.

* [ ] `admin@workplan.local` ile giriş yap
* [ ] `Kullanıcı Yönetimi` sayfasına git
* [ ] `E-posta` alanına `manual.qa@workplan.local` yaz
* [ ] `Ad Soyad` alanına `Manual QA` yaz
* [ ] `Şifre` alanına `Manual123!` yaz
* [ ] `Roller` içinden `HeadOfMaster` seç
* [ ] `Oluştur` butonuna bas
* [ ] Onay modalini onayla
* [ ] Kullanıcının tabloda `Aktif` olarak göründüğünü doğrula
* [ ] `Rolleri Düzenle` ile `HeadOfMaster` yerine `TechnicalOfficeEngineer` seç
* [ ] `Kaydet` butonuna bas
* [ ] Onay modalini onayla
* [ ] `Pasif Yap` butonuna bas
* [ ] Onay modalini onayla
* [ ] `Şifre Sıfırla` butonuna bas
* [ ] Yeni şifre olarak `Manual456!` gir
* [ ] `Kaydet` butonuna bas
* [ ] Onay modalini onayla

**Beklenen sonuç:**

* [ ] Kullanıcı oluşturuluyor
* [ ] Rolleri güncelleniyor
* [ ] Kullanıcı pasif yapılabiliyor
* [ ] Parola sıfırlama başarılı mesajı veriyor

---

## E2E-15 — Master Data Duman Testi

**Amaç:** Proje, lokasyon ve iş kalemi ekranlarının listeleme ve temel gezinme açısından çalıştığını doğrulamak.

* [ ] `admin@workplan.local` veya `to1@workplan.local` ile giriş yap
* [ ] `Projeler` sayfasına git
* [ ] Demo projenin listede göründüğünü doğrula
* [ ] Proje detay/ilgili linklerden saha bölgeleri ve lokasyonlara git
* [ ] Aşağıdaki demo kayıtların göründüğünü doğrula:

  * [ ] `A Bölgesi`
  * [ ] `Türbin Binası`
  * [ ] `Türbin Binası - Zemin Kat`
* [ ] `İş Kalemi Tipleri` sayfasına git
* [ ] Aşağıdaki hiyerarşinin göründüğünü doğrula:

  * [ ] `Betonarme İşleri`
  * [ ] `Kalıp İşleri`
  * [ ] `Temel Kalıbı`

**Beklenen sonuç:**

* [ ] Master data ekranları yükleniyor
* [ ] Demo kayıtlar hiyerarşik ve okunabilir şekilde görünüyor

---

## E2E-16 — Mobil Görünüm Duman Testi

**Amaç:** Mobil saha akışının kullanılabilir olduğunu kontrol etmek.

* [ ] Tarayıcı geliştirici araçlarında mobil viewport aç
* [ ] `hom1@workplan.local` ile giriş yap
* [ ] Üstteki menü butonuyla menüyü aç/kapat
* [ ] `Bana Atanan İşler` sayfasına git
* [ ] Bir işin `Detay` sayfasına gir
* [ ] Ekip seçiminin taşmadan kullanılabildiğini doğrula
* [ ] `Başlat` butonunun taşmadan kullanılabildiğini doğrula
* [ ] Gün sonu formunun taşmadan kullanılabildiğini doğrula
* [ ] `Kaydet ve Onaya Sun` butonunun taşmadan kullanılabildiğini doğrula

**Beklenen sonuç:**

* [ ] Mobilde menü kullanılabilir
* [ ] Mobilde tablolar kullanılabilir
* [ ] Mobilde formlar kullanılabilir
* [ ] Kritik butonlar ekrandan taşmıyor

---

# Hızlı Regresyon Kontrol Listesi

## Auth

* [ ] Login başarısız denemede hata gösteriyor
* [ ] Login başarılı denemede ana sayfaya dönüyor
* [ ] Çıkış yap token’ları temizleyip login sayfasına götürüyor

## Bildirimler

* [ ] Bildirimler okunmamışken ana sayfada görünüyor
* [ ] Detay açılınca bildirim okunmuş oluyor

## DailyPlan Mutlu Yol

* [ ] `Atandı` durumuna geçiyor
* [ ] `Devam Ediyor` durumuna geçiyor
* [ ] `Site Chief Onayında` durumuna geçiyor
* [ ] `Saha Şefi Onayladı` durumuna geçiyor
* [ ] `Tamamlandı` durumuna geçiyor

## Red Akışları

* [ ] Site Chief red sonrası iş `Devam Ediyor` durumuna dönüyor
* [ ] PM red sonrası iş `Devam Ediyor` durumuna dönüyor
* [ ] Red nedeni durum geçmişinde görünüyor

## Raporlama

* [ ] Raporlarda sadece tamamlanan işler listeleniyor
* [ ] Ustabaşı bazlı listede tamamlanan işler görünüyor

## Yetki / Scope

* [ ] Yanlış rol kendi dışındaki kaydı göremiyor
* [ ] Yanlış rol kendi dışındaki kaydı işleyemiyor
* [ ] Yanlış onay aşamasındaki kullanıcı işlem yapamıyor

## Kullanıcı Yönetimi

* [ ] Kullanıcı oluşturulabiliyor
* [ ] Kullanıcı rolü güncellenebiliyor
* [ ] Kullanıcı pasif yapılabiliyor
* [ ] Parola sıfırlanabiliyor
* [ ] En az bir rol olmadan kaydetme yapılamıyor

## Tema

* [ ] Tema değiştirme butonu açık/koyu temayı değiştiriyor
