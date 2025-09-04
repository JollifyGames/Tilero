# Unity Mobil Oyun Geliştirme Kuralları

## Proje Özellikleri
1. **Oyun Perspektifi**: 2D ve tepeden görünüm (top-down)
2. **Animasyon Sistemi**: DoTween kullanılacak
3. **JSON İşlemleri**: Newtonsoft.Json kullanılacak
4. **Update Kullanımı**: Update ve benzeri metodlar (FixedUpdate, LateUpdate vb.) kullanmadan önce **mutlaka izin alınmalı**

## Önemli Notlar
- Update metodları performans açısından kritik olduğundan, kullanımdan önce izin istenmeli
- Alternatif çözümler (Coroutine, DoTween, Event sistemleri) öncelikli olarak değerlendirilmeli