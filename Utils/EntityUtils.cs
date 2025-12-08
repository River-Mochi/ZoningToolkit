// Utils/EntityUtils.cs

namespace ZoningToolkit.Utils
{
    using System;
    using System.Runtime.CompilerServices;
    using Game.Common;
    using Game.UI;
    using Unity.Collections;
    using Unity.Entities;
    using ZoningToolkit.Systems;

    public static class EntityUtils
    {
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
    }
}
