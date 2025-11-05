# Start Menu Kurulum Rehberi

Bu belge, `StartMenu` sahnesinde çalışacak ana menüyü Unity editöründe kendiniz oluştururken hangi bileşenleri nereye yerleştirmeniz gerektiğini adım adım anlatır. Aşağıdaki adımlar mevcut `MainMenuUI`, `LevelButton`, `AudioSettingsManager` ve `AudioSettingsView` script'lerinin beklediği düzeni temel alır.

## 1. Sahne Hazırlığı
1. **Yeni sahne oluşturun** ve `StartMenu` olarak kaydedin (veya mevcut sahneyi açın).
2. Sahneye yalnızca tek bir **Main Camera** bırakın. Ekstra script eklemenize gerek yok.
3. Menü etkileşimi için sahneye bir **EventSystem** ekleyin (`GameObject ▸ UI ▸ Event System`).

## 2. Canvas ve Ana Menü Kökü
1. `GameObject ▸ UI ▸ Canvas` komutuyla bir Canvas oluşturun ve adını `Main Menu Canvas` yapın.
2. Canvas üzerinde şu bileşenlerin olduğundan emin olun:
   - `Canvas` (Render Mode: *Screen Space - Overlay*)
   - `Canvas Scaler` (UI Scale Mode: *Scale With Screen Size*, Reference Resolution: *1920x1080*)
   - `Graphic Raycaster`
3. Aynı GameObject'e `MainMenuUI` script'ini ekleyin. Bu script sahnedeki panelleri yönetir.

> **İpucu:** `MainMenuUI` bileşeni, alt nesnelerin adlarına bakarak ("MainPanel", "LevelPanel", "SettingsPanel") referansları otomatik olarak bulur. Farklı isimler kullanırsanız referansları Inspector üzerinden elle atayın.

## 3. Ana Panel (`MainPanel`)
1. Canvas altında `MainPanel` adında boş bir UI nesnesi (Empty Object) oluşturun.
2. `Image`, `Vertical Layout Group` ve `Content Size Fitter` bileşenlerini ekleyerek panelin arka planını ve düzenini ayarlayın.
3. Panelin içinde aşağıdaki öğeleri oluşturun:
   - Üstte bir `Text` bileşeni (örn. "Oyuna Hoş Geldiniz").
   - `Start`, `Ayarlar` ve `Çıkış` adlarında üç adet `Button`.
4. `MainMenuUI` bileşeninde yer alan `Start Button`, `Settings Button` ve `Exit Button` alanları otomatik dolmuyorsa ilgili butonları Inspector'dan sürükleyip bırakın.

## 4. Seviye Paneli (`LevelPanel`)
1. Canvas altında `LevelPanel` adında ikinci bir panel oluşturun (MainPanel'deki bileşenlerin aynısını kullanabilirsiniz).
2. Panel içine şu öğeleri yerleştirin:
   - Bir `Text` bileşeni (örn. "Seviye Seç").
   - Her seviye için bir `Button`. Butonların üzerine `LevelButton` script'ini ekleyin ve `Scene Name` alanına yüklenecek sahnenin adını yazın (örn. `level1`).
   - En altta `Back` veya `Geri` adlı bir `Button`.
3. `LevelPanel` GameObject'ine `LevelButton` script'i eklenmiş butonlar `MainMenuUI` tarafından otomatik algılanır. Gerekirse Inspector'da `Level Button Back` alanına "Geri" butonunu bağlayın.

## 5. Ayarlar Paneli (`SettingsPanel`)
1. Canvas altında `SettingsPanel` adında üçüncü bir panel oluşturun.
2. Panelin GameObject'ine `AudioSettingsView` script'ini ekleyin.
3. Panel içine aşağıdaki düzeni kurun:
   - Bir `Text` bileşeni (örn. "Ses Ayarları").
   - Her biri bir `Slider` içeren üç ayrı grup:
     - `Ana Ses` (Master)
     - `Müzik` (Music)
     - `SFX`
   - Her slider'ın yanında/altında yüzde değerini gösterecek bir `Text` oluşturun. `AudioSettingsView` slider'a göre otomatik olarak eşleşmesi için `Slider` GameObject'inin adını "Ana Ses Slider" gibi, değer metnini de "Ana Ses Value" şeklinde adlandırın.
   - En altta `Geri` adlı bir `Button`.
4. Inspector'da `AudioSettingsView` üzerindeki `Master Slider`, `Music Slider`, `Sfx Slider` alanlarına ilgili slider'ları; değer etiketleri otomatik bulunmazsa `Master Value Label`, `Music Value Label`, `Sfx Value Label` alanlarına ilgili `Text` bileşenlerini atayın.

## 6. Ses Yöneticisi
1. Sahne hiyerarşisinde boş bir GameObject oluşturun ve adını `Audio Settings Manager` yapın.
2. Üzerine `AudioSettingsManager` script'ini ekleyin.
3. Varsayılan ses seviyeleri Inspector'dan ayarlanabilir. Bu bileşen `DontDestroyOnLoad` olduğu için oyunun geri kalanında da çalışır.

## 7. Panel Varsayılan Durumları
- Oyunu başlatmadan önce `MainPanel` aktif (`SetActive(true)`), `LevelPanel` ve `SettingsPanel` pasif (`SetActive(false)`) olmalıdır.
- `MainMenuUI` script'i çalışırken bu düzeni otomatik olarak uygular; sahneyi düzenlerken de bu durumu korumanız önerilir.

## 8. Test
1. Oyunu `StartMenu` sahnesinden çalıştırın.
2. `Start` butonu seviyeler panelini göstermeli, seviye butonları doğru sahne adlarını yüklemeli.
3. `Ayarlar` butonu ses ayarları panelini açmalı, `Geri` butonları ana menüye dönmelidir.
4. `Çıkış` butonu editörde Play modunu durdurur, build'de uygulamayı kapatır.

Bu adımları izleyerek ana menüyü tamamen editör üzerinden kurabilir, gerekli tüm script'leri doğru nesnelere bağlayabilirsiniz.
