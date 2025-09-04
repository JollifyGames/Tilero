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
- Tüm manager'ları sırayla initialize eder
- Manager'lar IManager interface'ini implement eder
- Sıra: GridManager → BoardManager → SlotManager → EnemyManager → WorldManager

#### 2. GridManager
- 9x9 grid sistemi (inspector'dan ayarlanabilir)
- World Space pozisyonda grid oluşturur
- Her cell GridCell objesi tutar (occupation durumu, obje referansı)
- Preview sistemi için ShowPatternPreview/ClearPatternPreview metodları

#### 3. BoardManager
- Grid üzerindeki objelerin hareketini yönetir
- Player spawn ve hareket kontrolü
- Enemy registration
- Grid cell occupation yönetimi
- Attack için enemy arama (FindEnemyInDirection)

### Hareket ve Pattern Sistemi

#### 4. PlayerController
- Jump animasyonu ile grid-based hareket
- Direction sistemi (Up, Down, Left, Right)
- Pattern execution sistemi
- Attack sistemi (PieceType'a göre damage multiplier)
- Defense buff sistemi

#### 5. SlotManager & MovementSlot
- 3 slot kartları gösterir
- Hold to preview: Basılı tutunca grid'de pattern preview gösterir
- Click to execute: Tıklayınca pattern'i çalıştırır
- World Space Canvas'ta visualization (değerler /100)
- Slot kullanıldıktan sonra otomatik yeni kart çeker

#### 6. Pattern Sistemi
- **PatternSO**: ScriptableObject pattern tanımı
  - Steps listesi (position + PieceType)
  - PieceType: Player, Basic, Attack, Defense, Special
- **DeckSO**: Deck composition ScriptableObject
- **DeckService**: Kart çekme, karıştırma, discard pile yönetimi
  - Her kartın 4 rotasyonu otomatik eklenir (0°, 90°, 180°, 270°)
- **RotatedPatternSO**: Runtime'da rotate edilmiş pattern'ler

### Combat Sistemi

#### 7. CharacterStats & CharacterModel
- **CharacterStats**: ScriptableObject stat tanımları (HP, Damage, Defense, Dodge, Crit)
- **CharacterModel**: Runtime stat yönetimi
  - Current HP tracking
  - Damage/heal işlemleri
  - Temporary defense buff sistemi

#### 8. PlayerCharacter
- CharacterStats'tan model oluşturur
- PieceType damage multiplier'ları:
  - Basic: 1x
  - Attack: 2x
  - Special: 3x
  - Defense: +5 temporary defense (1 turn)
  - Player: Attack yapmaz

#### 9. EnemyCharacter
- CharacterStats ve model sistemi
- Death event sistemi
- ProcessTurn ile turn işleme
- HP ≤ 0 olunca ölür, grid'den temizlenir

#### 10. EnemyMovement
- Direction ve rotation sistemi (player gibi)
- Turn'de hareket:
  1. 4 yönde player arar
  2. Player bulursa ona döner (hareket etmez)
  3. Bulamazsa random boş cell'e gider
- Movement range inspector'dan ayarlanabilir (default: 1)
- Grid güncelleme (eski pozisyon temizlenir, yeni pozisyona register)

### Turn Sistemi

#### 11. WorldManager
- Turn state yönetimi (PlayerTurn → EnemyTurn → PlayerTurn)
- Player action complete → Enemy turn başlar
- Tüm enemy'ler ProcessTurn yapar → Player turn başlar
- Player turn başında defense buff resetlenir

#### 12. EnemyManager
- Enemy spawn sistemi (prefab + grid position)
- Active enemy listesi
- Death event dinleme
- ProcessAllEnemyTurns ile sırayla enemy turn'leri işler

### Oyun Akışı

1. **Player Turn:**
   - Slot'lardan kart seçer
   - Pattern'e göre hareket eder
   - Durduğu PieceType'a göre:
     - Basic/Attack/Special: Çevrede enemy varsa attack
     - Defense: +5 temporary defense
   - Turn biter

2. **Enemy Turn:**
   - Her enemy sırayla ProcessTurn yapar
   - Player'ı arar, bulamazsa random hareket
   - Turn biter, player defense resetlenir

3. **Combat:**
   - Player attack damage = base damage × PieceType multiplier
   - Enemy ölünce: Event → Grid temizleme → 0.25s delay ile destroy

### Inspector Ayarları

- **GridManager**: Grid size, world position, cell size, preview prefab
- **BoardManager**: Player start cell, start direction, player prefab
- **SlotManager**: Movement slots array, deck SO
- **PlayerCharacter**: CharacterStats, damage multipliers, defense bonus
- **EnemyCharacter**: CharacterStats
- **EnemyMovement**: Movement range, animation settings
- **WorldManager**: Initial turn state
- **EnemyManager**: Enemy spawn list (prefab + position)