
#!/usr/bin/env python3
"""Apply the Safe Zone Runner inspector lane-position fix.

Place this file in the Unity project root (the folder containing Assets),
then run: python apply_inspector_lane_fix.py
The script validates every expected source block before writing any file.
"""

from pathlib import Path
import shutil
import sys

ROOT = Path(__file__).resolve().parent

ENSURE_METHOD = '''    private void EnsureLanePositions()
    {
        if (lanePositions == null || lanePositions.Length != 2)
        {
            lanePositions = new float[] { -1f, 1f };
        }
    }'''


def replace_exact(
    text: str,
    old: str,
    new: str,
    expected: int,
    label: str,
) -> str:
    count = text.count(old)

    if count != expected:
        raise RuntimeError(
            f"{label}: expected {expected} match(es), "
            f"found {count}. No files have been modified; "
            "check whether the source has changed."
        )

    return text.replace(old, new)


def main() -> int:
    assets = ROOT / "Assets"

    if not assets.is_dir():
        print(
            "ERROR: Assets folder not found. "
            "Put this script in the Unity project root.",
            file=sys.stderr,
        )
        return 2

    paths = {
        "player": (
            ROOT
            / "Assets/Scripts/3D/Player/PlayerMovement3D.cs"
        ),
        "obstacles": (
            ROOT
            / "Assets/Scripts/3D/World/ObstacleSpawner3D.cs"
        ),
        "pickups": (
            ROOT
            / "Assets/Scripts/3D/Pickups/PickupSpawner3D.cs"
        ),
        "rescued": (
            ROOT
            / "Assets/Scripts/3D/Rescued/"
            "RescuedPersonSpawner3D.cs"
        ),
    }

    for path in paths.values():
        if not path.is_file():
            print(
                f"ERROR: Required file not found: "
                f"{path.relative_to(ROOT)}",
                file=sys.stderr,
            )
            return 2

    # Prepare all changes in memory first.
    # If validation fails, no source files will be written.
    updated = {
        name: path.read_text(encoding="utf-8-sig")
        for name, path in paths.items()
    }

    # ---------------------------------------------------------
    # PlayerMovement3D
    # ---------------------------------------------------------

    p = updated["player"]

    p = replace_exact(
        p,
        '''    [Header("Полосы движения")]
    public float[] lanePositions =
        new float[]
        {
            -1.35f,
            1.35f
        };''',
        '''    [Header("Полосы движения (мировые координаты X)")]
    [Tooltip("Элемент 0 — левая полоса, элемент 1 — правая. "
             "Значения берутся из Inspector.")]
    public float[] lanePositions =
        new float[]
        {
            -1f,
            1f
        };''',
        1,
        "PlayerMovement3D lane field",
    )

    p = replace_exact(
        p,
        '''        // ОДНА СИСТЕМА ПОЛОС ДЛЯ ВСЕЙ ИГРЫ.
        lanePositions =
            RunnerLaneSettings3D
                .GetLanePositions();''',
        "        EnsureLanePositions();",
        1,
        "PlayerMovement3D Awake overwrite",
    )

    p = replace_exact(
        p,
        '''        lanePositions =
            RunnerLaneSettings3D
                .GetLanePositions();''',
        "        EnsureLanePositions();",
        1,
        "PlayerMovement3D revive overwrite",
    )

    p = replace_exact(
        p,
        "    private void Awake()\n    {",
        ENSURE_METHOD + "\n\n    private void Awake()\n    {",
        1,
        "PlayerMovement3D helper insertion",
    )

    updated["player"] = p

    # ---------------------------------------------------------
    # ObstacleSpawner3D
    # ---------------------------------------------------------

    o = updated["obstacles"]

    o = replace_exact(
        o,
        '''    [Header("Lanes")]
    public float[] lanePositions = new float[]
    {
        -1f,
        1f
    };''',
        '''    [Header("Полосы препятствий (мировые координаты X)")]
    [Tooltip("Элемент 0 — левая полоса, элемент 1 — правая. "
             "Значения из Inspector не перезаписываются.")]
    public float[] lanePositions = new float[]
    {
        -1f,
        1f
    };''',
        1,
        "ObstacleSpawner3D lane field",
    )

    o = replace_exact(
        o,
        '''    private void SyncLanePositions()
    {
        lanePositions = RunnerLaneSettings3D.GetLanePositions();
    }''',
        ENSURE_METHOD,
        1,
        "ObstacleSpawner3D lane sync method",
    )

    o = replace_exact(
        o,
        "SyncLanePositions();",
        "EnsureLanePositions();",
        3,
        "ObstacleSpawner3D sync calls",
    )

    updated["obstacles"] = o

    # ---------------------------------------------------------
    # PickupSpawner3D
    # ---------------------------------------------------------

    k = updated["pickups"]

    k = replace_exact(
        k,
        '''    [Header("Полосы")]
    public float[] lanePositions =
        new float[] { -0.8f, 0.8f };''',
        '''    [Header("Полосы пикапов (мировые координаты X)")]
    [Tooltip("Элемент 0 — левая полоса, элемент 1 — правая. "
             "Настройка независима от PlayerMovement3D.")]
    public float[] lanePositions =
        new float[] { -1f, 1f };''',
        1,
        "PickupSpawner3D lane field",
    )

    old_pickup_method = '''    private void SyncLanePositionsWithPlayer()
    {
        PlayerMovement3D player =
            FindFirstObjectByType<PlayerMovement3D>();

        if (player != null &&
            player.lanePositions != null &&
            player.lanePositions.Length == 2)
        {
            lanePositions = new float[]
            {
                player.lanePositions[0],
                player.lanePositions[1]
            };
        }
        else if (lanePositions == null ||
                 lanePositions.Length != 2)
        {
            lanePositions = new float[]
            {
                -0.8f,
                0.8f
            };
        }
    }'''

    k = replace_exact(
        k,
        old_pickup_method,
        ENSURE_METHOD,
        1,
        "PickupSpawner3D lane sync method",
    )

    k = replace_exact(
        k,
        "SyncLanePositionsWithPlayer();",
        "EnsureLanePositions();",
        2,
        "PickupSpawner3D sync calls",
    )

    updated["pickups"] = k

    # ---------------------------------------------------------
    # RescuedPersonSpawner
    # ---------------------------------------------------------

    r = updated["rescued"]

    r = replace_exact(
        r,
        '''    [Header("Полосы")]
    public float[] lanePositions =
        new float[]
        {
            -1.35f,
            1.35f
        };''',
        '''    [Header("Полосы спасаемых людей "
             "(мировые координаты X)")]
    [Tooltip("Элемент 0 — левая полоса, элемент 1 — правая. "
             "Значения берутся из Inspector.")]
    public float[] lanePositions =
        new float[]
        {
            -1f,
            1f
        };''',
        1,
        "RescuedPersonSpawner lane field",
    )

    r = replace_exact(
        r,
        '''    private void SyncLanePositions()
    {
        lanePositions =
            RunnerLaneSettings3D
                .GetLanePositions();
    }''',
        ENSURE_METHOD,
        1,
        "RescuedPersonSpawner lane sync method",
    )

    r = replace_exact(
        r,
        "SyncLanePositions();",
        "EnsureLanePositions();",
        2,
        "RescuedPersonSpawner sync calls",
    )

    updated["rescued"] = r

    # ---------------------------------------------------------
    # Backup and write
    # ---------------------------------------------------------

    backup_root = ROOT / ".lane-fix-backup"

    # Every source block has been validated before reaching here.
    for name, path in paths.items():
        backup_path = backup_root / path.relative_to(ROOT)
        backup_path.parent.mkdir(
            parents=True,
            exist_ok=True,
        )

        if not backup_path.exists():
            shutil.copy2(path, backup_path)

    for name, path in paths.items():
        path.write_text(
            updated[name],
            encoding="utf-8",
        )
        print(f"Updated: {path.relative_to(ROOT)}")

    print("\nDone. Backups are in .lane-fix-backup/.")
    print(
        "Next: open Unity, wait for recompilation, "
        "then test MainRoad in Play Mode."
    )

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
