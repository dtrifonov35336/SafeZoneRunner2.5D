using UnityEngine;

public static class BackgroundStopper
{
    public static void StopAll()
    {
        int stopped = 0;

        // =====================================================
        // БОКОВОЙ SCROLLER
        // =====================================================

        SideScroller[] sideScrollers =
            Object.FindObjectsByType<SideScroller>(
                FindObjectsSortMode.None
            );

        foreach (var s in sideScrollers)
        {
            if (s != null &&
                s.enabled)
            {
                s.enabled = false;
                stopped++;
            }
        }

        // =====================================================
        // SPAWNER БОКОВОГО ДЕКОРА
        // =====================================================

        SideDecorationSpawner[] sideSpawners =
            Object.FindObjectsByType<SideDecorationSpawner>(
                FindObjectsSortMode.None
            );

        foreach (var s in sideSpawners)
        {
            if (s != null &&
                s.enabled)
            {
                s.enabled = false;
                stopped++;
            }
        }

        // =====================================================
        // УЖЕ СОЗДАННЫЙ ДЕКОР
        // =====================================================

        SideDecorationMover[] decorationMovers =
            Object.FindObjectsByType<SideDecorationMover>(
                FindObjectsSortMode.None
            );

        foreach (var mover in decorationMovers)
        {
            if (mover != null &&
                mover.enabled)
            {
                mover.enabled = false;
                stopped++;
            }
        }

        // =====================================================
        // CITY
        // =====================================================

        CityApproach[] cityApproaches =
            Object.FindObjectsByType<CityApproach>(
                FindObjectsSortMode.None
            );

        foreach (var c in cityApproaches)
        {
            if (c != null &&
                c.enabled)
            {
                c.enabled = false;
                stopped++;
            }
        }

        // =====================================================
        // ROAD
        // =====================================================

        RoadScroller[] roadScrollers =
            Object.FindObjectsByType<RoadScroller>(
                FindObjectsSortMode.None
            );

        foreach (var r in roadScrollers)
        {
            if (r != null &&
                r.enabled)
            {
                r.enabled = false;
                stopped++;
            }
        }

        // =====================================================
        // ZOMBIES
        // =====================================================

        ZombieCrowdAnimator[] zombies =
            Object.FindObjectsByType<ZombieCrowdAnimator>(
                FindObjectsSortMode.None
            );

        foreach (var z in zombies)
        {
            if (z != null &&
                z.enabled)
            {
                z.enabled = false;
                stopped++;
            }
        }

        // =====================================================
        // PARTICLES
        // =====================================================

        ParticleSystem[] particles =
            Object.FindObjectsByType<ParticleSystem>(
                FindObjectsSortMode.None
            );

        foreach (var ps in particles)
        {
            if (ps != null &&
                ps.isPlaying)
            {
                ps.Stop(
                    true,
                    ParticleSystemStopBehavior
                        .StopEmittingAndClear
                );

                stopped++;
            }
        }

        Debug.Log(
            $"[BackgroundStopper] Остановлено: {stopped}"
        );
    }

    public static void ResumeAll()
    {
        // SideScroller
        SideScroller[] sideScrollers =
            Object.FindObjectsByType<SideScroller>(
                FindObjectsSortMode.None
            );

        foreach (var s in sideScrollers)
        {
            if (s != null)
                s.enabled = true;
        }

        // SideDecorationSpawner
        SideDecorationSpawner[] sideSpawners =
            Object.FindObjectsByType<SideDecorationSpawner>(
                FindObjectsSortMode.None
            );

        foreach (var s in sideSpawners)
        {
            if (s != null)
                s.enabled = true;
        }

        // SideDecorationMover
        SideDecorationMover[] decorationMovers =
            Object.FindObjectsByType<SideDecorationMover>(
                FindObjectsSortMode.None
            );

        foreach (var mover in decorationMovers)
        {
            if (mover != null)
                mover.enabled = true;
        }

        // City
        CityApproach[] cityApproaches =
            Object.FindObjectsByType<CityApproach>(
                FindObjectsSortMode.None
            );

        foreach (var c in cityApproaches)
        {
            if (c != null)
                c.enabled = true;
        }

        // Road
        RoadScroller[] roadScrollers =
            Object.FindObjectsByType<RoadScroller>(
                FindObjectsSortMode.None
            );

        foreach (var r in roadScrollers)
        {
            if (r != null)
                r.enabled = true;
        }

        // Zombies
        ZombieCrowdAnimator[] zombies =
            Object.FindObjectsByType<ZombieCrowdAnimator>(
                FindObjectsSortMode.None
            );

        foreach (var z in zombies)
        {
            if (z != null)
                z.enabled = true;
        }

        // Particles
        ParticleSystem[] particles =
            Object.FindObjectsByType<ParticleSystem>(
                FindObjectsSortMode.None
            );

        foreach (var ps in particles)
        {
            if (ps != null &&
                !ps.isPlaying)
            {
                ps.Play();
            }
        }
    }
}