// Utils/EntityUtils.cs
// Debug inspect helper: dump component types on an entity (or entities in a query) to log.
// In Release builds these methods become no-ops (to avoid log spam/perf impact).

namespace ZoningToolkit.Utils
{
    using System.Runtime.CompilerServices;
    using Game.UI;
    using Unity.Entities;
    using ZoningToolkit.Systems;

    public static class EntityUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void listEntityComponents(this ZoneToolSystemCore system, Entity entity)
        {
#if DEBUG
            DumpEntityComponents(system.EntityManager, entity);
#else
            _ = system;
            _ = entity;
#endif
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void listEntityComponentsInQuery(this ZoneToolSystemCore system, EntityQuery entityQuery)
        {
#if DEBUG
            NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.Temp);
            try
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    Entity entity = entities[i];

                    Mod.s_Log.Debug("****** Listing entity components ******");
                    system.listEntityComponents(entity);

                    Mod.s_Log.Debug("***** Printing owner info ******");
                    if (system.ownerComponentLookup.HasComponent(entity))
                    {
                        Owner owner = system.ownerComponentLookup[entity];
                        system.listEntityComponents(owner.m_Owner);
                    }
                }
            }
            finally
            {
                entities.Dispose();
            }
#else
            _ = system;
            _ = entityQuery;
#endif
        }

#if DEBUG
        private static void DumpEntityComponents(EntityManager em, Entity entity)
        {
            NativeArray<ComponentType> types = em.GetComponentTypes(entity, Allocator.Temp);
            try
            {
                for (int i = 0; i < types.Length; i++)
                {
                    ComponentType type = types[i];
                    Mod.s_Log.Debug($"Entity has component {type.GetManagedType()}");
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
