// Systems/ZoneToolSystem.ExistingRoads.Jobs.cs
// Jobs used by ZoneToolSystemExistingRoads (highlight/unhighlight/apply zoning info).

namespace ZoningToolkit.Systems
{
    using Game.Common;
    using Game.Net;
    using Game.Tools;
    using Game.Zones;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using ZoningToolkit.Components;

    /// <summary>
    /// Job: mark a road entity (and its edge endpoints) as highlighted + updated.
    /// </summary>
    public struct HighlightEntitiesJob : IJob
    {
        public EntityCommandBuffer CommandBuffer;
        public Entity EntityToHighlight;
        public ComponentLookup<Edge> EdgeLookup;

        public void Execute()
        {
            CommandBuffer.AddComponent<Highlighted>(EntityToHighlight);
            CommandBuffer.AddComponent<Updated>(EntityToHighlight);

            if (EdgeLookup.HasComponent(EntityToHighlight))
            {
                Edge edge = EdgeLookup[EntityToHighlight];
                CommandBuffer.AddComponent<Updated>(edge.m_Start);
                CommandBuffer.AddComponent<Updated>(edge.m_End);
            }
        }
    }

    /// <summary>
    /// Job: remove highlight from a road entity (and update its endpoints).
    /// </summary>
    public struct UnHighlightEntitiesJob : IJob
    {
        public EntityCommandBuffer CommandBuffer;
        public Entity EntityToUnhighlight;
        public ComponentLookup<Edge> EdgeLookup;

        public void Execute()
        {
            CommandBuffer.RemoveComponent<Highlighted>(EntityToUnhighlight);
            CommandBuffer.AddComponent<Updated>(EntityToUnhighlight);

            if (EdgeLookup.HasComponent(EntityToUnhighlight))
            {
                Edge edge = EdgeLookup[EntityToUnhighlight];
                CommandBuffer.AddComponent<Updated>(edge.m_Start);
                CommandBuffer.AddComponent<Updated>(edge.m_End);
            }
        }
    }

    /// <summary>
    /// Job: apply a new ZoningInfo to all selected entities and mark blocks for update.
    /// </summary>
    public struct UpdateZoningInfo : IJob
    {
        public NativeHashSet<Entity> EntitySet;
        public ComponentLookup<Curve> CurveLookup;
        public ComponentLookup<ZoningInfo> ZoningInfoLookup;
        public BufferLookup<SubBlock> SubBlockBufferLookup;
        public ComponentLookup<Edge> EdgeLookup;
        public EntityCommandBuffer CommandBuffer;
        public ZoningInfo NewZoningInfo;

        public void Execute()
        {
            NativeArray<Entity> entities = EntitySet.ToNativeArray(Allocator.TempJob);

            foreach (Entity entity in entities)
            {
                if (CurveLookup.HasComponent(entity))
                {
                    // Apply zoning info to the road entity (set if present, add if missing).
                    if (ZoningInfoLookup.HasComponent(entity))
                    {
                        CommandBuffer.SetComponent(entity, NewZoningInfo);
                    }
                    else
                    {
                        CommandBuffer.AddComponent(entity, NewZoningInfo);
                    }

                    // Mark all sub-blocks as needing re-zoning.
                    if (SubBlockBufferLookup.HasBuffer(entity))
                    {
                        DynamicBuffer<SubBlock> subBlocks = SubBlockBufferLookup[entity];
                        foreach (SubBlock sub in subBlocks)
                        {
                            CommandBuffer.AddComponent<ZoningInfoUpdated>(sub.m_SubBlock);
                        }
                    }
                }

                // Remove highlight and mark road as updated.
                CommandBuffer.RemoveComponent<Highlighted>(entity);
                CommandBuffer.AddComponent<Updated>(entity);

                // Also mark edge endpoints as updated, if any.
                if (EdgeLookup.HasComponent(entity))
                {
                    Edge edge = EdgeLookup[entity];
                    CommandBuffer.AddComponent<Updated>(edge.m_Start);
                    CommandBuffer.AddComponent<Updated>(edge.m_End);
                }
            }

            entities.Dispose();
            EntitySet.Clear();
        }
    }
}
