// File: Systems/ZoneToolSystem.ExistingRoads.PrefabSafety.cs
// Purpose: Provide a non-null prefab for ToolBaseSystem.GetPrefab() and dump PrefabIDs for debugging.

namespace ZoningToolkit.Systems
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using Game.Prefabs;

    internal sealed partial class ZoneToolSystemExistingRoads
    {
        private PrefabBase? m_SafePrefabForUI;
        private bool m_TriedResolveSafePrefabForUI;

#if DEBUG
        private bool m_DidDebugDump;
#endif

        private void EnsureSafePrefabForUI()
        {
            if (m_SafePrefabForUI != null)
            {
                return;
            }

            if (m_TriedResolveSafePrefabForUI)
            {
                return;
            }

            m_TriedResolveSafePrefabForUI = true;

            // Prefer “tool-ish” candidates first. These are just *attempts*.
            // If these don't exist in a given version/mod set, TryGetPrefab will fail and we move on.
            TryAssignSafePrefab(new PrefabID("FencePrefab", "Quay"));
            TryAssignSafePrefab(new PrefabID("FencePrefab", "Quay01"));
            TryAssignSafePrefab(new PrefabID("FencePrefab", "RetainingWall"));
            TryAssignSafePrefab(new PrefabID("FencePrefab", "RetainingWall01"));
            TryAssignSafePrefab(new PrefabID("FencePrefab", "Tunnel"));
            TryAssignSafePrefab(new PrefabID("FencePrefab", "Tunnel01"));
            TryAssignSafePrefab(new PrefabID("FencePrefab", "Elevated"));
            TryAssignSafePrefab(new PrefabID("FencePrefab", "Elevated01"));

            // Fallback: pick the first prefab ID we can discover via reflection (guaranteed non-null GetPrefab()).
            if (m_SafePrefabForUI == null && TryGetAnyPrefabIdKey(out PrefabID anyId))
            {
                TryAssignSafePrefab(anyId);
            }

            if (m_SafePrefabForUI == null)
            {
                Mod.s_Log.Warn($"{Mod.ModTag} PrefabSafety failed: no prefab resolved");
            }
            else
            {
                Mod.s_Log.Info($"{Mod.ModTag} PrefabSafety selected: {m_SafePrefabForUI.name}");
            }

#if DEBUG
            // One-time debug dump after we know PrefabSystem is alive.
            if (!m_DidDebugDump)
            {
                m_DidDebugDump = true;
                DebugDumpPrefabIds("Crosswalk");
                DebugDumpPrefabIds("Wide Sidewalk");
                DebugDumpPrefabIds("Quay");
                DebugDumpPrefabIds("Fence");
                DebugDumpPrefabIds("Road");
                DebugDumpPrefabIds("Lane");

                // Also show some “donor” examples so we can choose a better stable candidate later.
                DebugDumpAnyPrefabDonors(max: 25);
            }
#endif
        }

        private PrefabBase GetSafePrefabForUI()
        {
            // ToolSystem calls GetPrefab() and assumes it can be null in vanilla tools,
            // but some mod/UIs choke on null. We guarantee non-null here.
            if (m_SafePrefabForUI == null)
            {
                Mod.s_Log.Warn($"{Mod.ModTag} GetPrefab fallback still null; forcing reflection pick.");
                if (TryGetAnyPrefabIdKey(out PrefabID anyId))
                {
                    TryAssignSafePrefab(anyId);
                }
            }

            return m_SafePrefabForUI!;
        }

        private void TryAssignSafePrefab(PrefabID id)
        {
            if (m_SafePrefabForUI != null)
            {
                return;
            }

            if (m_ZTPrefabSystem.TryGetPrefab(id, out PrefabBase prefab) && prefab != null)
            {
                m_SafePrefabForUI = prefab;
            }
        }

        private bool TryGetAnyPrefabIdKey(out PrefabID id)
        {
            id = default;

            try
            {
                foreach (IDictionary dict in EnumeratePrefabIdDictionaries(m_ZTPrefabSystem))
                {
                    foreach (object key in dict.Keys)
                    {
                        if (key is PrefabID pid)
                        {
                            id = pid;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Mod.s_Log.Warn($"{Mod.ModTag} PrefabSafety key scan failed: {ex.GetType().Name}");
            }

            return false;
        }

        internal void DebugDumpPrefabIds(string contains)
        {
#if !DEBUG
            _ = contains;
#else
            try
            {
                int dictCount = 0;
                int keyCount = 0;
                int matchCount = 0;

                foreach (IDictionary dict in EnumeratePrefabIdDictionaries(m_ZTPrefabSystem))
                {
                    dictCount++;

                    foreach (object key in dict.Keys)
                    {
                        keyCount++;

                        if (key is not PrefabID pid)
                        {
                            continue;
                        }

                        string s = pid.ToString();
                        if (s.IndexOf(contains, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            matchCount++;
                            Mod.s_Log.Info($"{Mod.ModTag} PrefabID match ({contains}): {s}");
                        }
                    }
                }

                Mod.s_Log.Info($"{Mod.ModTag} PrefabID scan '{contains}': dicts={dictCount}, keys={keyCount}, matches={matchCount}");
            }
            catch (Exception ex)
            {
                Mod.s_Log.Warn($"{Mod.ModTag} DebugDumpPrefabIds('{contains}') failed: {ex.GetType().Name}");
            }
#endif
        }

        internal void DebugDumpAnyPrefabDonors(int max)
        {
#if !DEBUG
            _ = max;
#else
            try
            {
                int printed = 0;
                int scanned = 0;

                foreach (IDictionary dict in EnumeratePrefabIdDictionaries(m_ZTPrefabSystem))
                {
                    foreach (object key in dict.Keys)
                    {
                        if (printed >= max)
                        {
                            Mod.s_Log.Info($"{Mod.ModTag} Donor dump done: printed={printed}, scanned={scanned}");
                            return;
                        }

                        scanned++;

                        if (key is not PrefabID pid)
                        {
                            continue;
                        }

                        if (!m_ZTPrefabSystem.TryGetPrefab(pid, out PrefabBase prefab) || prefab == null)
                        {
                            continue;
                        }

                        // Print a small sampling of real, resolvable prefabs.
                        Mod.s_Log.Info($"{Mod.ModTag} Donor candidate: {pid} -> {prefab.name}");
                        printed++;
                    }
                }

                Mod.s_Log.Info($"{Mod.ModTag} Donor dump done: printed={printed}, scanned={scanned}");
            }
            catch (Exception ex)
            {
                Mod.s_Log.Warn($"{Mod.ModTag} DebugDumpAnyPrefabDonors failed: {ex.GetType().Name}");
            }
#endif
        }

        private static IEnumerable<IDictionary> EnumeratePrefabIdDictionaries(PrefabSystem prefabSystem)
        {
            // Scan all Dictionary<PrefabID, *> fields inside PrefabSystem.
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            FieldInfo[] fields = typeof(PrefabSystem).GetFields(flags);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo f = fields[i];

                if (!f.FieldType.IsGenericType)
                {
                    continue;
                }

                Type gen = f.FieldType.GetGenericTypeDefinition();
                if (gen != typeof(Dictionary<,>))
                {
                    continue;
                }

                Type[] args = f.FieldType.GetGenericArguments();
                if (args.Length != 2 || args[0] != typeof(PrefabID))
                {
                    continue;
                }

                object? obj = f.GetValue(prefabSystem);
                if (obj is IDictionary dict && dict.Count > 0)
                {
                    yield return dict;
                }
            }
        }
    }
}
