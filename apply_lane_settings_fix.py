
#!/usr/bin/env python3
"""Один раз переводит координаты полос на общую настройку Inspector."""
from pathlib import Path
import re
import shutil
import sys

ROOT = Path(__file__).resolve().parent

FILES = {
    "settings": ROOT / "Assets/Scripts/3D/World/RunnerLaneSettings3D.cs",
    "player": ROOT / "Assets/Scripts/3D/Player/PlayerMovement3D.cs",
    "obstacles": ROOT / "Assets/Scripts/3D/World/ObstacleSpawner3D.cs",
    "pickups": ROOT / "Assets/Scripts/3D/Pickups/PickupSpawner3D.cs",
    "rescued": ROOT / "Assets/Scripts/3D/Rescued/RescuedPersonSpawner3D.cs",
    "scene": ROOT / "Assets/Scenes/MainRoad.unity",
}

SETTINGS_SOURCE = '''using UnityEngine;

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public class RunnerLaneSettings3D : MonoBehaviour
{
    public static RunnerLaneSettings3D Instance { get; private set; }

    [Header("Общие полосы движения — настройка для всей игры")]
    [Tooltip("Смещение левой полосы относительно центра дороги. Обычно -1.")]
    public float leftLaneOffset = -1f;

    [Tooltip("Смещение правой полосы относительно центра дороги. Обычно 1.")]
    public float rightLaneOffset = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "[RunnerLaneSettings3D] В сцене должен быть только один компонент настроек полос.",
                this
            );
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private static RunnerLaneSettings3D GetSettings()
    {
        if (Instance == null)
            Instance = Object.FindFirstObjectByType<RunnerLaneSettings3D>();

        return Instance;
    }

    public static float GetCenterX()
    {
        GameObject centerObject = GameObject.Find("RoadCenterMarking");

        if (centerObject != null)
            return centerObject.transform.position.x;

        RoadDashedLine line = Object.FindFirstObjectByType<RoadDashedLine>();

        if (line != null)
            return line.transform.position.x;

        return 0f;
    }

    public static float GetLaneX(int index)
    {
        float centerX = GetCenterX();
        RunnerLaneSettings3D settings = GetSettings();

        if (settings == null)
            return centerX + (index <= 0 ? -1f : 1f);

        return centerX + (
            index <= 0
                ? settings.leftLaneOffset
                : settings.rightLaneOffset
        );
    }

    public static float[] GetLanePositions()
    {
        float centerX = GetCenterX();
        RunnerLaneSettings3D settings = GetSettings();

        if (settings == null)
            return new float[] { centerX - 1f, centerX + 1f };

        return new float[]
        {
            centerX + settings.leftLaneOffset,
            centerX + settings.rightLaneOffset
        };
    }
}
'''


def replace_method(text, accepted_names, replacement, label):
    pattern = re.compile(
        rf"(?ms)^    private void (?:{accepted_names})\(\)\s*\{{.*?^    \}}"
    )

    result, count = pattern.subn(replacement, text, count=1)

    if count != 1:
        raise RuntimeError(
            f"{label}: не найден ожидаемый метод синхронизации полос "
            f"(совпадений: {count})."
        )

    return result


def hide_lane_array(text, label):
    pattern = re.compile(
        r"(?m)^(    )public float\[\] lanePositions\s*="
    )
    matches = list(pattern.finditer(text))

    if len(matches) != 1:
        raise RuntimeError(
            f"{label}: ожидалось одно поле lanePositions, найдено {len(matches)}."
        )

    match = matches[0]

    if text[:match.start()].rstrip().endswith("[HideInInspector]"):
        return text

    return (
        text[:match.start()]
        + "    [HideInInspector]\n"
        + text[match.start():]
    )


def insert_settings_component(scene, script_guid):
    # Если компонент уже записан в сцене, не добавляем его второй раз.
    if re.search(
        rf"(?m)^  m_Script: \{{fileID: 11500000, "
        rf"guid: {script_guid}, type: 3\}}$",
        scene,
    ):
        return scene, "компонент уже присутствует"

    blocks = list(re.finditer(
        r"(?ms)^--- !u!1 &(\d+)\nGameObject:\n(.*?)(?=^--- !u!|\Z)",
        scene,
    ))

    targets = [
        match for match in blocks
        if re.search(
            r"(?m)^  m_Name: ObstacleSpawner3D\s*$",
            match.group(2),
        )
    ]

    if len(targets) != 1:
        raise RuntimeError(
            "MainRoad.unity: ожидался один объект ObstacleSpawner3D, "
            f"найдено {len(targets)}."
        )

    target = targets[0]
    game_object_id = int(target.group(1))

    all_ids = [
        int(value)
        for value in re.findall(
            r"(?m)^--- !u!\d+ &(\d+)\s*$",
            scene,
        )
    ]
    component_id = max(all_ids, default=100000) + 1

    component_list = re.compile(
        r"(?m)^  m_Component:\n"
        r"(?:  - component: \{fileID: \d+\}\n)+"
    )

    go_text, count = component_list.subn(
        lambda match: (
            match.group(0)
            + f"  - component: {{fileID: {component_id}}}\n"
        ),
        target.group(2),
        count=1,
    )

    if count != 1:
        raise RuntimeError(
            "MainRoad.unity: не удалось обновить список компонентов "
            "ObstacleSpawner3D."
        )

    start, end = target.span(2)
    scene = scene[:start] + go_text + scene[end:]

    component_block = f'''
--- !u!114 &{component_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {game_object_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}
  m_Name:
  m_EditorClassIdentifier: Assembly-CSharp::RunnerLaneSettings3D
  leftLaneOffset: -1
  rightLaneOffset: 1
'''

    return scene.rstrip() + "\n" + component_block, (
        "компонент добавлен к ObstacleSpawner3D"
    )


def main():
    if not (ROOT / "Assets").is_dir():
        raise RuntimeError(
            "Помести скрипт в корень Unity-проекта, рядом с папкой Assets."
        )

    for path in FILES.values():
        if not path.is_file():
            raise RuntimeError(f"Не найден файл: {path.relative_to(ROOT)}")

    # Подготавливаем все изменения в памяти до записи на диск.
    source = {
        name: path.read_text(encoding="utf-8-sig")
        for name, path in FILES.items()
    }
    result = dict(source)

    # Игрок: координаты должны браться из общей настройки
    # как при запуске, так и после возрождения.
    player = re.sub(
        r"(?m)^([ \t]*)EnsureLanePositions\(\);[ \t]*$",
        r"\1lanePositions = RunnerLaneSettings3D.GetLanePositions();",
        source["player"],
    )

    player, count = re.subn(
        r"lanePositions\s*=\s*RunnerLaneSettings3D\s*"
        r"\.\s*GetLanePositions\(\);",
        "lanePositions = RunnerLaneSettings3D.GetLanePositions();",
        player,
    )

    if count != 2:
        raise RuntimeError(
            "PlayerMovement3D: ожидались две синхронизации полос "
            f"(Awake и Revive), найдено {count}."
        )

    player = re.sub(
        r"(?ms)^    private void EnsureLanePositions\(\)\s*"
        r"\{.*?^    \}\n?",
        "",
        player,
        count=1,
    )

    result["player"] = hide_lane_array(player, "PlayerMovement3D")

    sync_body = '''    private void SyncLanePositions()
    {
        lanePositions = RunnerLaneSettings3D.GetLanePositions();
    }'''

    # Препятствия: поддерживаем исходную версию и прошлый патч.
    obstacles = replace_method(
        source["obstacles"],
        r"SyncLanePositions|EnsureLanePositions",
        sync_body,
        "ObstacleSpawner3D",
    )

    obstacles = re.sub(
        r"(?m)^([ \t]*)EnsureLanePositions\(\);[ \t]*$",
        r"\1SyncLanePositions();",
        obstacles,
    )

    count = len(re.findall(
        r"(?m)^[ \t]*SyncLanePositions\(\);[ \t]*$",
        obstacles,
    ))

    if count != 3:
        raise RuntimeError(
            f"ObstacleSpawner3D: ожидались три вызова синхронизации, найдено {count}."
        )

    result["obstacles"] = hide_lane_array(
        obstacles, "ObstacleSpawner3D"
    )

    # Пикапы: перестаём копировать потенциально устаревшие полосы
    # из массива игрока — используем общую настройку напрямую.
    pickups = replace_method(
        source["pickups"],
        r"SyncLanePositionsWithPlayer|SyncLanePositions|EnsureLanePositions",
        sync_body,
        "PickupSpawner3D",
    )

    pickups = re.sub(
        r"(?m)^([ \t]*)(?:EnsureLanePositions|SyncLanePositionsWithPlayer)"
        r"\(\);[ \t]*$",
        r"\1SyncLanePositions();",
        pickups,
    )

    count = len(re.findall(
        r"(?m)^[ \t]*SyncLanePositions\(\);[ \t]*$",
        pickups,
    ))

    if count != 2:
        raise RuntimeError(
            f"PickupSpawner3D: ожидались два вызова синхронизации, найдено {count}."
        )

    result["pickups"] = hide_lane_array(pickups, "PickupSpawner3D")

    # Спасаемые люди используют тот же источник координат.
    rescued = replace_method(
        source["rescued"],
        r"SyncLanePositions|EnsureLanePositions",
        sync_body,
        "RescuedPersonSpawner3D",
    )

    rescued = re.sub(
        r"(?m)^([ \t]*)EnsureLanePositions\(\);[ \t]*$",
        r"\1SyncLanePositions();",
        rescued,
    )

    count = len(re.findall(
        r"(?m)^[ \t]*SyncLanePositions\(\);[ \t]*$",
        rescued,
    ))

    if count != 2:
        raise RuntimeError(
            "RescuedPersonSpawner3D: ожидались два вызова "
            f"синхронизации, найдено {count}."
        )

    result["rescued"] = hide_lane_array(
        rescued, "RescuedPersonSpawner3D"
    )

    result["settings"] = SETTINGS_SOURCE

    meta_path = FILES["settings"].with_suffix(".cs.meta")

    if not meta_path.is_file():
        raise RuntimeError("Не найден RunnerLaneSettings3D.cs.meta.")

    meta = meta_path.read_text(encoding="utf-8-sig")
    guid_match = re.search(
        r"(?m)^guid:\s*([0-9a-fA-F]+)\s*$",
        meta,
    )

    if not guid_match:
        raise RuntimeError("В .meta-файле не найден GUID скрипта.")

    result["scene"], scene_status = insert_settings_component(
        result["scene"],
        guid_match.group(1),
    )

    # Все проверки пройдены. Создаём резервные копии до первой записи.
    backup_root = ROOT / ".lane-settings-backup"

    for name, path in FILES.items():
        backup = backup_root / path.relative_to(ROOT)
        backup.parent.mkdir(parents=True, exist_ok=True)

        if not backup.exists():
            shutil.copy2(path, backup)

    for name, path in FILES.items():
        path.write_text(result[name], encoding="utf-8")
        print(f"Обновлён: {path.relative_to(ROOT)}")

    print(f"\nСцена: {scene_status}")
    print("Резервные копии сохранены в .lane-settings-backup.")
    print("Теперь настройки полос находятся в Inspector у ObstacleSpawner3D.")
    print("Используй поля leftLaneOffset и rightLaneOffset.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"ОШИБКА: {exc}", file=sys.stderr)
        print(
            "Проверка остановлена. При ошибке проверки исходные файлы "
            "не должны быть изменены.",
            file=sys.stderr,
        )
        raise SystemExit(1)
