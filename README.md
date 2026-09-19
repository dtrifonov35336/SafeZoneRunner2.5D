# Safe Zone Runner

2D / 2.5D mobile endless runner с элементами survival и постапокалиптическим сеттингом.

## 🎮 О проекте

Игрок выбирает персонажа и участвует в гонке за выживание, спасаясь от толпы зомби. 
Задача — пробраться через поток эвакуируемых, уворачиваться от препятствий и добраться до убежища.

**Жанр:** мобильный 2D endless runner / survival  
**Платформа:** Android (портретная ориентация)  
**Планируемые магазины:** RuStore, Яндекс Игры  
**Команда:** 1 разработчик  
**Движок:** Unity 6.3 LTS (6000.3.24f1)  
**Стиль:** semi-realistic 2D, grunge, post-apocalyptic

---

## 🎯 Игровой цикл

1. Игрок выбирает персонажа и снаряжение
2. Запускает забег: бежит от окраины города к убежищу
3. Уворачивается от препятствий на 2 полосах
4. Собирает монеты и сердечки
5. Сзади наступают зомби — задержка = смерть
6. Добирается до убежища → экран победы → ангар
7. Получает награды, прокачивает персонажа
8. Начинает следующий раунд

---

## 🛠️ Технологии и стек

| Компонент | Что используется |
| :--- | :--- |
| **Движок** | Unity 6.3 LTS |
| **Рендеринг** | URP 2D |
| **Ввод** | Input System (новый) |
| **UI** | TextMeshPro + SafeAreaFitter |
| **Анимации** | Animator + Sprite Swap |
| **Генерация ассетов** | ComfyUI (Flux Schnell) + PixelOrama |
| **ИИ-разработка** | Trae Desktop + Unity MCP + Cline |
| **Локальная LLM** | LM Studio (Qwen2.5-Coder-7B) |
| **Оплата** | RuStore Pay SDK |
| **Хранение прогресса** | PlayerPrefs + SaveSystem (локально) |

---

## 📁 Структура проекта
Assets/
├── Animations/ # Анимации бега, смерти, победы
├── Audio/ # Звуки и музыка (в процессе)
├── Editor/ # Скрипты редактора
├── Fonts/ # Шрифты (Roboto, Montserrat)
├── Materials/ # Материалы
├── Prefabs/ # Префабы
│ ├── Environment/ # SafeZone и декорации
│ ├── Obstacles/ # Препятствия (машины, барьеры)
│ ├── Pickups/ # Монеты, сердечки
│ └── UI/ # Карточки персонажей, префабы интерфейса
├── Resources/
│ └── Characters/ # Спрайты персонажей
├── Scenes/
│ ├── MainMenu.unity
│ ├── MainRoad.unity
│ ├── CharacterSelect.unity
│ ├── Equipment.unity
│ ├── Shop.unity
│ └── Hangar.unity
├── Scripts/
│ ├── Core/ # GameManager, SaveSystem
│ ├── Player/ # PlayerMovement2D, PlayerVisualController
│ ├── World/ # Parallax, RoadMarking, Dust
│ ├── Enemies/ # Zombie, Horde
│ ├── Crowd/ # NPC, CrowdManager
│ ├── Pickups/ # Pickup, PickupMover, PickupSpawner
│ ├── Hangar/ # Upgrade system
│ ├── Equipment/ # Equipment system
│ ├── Progression/ # ProfileManager, BonusCalculator
│ ├── UI/ # HUD, Toast, ProfileSettings
│ └── Monetization/ # RuStore provider
├── Sprites/
│ ├── Background/ # Фон (небо, город, лес)
│ ├── Obstacles/ # Спрайты препятствий
│ ├── Player/ # Спрайты персонажей
│ ├── Road/ # Дорога и разметка
│ └── SafeZone/ # Убежище
└── UI/
├── Icons/ # Иконки валют, задач
└── Menu/ # Логотип, кнопки

---

## ✅ Реализованные системы

### Геймплей
- [x] Движение игрока влево/вправо (3 полосы)
- [x] Прыжок с отключением коллайдера
- [x] Бесконечная дорога (UV-скролл / тайлы)
- [x] Разметка с очередью спавна
- [x] Препятствия с перспективным масштабированием
- [x] Пикапы (монеты, сердечки)
- [x] Система погони (ChaseDistance)
- [x] Толпа зомби (20+ иконок, анимация)
- [x] SafeZone (финиш забега)
- [x] Экран результатов (победа / поражение)

### Прогрессия
- [x] Профиль игрока (имя, аватар)
- [x] 6 персонажей с разными бонусами
- [x] Индивидуальная прокачка каждого персонажа
- [x] XP и уровни (пороговые бонусы: +5% скорость, +1 HP, +10% награда)
- [x] Ангар: 5 улучшений × 5 уровней
- [x] Снаряжение: 6 предметов × 5 уровней
- [x] Магазин: 4 набора для покупки

### Экономика и монетизация
- [x] Две валюты (жетоны / кристаллы)
- [x] Reward-система с бонусами
- [x] RuStore Pay SDK подключен
- [ ] Rewarded Ads (в планах)
- [ ] Внутриигровые покупки (тест цен: 59, 149, 299, 599, 999 ₽)

### UI / UX
- [x] SafeArea для телефонов с вырезом
- [x] Главное меню с навигацией
- [x] HUD (HP, валюты, дистанция, задача)
- [x] Экран настроек профиля (имя, аватар)
- [x] Toast-уведомления
- [x] Пауза и экран результатов

### Визуал и эффекты
- [x] Параллакс фона (3 слоя)
- [x] Частицы пыли (летят на игрока)
- [x] Тряска камеры при ударе
- [x] Покачивание игрока (bob)
- [x] Анимация бега (6 кадров на персонажа)
- [x] Анимация смерти (5 кадров)

---

## 🚧 В работе

- [ ] Звук (шаги, зомби, удары, музыка)
- [ ] Оптимизация под слабые Android-устройства
- [ ] Балансировка сложности
- [ ] Публикация в RuStore
- [ ] Сборка для Яндекс Игр
- [ ] Переход на 2.5D (в обсуждении)

---

## 📦 Сборка

### Требования
- Unity **6.3.24f1 LTS**
- Android Build Support (SDK, NDK, OpenJDK)
- Android 7.0+ (API 24)

### Сборка APK
1. Открой проект в Unity 6.3 LTS
2. `File → Build Profiles` → выбери **Android**
3. `Switch Platform` (если нужно)
4. Убедись, что сцены добавлены в **Scene List**:
   - `MainMenu` (индекс 0)
   - `MainRoad` (индекс 1)
   - `CharacterSelect` (индекс 2)
   - `Equipment`, `Shop`, `Hangar`
5. Нажми **Build** или **Build and Run**
6. Готовый APK появится в папке `Builds/`

### Player Settings
| Параметр | Значение |
| :--- | :--- |
| Company Name | DmitriyGames |
| Product Name | Safe Zone Runner |
| Package Name | com.dmitriygames.safezonerunner |
| Default Orientation | Portrait |
| Minimum API Level | Android 7.0 (API 24) |
| Target API Level | Automatic |
| Scripting Backend | IL2CPP |
| Target Architectures | ARM64, ARMv7 |

---

## 🤖 Использование локальной LLM для разработки

В проекте настроена связка:
- **Trae Desktop** — основная среда разработки с ИИ
- **Unity MCP Server** — мост между Trae и Unity
- **ComfyUI MCP** — генерация ассетов через Trae
- **LM Studio** — локальный сервер с моделью Qwen2.5-Coder-7B

### Настройка
1. Запусти LM Studio, загрузи модель, нажми `Start Server` (порт `1234`)
2. В Trae подключи MCP-серверы Unity и ComfyUI
3. Пиши задачу в чат — Trae сам создаст скрипты, объекты, импортирует ассеты

---

## 🎨 Генерация ассетов

### 2D-спрайты (дороги, фоны, персонажи)
- **ComfyUI + Flux Schnell** — основная генерация
- **PixelOrama** — ручная доработка
- **ChatGPT / Leonardo AI** — для отдельных идей

### Стиль проекта
Semi-realistic 2D game art, grunge, post-apocalyptic,
muted dark colors, no bright neon, dirty textures,
soft lighting, vertical composition for mobile.

---

## 📜 Лицензии и кредиты

- **Unity** — движок проекта
- **RuStore Pay SDK** — оплата в приложении
- **TextMeshPro** — UI-текст
- **NativeGallery** — доступ к галерее на Android
- **ComfyUI** — генерация ассетов
- **Flux Schnell** — модель генерации изображений

---

## 🎯 Цели

1. Рабочая игра, которая проходится без багов
2. Первый релиз в RuStore
3. 1 000+ установок
4. Улучшение retention (D1, D7)
5. Монетизация (Rewarded Ads + IAP)

---

## 📞 Контакты

**Разработчик:** Dmitriy Trifonov  
**GitHub:** [@dtrifonov35336](https://github.com/dtrifonov35336)

---

*Последнее обновление: 19.09.2026*