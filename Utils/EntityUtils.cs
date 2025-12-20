// Utils/EntityUtils.cs
// Debug inspect helper: dump component types on an entity (or entities in a query) to log.
// In Release builds these methods become no-ops (to avoid log spam/perf impact).

namespace ZoningToolkit.Utils
{
    using System.Runtime.CompilerServices;
    using Game.Tools;
    using Game.UI;
    using Unity.Collections;
    using Unity.Entities;

    public static class EntityUtils
    {
        // Works for your tool systems (ZoneToolSystemExistingRoads is a ToolBaseSystem).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void listEntityComponents(this ToolBaseSystem system, Entity entity)
        {
#if DEBUG
            DumpEntityComponents(system.EntityManager, entity);
#else
            _ = system;
            _ = entity;
#endif
        }

        // Works for UI systems too.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void listEntityComponents(this UISystemBase uiSystemBase, Entity entity)
        {
#if DEBUG
            DumpEntityComponents(uiSystemBase.EntityManager, entity);
#else
            _ = uiSystemBase;
            _ = entity;
#endif
        }

#if DEBUG
        private static void DumpEntityComponents(EntityManager em, Entity entity)
        {
            if (entity == Entity.Null || !em.Exists(entity))
            {
                Mod.s_Log.Debug($"{Mod.ModTag} EntityUtils: entity is Null or does not exist");
                return;
            }

            NativeArray<ComponentType> types = em.GetComponentTypes(entity, Allocator.Temp);
            try
            {
                for (int i = 0; i < types.Length; i++)
                {
                    ComponentType type = types[i];
                    Mod.s_Log.Debug($"{Mod.ModTag} Entity has component {type.GetManagedType()}");
                }
            }
            finally
            {
                types.Dispose();
            }
        }
#endif
    }
}
