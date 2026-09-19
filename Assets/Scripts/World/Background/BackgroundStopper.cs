using UnityEngine;

public static class BackgroundStopper
{
    /// <summary>
    /// Останавливает все скроллеры фона и дороги. Вызывается при GameOver/Victory.
    /// </summary>
    public static void StopAll()
    {
        int stopped = 0;

        // 1. Боковой лес
        SideScroller[] sideScrollers = Object.FindObjectsByType<SideScroller>(FindObjectsSortMode.None);
        foreach (var s in sideScrollers)
        {
            if (s != null && s.enabled)
            {
                s.enabled = false;
                stopped++;
            }
        }

        // 2. Приближение города
        CityApproach[] cityApproaches = Object.FindObjectsByType<CityApproach>(FindObjectsSortMode.None);
        foreach (var c in cityApproaches)
        {
            if (c != null && c.enabled)
            {
                c.enabled = false;
                stopped++;
            }
        }

        // 3. Дорога
        RoadScroller[] roadScrollers = Object.FindObjectsByType<RoadScroller>(FindObjectsSortMode.None);
        foreach (var r in roadScrollers)
        {
            if (r != null && r.enabled)
            {
                r.enabled = false;
                stopped++;
            }
        }

        // 4. Толпа зомби в UI
        ZombieCrowdAnimator[] zombies = Object.FindObjectsByType<ZombieCrowdAnimator>(FindObjectsSortMode.None);
        foreach (var z in zombies)
        {
            if (z != null && z.enabled)
            {
                z.enabled = false;
                stopped++;
            }
        }

        // 5. Пыль (Particle System)
        ParticleSystem[] particles = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        foreach (var ps in particles)
        {
            if (ps != null && ps.isPlaying)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                stopped++;
            }
        }

        Debug.Log($"[BackgroundStopper] Остановлено скроллеров: {stopped}");
    }

    /// <summary>
    /// Включает всё обратно — для перезапуска сцены.
    /// </summary>
    public static void ResumeAll()
    {
        SideScroller[] sideScrollers = Object.FindObjectsByType<SideScroller>(FindObjectsSortMode.None);
        foreach (var s in sideScrollers)
            if (s != null) s.enabled = true;

        CityApproach[] cityApproaches = Object.FindObjectsByType<CityApproach>(FindObjectsSortMode.None);
        foreach (var c in cityApproaches)
            if (c != null) c.enabled = true;

        RoadScroller[] roadScrollers = Object.FindObjectsByType<RoadScroller>(FindObjectsSortMode.None);
        foreach (var r in roadScrollers)
            if (r != null) r.enabled = true;

        ZombieCrowdAnimator[] zombies = Object.FindObjectsByType<ZombieCrowdAnimator>(FindObjectsSortMode.None);
        foreach (var z in zombies)
            if (z != null) z.enabled = true;

        ParticleSystem[] particles = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        foreach (var ps in particles)
        {
            if (ps != null && !ps.isPlaying)
                ps.Play();
        }
    }
}