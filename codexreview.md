# Codex Review — 2175rework (Unity 6 / URP 2D)

Bu dosya, projenin Scripts ve ilgili oyun akışı bileşenlerini hızlı fakat kapsamlı bir gözden geçirmenin özetidir. Odak: karakter kontrolü, FSM, giriş akışı, etkileşim, çift karakter sistemi, kamera ve UI/kayıt.

## Genel Proje Bilgisi
- Unity sürümü: 6000.0.59f2 (Unity 6)
- Render Pipeline: URP (Assets/Settings/UniversalRP.asset, Renderer2D.asset)
- Önemli paketler: URP 17.0.4, Input System 1.14.2, Cinemachine 3.1.4, UGUI, Timeline, Visual Scripting
- Build sahneleri: BaslangicMenu, level1, LevelCengo2

## Mimari Akış (Oynanış)
- Girdi: `InputAdapter` → `PlayerOrchestrator.Update()` içinde `Collect()`
- Algılama: `Sensors2D.Sample()` zemini/duvarı/merdiveni/etkileşimi örnekler
- Yetenek/Hareket: `AbilityController.Tick()` sıralı olarak WallClimb → WallSlide → WallJump → Climb → Jump → Interact
- Fizik: `LocomotionMotor2D.PhysicsStep()` `FixedUpdate` içinde çağrılır
- Durum Makinesi: `PlayerStateMachine` (Idle/Run/JumpRise/JumpFall/WallSlide/WallClimb/Climb) + geçiş kuralları ve öncelik sistemi
- Animasyon: `AnimationStateSync` FSM olaylarını Animator’a yansıtır ve flip/facing çözer
- Çift Karakter: `DualCharacterController` split/merge, aktif karakter girişi ve animasyon tetikleme
- Kamera: `CinemachinePartySwitcher` aktif karakter ve merge durumuna göre vcam öncelikleri ve kamera profilleri
- UI/Kayıt: `MainMenuUI`, `LevelButton`, `LevelProgressSaveManager`, `AudioSettingsManager`

## Girdi (Input) ve Yürütme Sırası
- `InputAdapter` edge state’leri yönetiyor (JumpPressed vb.). `PlayerOrchestrator` opsiyonel olarak `autoClearInput` ile frame sonunda temizliyor.
- `DualCharacterController` her frame sonunda hem Elior hem Sim için `ClearFrameEdges()` çağırıyor.
- `FlashlightController` de kendi `InputAdapter`’ında `Collect()` ve `ClearFrameEdges()` çağırıyor.
  - Not: Aynı frame içinde birden fazla bileşen `ClearFrameEdges()` çağırdığında edge event’lerin beklenenden önce “kaybolma” riski doğar. Tek bir toplayıcı/temizleyici (genellikle `PlayerOrchestrator`) kullanmak daha güvenli.
  - Alternatif: `FlashlightController` doğrudan `InputActionReference` ile kendi aksiyonunu abonelik (performed/canceled) olarak dinleyebilir; böylece `InputAdapter` kenar durumlarını etkilemez.
- `InputAdapter` içindeki MoveX mantığı: çapraz hareketlerde hız düşümünü önlemek için X’i ±1’e quantize ediyor. Analog hız istenirse bu davranış opsiyonelleştirilebilir.

## Hareket Motoru (Locomotion)
- `LocomotionMotor2D` hedef hız takibi, yer/hava kontrol oranı, değişken zıplama (hold/cut), duvar kayışı sınırı ve facing mantığını yönetiyor.
- Unity 6’da `Rigidbody2D.velocity` yaygın kullanımdır; projede `rb.linearVelocity` kullanılıyor. Derleme/oynatma hatası alırsanız `linearVelocity` → `velocity` değişimi gerekebilir.
- Facing çözümü: Yatay hız deadzone üstünde velocity yönü; değilse WallSlide/WallClimb durumunda duvar tarafına bak; aksi halde önceki facing korunur.

## Duvar/Tırmanma/Zıplama
- `WallMovement`: Havadayken ve duvara doğru input varken WallSlide’a geçiş isteği yapar.
- `WallClimbController`: Climbable maskesi ve MoveY ile duvar tırmanışı; çıkış koşulları config ile yönetilir.
- `WallJumpController`: Sadece WallSlide durumunda, `Jumpable` maskelerine bakarak belirlenen açı/itki ile zıplar ve JumpRise’a geçer.
- `ClimbController`: Merdiven alanında MoveY ile Climb durumuna girer; Jump ile çıkış yapabilir.
- `PlayerStateHooks`: Grounded/NoWall gibi koşulları izleyip state’i normalize eder (Idle/Run <-> JumpRise/Fall vb.).

## Etkileşim Sistemi
- `Sensors2D` “en iyi” `Interactable`’ı (öncelik, mesafe) seçer ve `InteractionController` fokus/hold/cooldown ve hareket kilidi yönetimini yapar.
- `Interactable` türleri: Tap, Hold, Toggle, Panel; event tabanlı bildirimlerle sahne mantığı tetiklenir.

## Çift Karakter (Split/Merge) ve Fener
- `DualCharacterController` aktif karakteri değiştirir, split’te güvenli spawn arar, merge/split animasyon tetikler ve giriş yetkisini aktif karaktere verir.
- `FlashlightController` `IActiveCharacterGate` desteği ile yalnızca aktif karakter kontrolündeyken çalışacak şekilde yapılandırılabilir; fareye göre pozisyon ve rotasyon günceller.
- Input sırası uyarısı burada da geçerli: `FlashlightController` kendi `InputAdapter`’ında edge temizliği yaptığı için diğer sistemlerle yarışabilir.

## Kamera (Cinemachine 3)
- `CinemachinePartySwitcher` vcam’leri otomatik bulup Follow hedeflerini atıyor, profil (FOV + pozisyon offset) uyguluyor ve öncelikleri durumlara göre ayarlıyor.
- Not: VCam transform konumunu doğrudan ayarlamak (Transposer/Framing yerine) Cinemachine’in izleme davranışını by‑pass edebilir. Gerekirse `CinemachinePositionComposer/Transposer` ayarları üzerinden offset/damping yapılması daha stabil olur.

## Diyalog Sistemi
- `DialogueManager` singleton; InputAction ile ilerleme, yazım hızı, konuşmacı kilidi, UI entegrasyonu ve event’ler mevcut.
- `DialogueUI` TMP ile yazma animasyonu ve tamamlandığında event yükseltme içerir.
- `DialogueTrigger` collider üzerinden başlatır; gerekirse konuşma bitene kadar karakter girişini kısıtlar.

## UI ve Kayıt
- `MainMenuUI` panel geçişleri, seviye yükleme, çıkış; çocuk `LevelButton` bileşenleriyle entegre.
- `LevelProgressSaveManager` PlayerPrefs üzerinden unlock/completed takibi; isteğe göre ilk seviyeyi otomatik açma.
- `AudioSettingsManager` master/music/sfx volümünün global kontrolü, kaynak kayıt/uygulama ve persist.

## Gözlenen İyileştirme Alanları
1) Input yaşam döngüsü tekleştirilmeli
- Risk: Birden fazla yerde `ClearFrameEdges()` çağrısı edge event’leri erken temizleyebilir.
- Öneri: Frame akışı: `Collect()` → tüm `Tick()`ler → tek bir noktada `ClearFrameEdges()` (tercihen `PlayerOrchestrator`). `FlashlightController` kendi aksiyonunu doğrudan `InputActionReference` ile dinleyebilir.

2) Rigidbody2D API uyumluluğu
- Hata olasılığına karşı `linearVelocity` yerine `velocity` kullanımı değerlendirilmelidir.

3) Analog yürüyüş seçeneği
- `InputAdapter` MoveX’i ±1’e zorluyor. İstenirse “analog yatay kontrol” toggle’ı eklenip nicelendirme devre dışı bırakılabilir.

4) Cinemachine profil uygulaması
- VCam konumunu transform ile taşımak yerine Cinemachine component’leri (Transposer/Composer) üstünden offset/damping ayarı daha iyi sonuç verir.

5) Türkçe karakter/encoding
- Bazı yorumlarda encoding bozulmaları görünüyor. Kaynak dosyalar UTF‑8 tutulup editörde de aynı standardı zorlamak iyi olur (örn. `.editorconfig`).

## Önerilen Sonraki Adımlar
- Input tekleştirme için küçük bir refactor (Flashlight’ın InputAction’a doğrudan abonelik veya edge temizlemeyi kaldırma).
- `Rigidbody2D.linearVelocity` kullanım noktalarını hızlı bir tarama ile doğrulayıp gerekirse `velocity`’e geçiş.
- Cinemachine ayarlarını sahne özelinde düzenlemek (profile offset → Transposer/Composer parametreleri).
- `.editorconfig` ekleyip UTF‑8 ve EOL’ları standardize etmek.

> Not: Bu inceleme, Assets/Scripts altındaki tüm C# script’leri, giriş aksiyonlarını ve temel ProjectSettings/URP yapılandırmasını tarayarak hazırlanmıştır.

