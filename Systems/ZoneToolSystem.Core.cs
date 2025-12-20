// Systems/ZoneToolSystem.Core.cs
// Core zoning application system (new + updated blocks).
// Applies optional protection rules from Settings:
// - ProtectOccupiedCells (default ON)
// - ProtectZonedCells (default OFF)

namespace ZoningToolkit.Systems
{
    using Game;
    using Game.Common;
    using Game.Net;
    using Game.Zones;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine.Scripting;
    using ZoningToolkit.Components;
    using ZoningToolkit.Utils;

    public partial class ZoneToolSystemCore : GameSystemBase
    {
        private EntityQuery m_NewBlocksQuery;
        private EntityQuery m_UpdateBlocksQuery;

        private ComponentTypeHandle<Block> m_BlockTypeHandle;
        private EntityTypeHandle m_EntityTypeHandle;
        private ComponentTypeHandle<ValidArea> m_ValidAreaTypeHandle;
        private ComponentTypeHandle<Deleted> m_DeletedTypeHandle;

        public ComponentTypeHandle<Owner> ownerTypeHandle;
        public BufferTypeHandle<Cell> cellBufferTypeHandle;

        public ComponentLookup<Owner> ownerComponentLookup;
        [ReadOnly] protected ComponentLookup<Curve> curveComponentLookup;
        private ComponentLookup<ZoningInfo> zoningInfoComponentLookup;
        private ComponentLookup<Deleted> deletedLookup;
        private ComponentLookup<Applied> appliedLookup;
        private ComponentLookup<Updated> updatedLookup;
        private ComponentLookup<ZoningInfoUpdated> zoningInfoUpdatedLookup;

        private ModificationBarrier4B m_ModificationBarrier4B = null!;

        internal ZoningMode zoningMode;

        protected override void OnCreate()
        {
            Mod.s_Log.Info("Creating ZoneToolSystemCore");

            base.OnCreate();

            m_NewBlocksQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadWrite<Block>(),
                    ComponentType.ReadWrite<Owner>(),
                    ComponentType.ReadOnly<Cell>(),
                    ComponentType.ReadOnly<ValidArea>()
                },
                Any = new[]
                {
                    ComponentType.ReadOnly<Created>(),
                    ComponentType.ReadOnly<Deleted>()
                }
            });

            m_UpdateBlocksQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadWrite<Block>(),
                    ComponentType.ReadWrite<Owner>(),
                    ComponentType.ReadOnly<Cell>(),
                    ComponentType.ReadOnly<ValidArea>(),
                    ComponentType.ReadOnly<ZoningInfoUpdated>()
                }
            });

            m_BlockTypeHandle = GetComponentTypeHandle<Block>();
            ownerTypeHandle = GetComponentTypeHandle<Owner>();
            cellBufferTypeHandle = GetBufferTypeHandle<Cell>();
            m_EntityTypeHandle = GetEntityTypeHandle();
            m_ValidAreaTypeHandle = GetComponentTypeHandle<ValidArea>();
            m_DeletedTypeHandle = GetComponentTypeHandle<Deleted>();

            ownerComponentLookup = GetComponentLookup<Owner>();
            curveComponentLookup = GetComponentLookup<Curve>(true);
            zoningInfoComponentLookup = GetComponentLookup<ZoningInfo>();
            deletedLookup = GetComponentLookup<Deleted>();
            appliedLookup = GetComponentLookup<Applied>();
            updatedLookup = GetComponentLookup<Updated>();
            zoningInfoUpdatedLookup = GetComponentLookup<ZoningInfoUpdated>();

            m_ModificationBarrier4B = World.GetOrCreateSystemManaged<ModificationBarrier4B>();

            RequireAnyForUpdate(m_NewBlocksQuery, m_UpdateBlocksQuery);

            zoningMode = ZoningMode.Default;
        }

        [Preserve]
        protected override void OnUpdate()
        {
            m_BlockTypeHandle.Update(ref CheckedStateRef);
            ownerComponentLookup.Update(ref CheckedStateRef);
            curveComponentLookup.Update(ref CheckedStateRef);
            zoningInfoComponentLookup.Update(ref CheckedStateRef);
            m_EntityTypeHandle.Update(ref CheckedStateRef);
            m_ValidAreaTypeHandle.Update(ref CheckedStateRef);
            m_DeletedTypeHandle.Update(ref CheckedStateRef);
            ownerTypeHandle.Update(ref CheckedStateRef);
            deletedLookup.Update(ref CheckedStateRef);
            cellBufferTypeHandle.Update(ref CheckedStateRef);
            appliedLookup.Update(ref CheckedStateRef);
            updatedLookup.Update(ref CheckedStateRef);
            zoningInfoUpdatedLookup.Update(ref CheckedStateRef);

            EntityCommandBuffer ecb = m_ModificationBarrier4B.CreateCommandBuffer();

            bool protectOccupiedCells = Mod.Settings?.ProtectOccupiedCells ?? true;
            bool protectZonedCells = Mod.Settings?.ProtectZonedCells ?? false;

            NativeParallelHashMap<float2, Entity> deletedByStart =
                new NativeParallelHashMap<float2, Entity>(32, Allocator.TempJob);
            NativeParallelHashMap<float2, Entity> deletedByEnd =
                new NativeParallelHashMap<float2, Entity>(32, Allocator.TempJob);

            JobHandle deps = Dependency;

            JobHandle collectDeletedJob = new CollectDeletedCurves
            {
                ownerTypeHandle = ownerTypeHandle,
                deletedTypeHandle = m_DeletedTypeHandle,
                curveLookup = curveComponentLookup,
                deletedLookup = deletedLookup,
                curvesByStartPoint = deletedByStart,
                curvesByEndPoint = deletedByEnd
            }.Schedule(m_NewBlocksQuery, deps);

            deps = JobHandle.CombineDependencies(deps, collectDeletedJob);

            if (!m_NewBlocksQuery.IsEmptyIgnoreFilter)
            {
                JobHandle job = new UpdateZoneData
                {
                    zoningMode = zoningMode,
                    protectOccupiedCells = protectOccupiedCells,
                    protectZonedCells = protectZonedCells,

                    entityTypeHandle = m_EntityTypeHandle,
                    blockComponentTypeHandle = m_BlockTypeHandle,
                    validAreaComponentTypeHandle = m_ValidAreaTypeHandle,
                    deletedTypeHandle = m_DeletedTypeHandle,
                    bufferTypeHandle = cellBufferTypeHandle,
                    ownerComponentLookup = ownerComponentLookup,
                    curveComponentLookup = curveComponentLookup,
                    zoningInfoComponentLookup = zoningInfoComponentLookup,
                    appliedLookup = appliedLookup,
                    entityCommandBuffer = ecb,
                    entitiesByStartPoint = deletedByStart,
                    entitiesByEndPoint = deletedByEnd
                }.Schedule(m_NewBlocksQuery, deps);

                deps = JobHandle.CombineDependencies(deps, job);
            }

            if (!m_UpdateBlocksQuery.IsEmptyIgnoreFilter)
            {
                JobHandle job = new UpdateZoningInfoJob
                {
                    zoningMode = zoningMode,
                    protectOccupiedCells = protectOccupiedCells,
                    protectZonedCells = protectZonedCells,

                    entityTypeHandle = m_EntityTypeHandle,
                    blockComponentTypeHandle = m_BlockTypeHandle,
                    validAreaComponentTypeHandle = m_ValidAreaTypeHandle,
                    bufferTypeHandle = cellBufferTypeHandle,
                    ownerComponentLookup = ownerComponentLookup,
                    curveComponentLookup = curveComponentLookup,
                    zoningInfoComponentLookup = zoningInfoComponentLookup,
                    zoningInfoUpdateComponentLookup = zoningInfoUpdatedLookup,
                    entityCommandBuffer = ecb,
                    updatedLookup = updatedLookup
                }.Schedule(m_UpdateBlocksQuery, deps);

                deps = JobHandle.CombineDependencies(deps, job);
            }

            JobHandle disposeJob = new DisposeHashMaps
            {
                toDispose1 = deletedByStart,
                toDispose2 = deletedByEnd
            }.Schedule(deps);

            Dependency = JobHandle.CombineDependencies(disposeJob, deps);
            m_ModificationBarrier4B.AddJobHandleForProducer(Dependency);
        }

        public void SetZoningMode(string mode)
        {
            Mod.s_Log.Info($"Changing zoning mode to {mode}");

            zoningMode = mode switch
            {
                "Left" => ZoningMode.Left,
                "Right" => ZoningMode.Right,
                "Default" => ZoningMode.Default,
                "None" => ZoningMode.None,
                _ => ZoningMode.Default
            };
        }

        private static ZoningInfo DefaultZI() => new ZoningInfo { zoningMode = ZoningMode.Default };

        private static void AddOrSetZoningInfo(
            EntityCommandBuffer ecb,
            ComponentLookup<ZoningInfo> lookup,
            Entity owner,
            ZoningInfo zi)
        {
            if (lookup.HasComponent(owner))
            {
                ecb.SetComponent(owner, zi);
            }
            else
            {
                ecb.AddComponent(owner, zi);
            }
        }

        private struct UpdateZoningInfoJob : IJobChunk
        {
            [ReadOnly] public ZoningMode zoningMode;
            [ReadOnly] public bool protectOccupiedCells;
            [ReadOnly] public bool protectZonedCells;

            [ReadOnly] public EntityTypeHandle entityTypeHandle;
            public ComponentTypeHandle<Block> blockComponentTypeHandle;
            public ComponentTypeHandle<ValidArea> validAreaComponentTypeHandle;
            public BufferTypeHandle<Cell> bufferTypeHandle;
            public ComponentLookup<Owner> ownerComponentLookup;
            [ReadOnly] public ComponentLookup<Curve> curveComponentLookup;
            public ComponentLookup<ZoningInfo> zoningInfoComponentLookup;
            public ComponentLookup<ZoningInfoUpdated> zoningInfoUpdateComponentLookup;
            public EntityCommandBuffer entityCommandBuffer;
            public ComponentLookup<Updated> updatedLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Block> blocks = chunk.GetNativeArray(ref blockComponentTypeHandle);
                NativeArray<Entity> entities = chunk.GetNativeArray(entityTypeHandle);
                BufferAccessor<Cell> cellBufs = chunk.GetBufferAccessor(ref bufferTypeHandle);
                NativeArray<ValidArea> validAreas = chunk.GetNativeArray(ref validAreaComponentTypeHandle);

                for (int i = 0; i < entities.Length; i++)
                {
                    Entity entity = entities[i];

                    if (!ownerComponentLookup.HasComponent(entity))
                    {
                        continue;
                    }

                    Owner owner = ownerComponentLookup[entity];

                    if (!zoningInfoComponentLookup.HasComponent(owner.m_Owner))
                    {
                        continue;
                    }

                    if (!curveComponentLookup.HasComponent(owner.m_Owner))
                    {
                        continue;
                    }

                    Curve curve = curveComponentLookup[owner.m_Owner];
                    Block block = blocks[i];
                    DynamicBuffer<Cell> cells = cellBufs[i];
                    ValidArea validArea = validAreas[i];

                    float dot = BlockUtils.blockCurveDotProduct(block, curve);
                    ZoningInfo zi = zoningInfoComponentLookup[owner.m_Owner];

                    bool blocked =
                        (protectOccupiedCells && BlockUtils.isAnyCellOccupied(ref cells, ref block, ref validArea)) ||
                        (protectZonedCells && BlockUtils.isAnyCellZoned(ref cells, ref block, ref validArea));

                    if (!blocked)
                    {
                        BlockUtils.editBlockSizes(dot, zi, validArea, block, entity, entityCommandBuffer);
                        // No need to write ZoningInfo back to owner here; we're not changing it.
                    }

                    entityCommandBuffer.RemoveComponent<ZoningInfoUpdated>(entity);
                }
            }
        }

        private struct UpdateZoneData : IJobChunk
        {
            [ReadOnly] public ZoningMode zoningMode;
            [ReadOnly] public bool protectOccupiedCells;
            [ReadOnly] public bool protectZonedCells;

            [ReadOnly] public EntityTypeHandle entityTypeHandle;
            public ComponentTypeHandle<Block> blockComponentTypeHandle;
            public ComponentTypeHandle<ValidArea> validAreaComponentTypeHandle;
            public ComponentTypeHandle<Deleted> deletedTypeHandle;
            public BufferTypeHandle<Cell> bufferTypeHandle;
            public ComponentLookup<Owner> ownerComponentLookup;
            [ReadOnly] public ComponentLookup<Curve> curveComponentLookup;
            public ComponentLookup<ZoningInfo> zoningInfoComponentLookup;
            public ComponentLookup<Applied> appliedLookup;
            public EntityCommandBuffer entityCommandBuffer;
            public NativeParallelHashMap<float2, Entity> entitiesByStartPoint;
            public NativeParallelHashMap<float2, Entity> entitiesByEndPoint;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Block> blocks = chunk.GetNativeArray(ref blockComponentTypeHandle);
                NativeArray<Entity> entities = chunk.GetNativeArray(entityTypeHandle);
                BufferAccessor<Cell> cellBufs = chunk.GetBufferAccessor(ref bufferTypeHandle);
                NativeArray<ValidArea> validAreas = chunk.GetNativeArray(ref validAreaComponentTypeHandle);

                if (chunk.Has(ref deletedTypeHandle))
                {
                    return;
                }

                for (int i = 0; i < blocks.Length; i++)
                {
                    Entity entity = entities[i];

                    if (!ownerComponentLookup.HasComponent(entity))
                    {
                        continue;
                    }

                    Owner owner = ownerComponentLookup[entity];

                    if (!curveComponentLookup.HasComponent(owner.m_Owner))
                    {
                        continue;
                    }

                    Curve curve = curveComponentLookup[owner.m_Owner];

                    ZoningInfo zi = new ZoningInfo { zoningMode = zoningMode };

                    if (!appliedLookup.HasComponent(owner.m_Owner))
                    {
                        if (zoningInfoComponentLookup.HasComponent(owner.m_Owner))
                        {
                            zi = zoningInfoComponentLookup[owner.m_Owner];
                        }
                        else
                        {
                            zi = DefaultZI();
                        }
                    }
                    else
                    {
                        if (entitiesByStartPoint.TryGetValue(curve.m_Bezier.a.xz, out Entity s) &&
                            entitiesByEndPoint.TryGetValue(curve.m_Bezier.d.xz, out Entity e))
                        {
                            if (s == e && zoningInfoComponentLookup.HasComponent(s))
                            {
                                zi = zoningInfoComponentLookup[s];
                            }
                            else if (curveComponentLookup.HasComponent(s) && curveComponentLookup.HasComponent(e))
                            {
                                ZoningInfo sZI = zoningInfoComponentLookup.HasComponent(s)
                                    ? zoningInfoComponentLookup[s]
                                    : DefaultZI();
                                ZoningInfo eZI = zoningInfoComponentLookup.HasComponent(e)
                                    ? zoningInfoComponentLookup[e]
                                    : DefaultZI();

                                zi = sZI.Equals(eZI) ? sZI : DefaultZI();
                            }
                        }
                        else if (entitiesByEndPoint.TryGetValue(curve.m_Bezier.d.xz, out Entity eOnly) &&
                                 zoningInfoComponentLookup.HasComponent(eOnly))
                        {
                            zi = zoningInfoComponentLookup[eOnly];
                        }
                        else if (entitiesByStartPoint.TryGetValue(curve.m_Bezier.a.xz, out Entity sOnly) &&
                                 zoningInfoComponentLookup.HasComponent(sOnly))
                        {
                            zi = zoningInfoComponentLookup[sOnly];
                        }

                        if (zoningInfoComponentLookup.HasComponent(owner.m_Owner))
                        {
                            zi = zoningInfoComponentLookup[owner.m_Owner];
                        }
                    }

                    Block block = blocks[i];
                    DynamicBuffer<Cell> cells = cellBufs[i];
                    ValidArea validArea = validAreas[i];

                    float dot = BlockUtils.blockCurveDotProduct(block, curve);

                    if (protectOccupiedCells && BlockUtils.isAnyCellOccupied(ref cells, ref block, ref validArea))
                    {
                        continue;
                    }

                    if (protectZonedCells && BlockUtils.isAnyCellZoned(ref cells, ref block, ref validArea))
                    {
                        continue;
                    }

                    BlockUtils.editBlockSizes(dot, zi, validArea, block, entity, entityCommandBuffer);

                    // Safe: add if missing, set if present.
                    AddOrSetZoningInfo(entityCommandBuffer, zoningInfoComponentLookup, owner.m_Owner, zi);
                }
            }
        }

        private struct CollectDeletedCurves : IJobChunk
        {
            public ComponentTypeHandle<Owner> ownerTypeHandle;
            public ComponentTypeHandle<Deleted> deletedTypeHandle;
            public ComponentLookup<Curve> curveLookup;
            public ComponentLookup<Deleted> deletedLookup;
            public NativeParallelHashMap<float2, Entity> curvesByStartPoint;
            public NativeParallelHashMap<float2, Entity> curvesByEndPoint;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Owner> owners = chunk.GetNativeArray(ref ownerTypeHandle);

                if (!chunk.Has(ref deletedTypeHandle))
                {
                    return;
                }

                for (int i = 0; i < owners.Length; i++)
                {
                    Owner owner = owners[i];

                    if (curveLookup.HasComponent(owner.m_Owner))
                    {
                        Curve curve = curveLookup[owner.m_Owner];
                        curvesByStartPoint.TryAdd(curve.m_Bezier.a.xz, owner.m_Owner);
                        curvesByEndPoint.TryAdd(curve.m_Bezier.d.xz, owner.m_Owner);
                    }
                }
            }
        }

        private struct DisposeHashMaps : IJob
        {
            public NativeParallelHashMap<float2, Entity> toDispose1;
            public NativeParallelHashMap<float2, Entity> toDispose2;

            public void Execute()
            {
                toDispose1.Dispose();
                toDispose2.Dispose();
            }
        }
    }
}
