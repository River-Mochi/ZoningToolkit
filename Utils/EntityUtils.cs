// Utils/EntityUtils.cs
// Debug inspect helper (DEBUG-only).
// Dump component types on an entity (or entities in a query) to console/log.

namespace ZoningToolkit.Utils
{
    using System.Runtime.CompilerServices;
    using Game.UI;
    using Unity.Entities;
    using ZoningToolkit.Systems;

    public static class EntityUtils
    {
#if DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void listEntityComponents(this ZoneToolSystemCore system, Entity entity)
        {
            var types = system.EntityManager.GetComponentTypes(entity);
            foreach (var type in types)
            {
                Console.WriteLine($"Entity has component {type.GetManagedType()}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void listEntityComponents(this UISystemBase uiSystemBase, Entity entity)
        {
            var types = uiSystemBase.EntityManager.GetComponentTypes(entity);
            foreach (var type in types)
            {
                Console.WriteLine($"Entity has component {type.GetManagedType()}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void listEntityComponentsInQuery(this ZoneToolSystemCore system, EntityQuery entityQuery)
        {
            NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];

                Mod.s_Log.Info("****** Listing entity components ******");
                system.listEntityComponents(entity);

                Mod.s_Log.Info("***** Printing owner info ******");
                if (system.ownerComponentLookup.HasComponent(entity))
                {
                    Owner owner = system.ownerComponentLookup[entity];
                    system.listEntityComponents(owner.m_Owner);
                }
            }

            entities.Dispose();
        }
#else
        // Release builds: no-op to avoid accidental allocations/log spam.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void listEntityComponents(this ZoneToolSystemCore system, Entity entity)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void listEntityComponents(this UISystemBase uiSystemBase, Entity entity)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void listEntityComponentsInQuery(this ZoneToolSystemCore system, EntityQuery entityQuery)
        {
        }
#endif
    }
}
