# Unity Mobil Oyun Geliştirme Kuralları

## Proje Özellikleri
1. **Oyun Perspektifi**: 2D ve tepeden görünüm (top-down)
2. **Animasyon Sistemi**: DoTween kullanılacak
3. **JSON İşlemleri**: Newtonsoft.Json kullanılacak
4. **Update Kullanımı**: Update ve benzeri metodlar (FixedUpdate, LateUpdate vb.) kullanmadan önce **mutlaka izin alınmalı**

## Önemli Notlar
- Update metodları performans açısından kritik olduğundan, kullanımdan önce izin istenmeli
- Alternatif çözümler (Coroutine, DoTween, Event sistemleri) öncelikli olarak değerlendirilmeli

## Proje Yapısı

### Core Sistemler

#### 1. DependencyManager
- Tüm manager'ları **sıralı** olarak initialize eder (artık paralel değil)
- Manager'lar IManager interface'ini implement eder
- Initialization Sırası:
  1. GridManager (bağımlılık yok)
  2. BoardManager (GridManager'a bağımlı)
  3. SlotManager (BoardManager'a bağımlı)
  4. EnemyManager (GridManager ve BoardManager'a bağımlı)
  5. WorldManager (EnemyManager'a bağımlı)

#### 2. GridManager
- 9x9 grid sistemi (inspector'dan ayarlanabilir)
- World Space pozisyonda grid oluşturur
- Her cell GridCell objesi tutar:
  - IsOccupied: Cell'de obje var mı
  - IsObstacle: Cell obstacle mı (yürünemez, knockback yapılamaz)
  - IsBorder: Cell border mı (yürünemez, knockback yapılabilir, düşen ölür)
- **Obstacle Sistemi**:
  - ObstaclesList: Inspector'dan Vector2Int listesi
  - ObstaclePrefab: Obstacle görseli
  - Yürünemez, knockback yapılamaz
- **Border Sistemi**:
  - BordersList: Inspector'dan Vector2Int listesi
  - BorderPrefab: Border görseli
  - Yürünemez, knockback yapılabilir
  - Border'a knockback edilen enemy instant ölür
- Preview sistemi için ShowPatternPreview/ClearPatternPreview metodları
- IsWalkable() metodu: Cell'in yürünebilir olup olmadığını kontrol eder

#### 3. BoardManager
- Grid üzerindeki objelerin hareketini yönetir
- Player spawn ve hareket kontrolü
- Enemy registration
- Grid cell occupation yönetimi
- Attack için enemy arama (FindEnemyInDirection)
- Obstacle ve Border kontrolü TryMoveObject'te yapılır

### Hareket ve Pattern Sistemi

#### 4. PlayerController
- **Hareket**: Düz linear hareket (jump animasyonu kaldırıldı)
- **Animasyon Sistemi**:
  - Animator component
  - visual Transform (scale flip için)
  - Triggers: Run, RunBack, Attack, Attack2
  - Parameters: IsWalking (bool), IsDefense (bool)
  - Sola giderken scale.x = -1, sağa giderken scale.x = 1
  - Yukarı (Y+) giderken RunBack, diğer yönlerde Run trigger
- Direction sistemi (Up, Down, Left, Right) - rotation yok
- Pattern execution sistemi
- Attack sistemi (PieceType'a göre damage multiplier)
- Defense buff sistemi

#### 5. SlotManager & MovementSlot
- 3 slot kartları gösterir
- **Energy Sistemi İle Çalışma**:
  - Kart kullanıldığında yeni kart ÇEKİLMEZ, slot boş kalır
  - Turn başında tüm slotlar RefreshAllSlots() ile yenilenir
  - Energy kontrolü yapılır, yetersizse kart kullanılamaz
- **Pattern Rotation**:
  - Her slot'ta Rotate Button (Inspector'dan bağlanır)
  - Tıklandığında pattern 90° döner (0°, 90°, 180°, 270°)
  - Görsel ve preview rotation'ı yansıtır
  - Pattern execution'da rotated hali kullanılır
- **MovementSlot UI**:
  - Cost Text (TMP): Kartın energy cost'u
  - Slot Background: Energy durumuna göre renk değişir
    - Beyaz: Kullanılabilir
    - Kırmızımsı: Energy yetersiz
    - Gri: Slot boş
- Hold to preview: Basılı tutunca grid'de pattern preview gösterir
- Click to execute: Tıklayınca pattern'i çalıştırır
- World Space Canvas'ta visualization (değerler /100)

#### 6. Pattern Sistemi
- **PatternSO**: ScriptableObject pattern tanımı
  - PatternName: Pattern adı
  - Description: Açıklama
  - **Cost**: Energy maliyeti (default: 2)
  - Steps listesi (position + PieceType)
  - PieceType: Player, Basic, Attack, Defense, Special
- **DeckSO**: Deck composition ScriptableObject
- **DeckService**: Kart çekme, karıştırma, discard pile yönetimi
- **RotatedPatternSO**: Runtime'da rotate edilmiş pattern'ler (cost'u da inherit eder)

### Combat Sistemi

#### 7. CharacterStats & CharacterModel
- **CharacterStats**: ScriptableObject stat tanımları (HP, Damage, Defense, Dodge, Crit)
- **CharacterModel**: Runtime stat yönetimi
  - Current HP tracking
  - **OnHpChanged Event**: HP değişimlerini bildirir
  - Damage/heal işlemleri
  - Temporary defense buff sistemi

#### 8. PlayerCharacter
- CharacterStats'tan model oluşturur
- PieceType damage multiplier'ları:
  - Basic: 1x
  - Attack: 2x
  - **Special: 2x + Knockback** (önceki 3x yerine)
  - Defense: +5 temporary defense (1 turn)
  - Player: Attack yapmaz

#### 9. EnemyCharacter
- CharacterStats ve model sistemi
- Death event sistemi
- ProcessTurn ile turn işleme
- HP ≤ 0 olunca ölür, grid'den temizlenir
- **Knockback Sistemi**:
  - ApplyKnockback(Direction) metodu
  - 1 cell geriye gider
  - Obstacle/occupied cell'e çarparsa +5 damage (wallCollisionDamage)
  - Border'a düşerse instant death
  - Map dışına çıkamazsa +5 damage
- **EnemyHpView**: HP bar gösterimi
  - Fill Image animasyonlu güncelleme
  - HP < Max olduğunda görünür
  - HP full olduğunda gizlenir

#### 10. EnemyMovement
- **Hareket**: Düz linear hareket (jump animasyonu kaldırıldı)
- **Animasyon Sistemi**:
  - Animator component
  - visual Transform (scale flip için)
  - Triggers: Run, RunBack
  - Parameter: IsWalking (bool)
  - Defense ve Attack trigger'ları YOK
- Turn'de hareket:
  1. 4 yönde player arar
  2. Player bulursa ona döner (hareket etmez)
  3. Bulamazsa random boş cell'e gider
- Movement range inspector'dan ayarlanabilir (default: 1)
- Obstacle ve Border'a yürüyemez

### Turn ve Energy Sistemi

#### 11. WorldManager
- Turn state yönetimi (PlayerTurn → EnemyTurn → PlayerTurn)
- **Energy Sistemi**:
  - playerEnergyBase: 4 (Inspector'dan ayarlanabilir)
  - currentPlayerEnergy: Mevcut energy
  - Turn başında energy resetlenir
  - SpendEnergy(cost): Energy harcar
  - Energy 0 olunca otomatik turn geçer
  - Oynanabilir kart kalmazsa otomatik turn geçer
  - EndPlayerTurn(): Manuel turn bitirme
- Player turn başında:
  - Energy resetlenir (base değere)
  - Tüm slotlar yenilenir (RefreshAllSlots)
  - Defense buff resetlenir

#### 12. EnemyManager
- Enemy spawn sistemi (prefab + grid position)
- Active enemy listesi
- Death event dinleme
- ProcessAllEnemyTurns ile sırayla enemy turn'leri işler

### UI Sistemleri

#### 13. EnergyView
- TMP Text ile current energy gösterimi
- Format: "current/max" veya sadece "current"
- Energy seviyesine göre renk değişimi
- **End Turn Button**: 
  - Manuel turn bitirme
  - Sadece player turn'ünde aktif
  - Enemy turn'de inaktif

### Oyun Akışı

1. **Player Turn Başlangıcı:**
   - Energy resetlenir (4)
   - Tüm slotlar yeni kartlarla doldurulur
   - Defense buff resetlenir

2. **Kart Kullanımı:**
   - Energy kontrolü yapılır
   - Cost kadar energy harcanır
   - Slot boşalır (yeni kart çekilmez)
   - Pattern'e göre hareket
   - PieceType'a göre aksiyon:
     - Basic: 1x damage
     - Attack: 2x damage
     - Special: 2x damage + knockback
     - Defense: +5 temporary defense
   
3. **Turn Geçişi:**
   - Energy 0 olunca otomatik
   - Oynanabilir kart kalmazsa otomatik
   - End Turn butonu ile manuel
   - Energy > 0 iken birden fazla kart oynanabilir

4. **Enemy Turn:**
   - Her enemy sırayla ProcessTurn yapar
   - Player'ı arar, bulamazsa random hareket
   - Obstacle ve Border'a gidemez

5. **Combat Özellikleri:**
   - Special attack knockback yapar
   - Knockback'te duvara çarpma = +5 damage
   - Border'a düşme = instant death
   - Enemy HP bar'ı damage aldıkça güncellenir

### Inspector Ayarları

- **GridManager**: 
  - Grid size, world position, cell size
  - ObstaclesList + ObstaclePrefab
  - BordersList + BorderPrefab
  - Preview prefab
- **BoardManager**: Player start cell, start direction, player prefab
- **SlotManager**: Movement slots array, deck SO
- **MovementSlot**: 
  - Cost Text (TMP)
  - Slot Background (Image)
  - Rotate Button
- **PlayerController**:
  - Animator
  - Visual (Transform)
- **PlayerCharacter**: CharacterStats, damage multipliers, defense bonus
- **EnemyCharacter**: 
  - CharacterStats
  - Knockback duration, wall collision damage
- **EnemyHpView**:
  - Fill Image
  - HP Canvas
  - Hide when full option
- **EnemyMovement**: 
  - Movement range
  - Animator
  - Visual (Transform)
- **WorldManager**: 
  - Initial turn state
  - Player energy base (default: 4)
- **EnemyManager**: Enemy spawn list (prefab + position)
- **EnergyView**:
  - Energy Text (TMP)
  - End Turn Button
  - Display format