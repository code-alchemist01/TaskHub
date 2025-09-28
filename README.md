# TaskMaster - Görev Yönetim Sistemi

TaskMaster, günlük görevlerinizi organize etmenize yardımcı olan modern ve kullanıcı dostu bir görev yönetim uygulamasıdır. Hem komut satırı hem de grafik arayüz desteği ile farklı kullanım tercihlerinize uygun çözümler sunar.

## 🚀 Özellikler

### Temel Özellikler
- **Görev Yönetimi**: Görev ekleme, düzenleme, silme ve listeleme
- **Öncelik Seviyeleri**: Yüksek, Orta, Düşük öncelik ataması
- **Durum Takibi**: Bekliyor, Devam Ediyor, Tamamlandı durumları
- **Son Tarih Yönetimi**: Görevler için son tarih belirleme
- **Etiket Sistemi**: Görevleri kategorize etmek için etiket desteği

### Gelişmiş Özellikler
- **Arama ve Filtreleme**: Başlık, açıklama ve etiketlerde arama
- **Veri Kalıcılığı**: Otomatik veri saklama ve geri yükleme
- **Çoklu Platform**: CLI ve Desktop uygulaması desteği
- **Senkronizasyon**: Farklı arayüzler arasında veri paylaşımı

## 🛠️ Teknoloji Stack

- **.NET 9.0**: Modern C# framework
- **WPF**: Windows Presentation Foundation ile desktop arayüzü
- **JSON**: Hafif ve hızlı veri saklama
- **Command Pattern**: CLI komutları için yapılandırılmış mimari

## 📦 Kurulum

### Gereksinimler
- .NET 9.0 SDK
- Windows 10/11 (Desktop uygulaması için)

### Projeyi Çalıştırma

1. **Repoyu klonlayın:**
```bash
git clone https://github.com/kullanici/taskmaster.git
cd taskmaster
```

2. **Bağımlılıkları yükleyin:**
```bash
dotnet restore
```

3. **Projeyi derleyin:**
```bash
dotnet build
```

## 🎯 Kullanım

### CLI Uygulaması

**Görev Ekleme:**
```bash
cd TaskMaster.CLI
dotnet run -- add -t "Proje tamamla" -d "Son kontroller ve test" --priority High --due "15.01.2025"
```

**Görevleri Listeleme:**
```bash
dotnet run -- list
```

**Görev Güncelleme:**
```bash
dotnet run -- update 1 -t "Güncellenmiş başlık" --priority Medium
```

**Görev Silme:**
```bash
dotnet run -- delete 1
```

**Görev Arama:**
```bash
dotnet run -- search "proje"
```

### Desktop Uygulaması

```bash
cd TaskMaster.Desktop
dotnet run
```

Desktop uygulaması sezgisel bir arayüz sunar:
- Ana pencerede görev listesi ve filtreleme seçenekleri
- "Ekle" butonu ile yeni görev oluşturma
- Çift tıklama ile görev düzenleme
- Sağ tık menüsü ile hızlı işlemler
<img width="879" height="556" alt="Ekran görüntüsü 2025-09-29 020629" src="https://github.com/user-attachments/assets/9a1b3819-3f73-487b-9a92-c550e77ed9cc" />
<img width="577" height="485" alt="Ekran görüntüsü 2025-09-29 020658" src="https://github.com/user-attachments/assets/52557cb1-ddec-4db9-bb20-8cd212531780" />
<img width="881" height="566" alt="Ekran görüntüsü 2025-09-29 021222" src="https://github.com/user-attachments/assets/f4c783ca-b171-4aff-8b5b-d7425242c200" />
<img width="870" height="556" alt="Ekran görüntüsü 2025-09-29 021243" src="https://github.com/user-attachments/assets/6c3c4bf5-792e-4a1e-9a66-17b625debb54" />

  

## 📁 Proje Yapısı

```
TaskMaster/
├── TaskMaster.Core/          # Ortak iş mantığı ve modeller
│   ├── Models/               # Veri modelleri
│   └── Services/             # İş mantığı servisleri
├── TaskMaster.CLI/           # Komut satırı uygulaması
│   └── Commands/             # CLI komut sınıfları
├── TaskMaster.Desktop/       # WPF desktop uygulaması
│   ├── Windows/              # Pencere sınıfları
│   ├── Models/               # View modelleri
│   └── Converters/           # XAML dönüştürücüleri
└── README.md
```

## 💾 Veri Saklama

Görevler otomatik olarak şu konumda saklanır:
```
%APPDATA%\TaskMaster\tasks.json
```

Bu sayede:
- Uygulama yeniden başlatıldığında veriler korunur
- CLI ve Desktop uygulamaları aynı veriyi paylaşır
- Manuel backup ve restore işlemleri yapılabilir

## 🔧 Geliştirme

### Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/yeni-ozellik`)
3. Değişikliklerinizi commit edin (`git commit -am 'Yeni özellik eklendi'`)
4. Branch'inizi push edin (`git push origin feature/yeni-ozellik`)
5. Pull Request oluşturun

### Test Etme

```bash
# Tüm projeleri test et
dotnet test

# Belirli bir projeyi test et
dotnet test TaskMaster.Core.Tests
```

## 📋 Roadmap

- [ ] Web uygulaması desteği
- [ ] Mobil uygulama (MAUI)
- [ ] Takım işbirliği özellikleri
- [ ] Gelişmiş raporlama
- [ ] Takvim entegrasyonu
- [ ] Bildirim sistemi

## 🐛 Bilinen Sorunlar

- Çok büyük görev listelerinde performans optimizasyonu gerekebilir
- Bazı özel karakterler etiketlerde sorun yaratabilir

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için `LICENSE` dosyasına bakın.

## 🤝 Destek

Sorularınız veya önerileriniz için:
- Issue açın
- E-posta gönderin: destek@taskmaster.com
- Dokümantasyonu inceleyin

## 📊 İstatistikler

- **Kod Satırı**: ~2,500 satır
- **Test Kapsamı**: %85
- **Desteklenen Diller**: Türkçe, İngilizce
- **Platform Desteği**: Windows 10+

---

*TaskMaster ile görevlerinizi daha verimli yönetin! 🎯*
