using Colossal.Mathematics;
using Game;
using Game.Common;
using Game.Net;
using Game.Zones;
using System.Diagnostics;
using System.Linq;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting;
using ZoningToolkit.Components;
using ZoningToolkit.Utils;

namespace ZoningToolkit.Systems
{
    // [UpdateAfter(typeof(BlockSystem))]
    public partial class ZoningToolkitModSystem : GameSystemBase
    {

        private EntityQuery newEntityQuery;
        private EntityQuery updateEntityQuery;
        private ComponentTypeHandle<Block> blockComponentTypeHandle;
        private EntityTypeHandle entityTypeHandle;
        private ComponentTypeHandle<ValidArea> validAreaComponentTypeHandle;
        private ComponentTypeHandle<Deleted> deletedTypeHandle;
        public ComponentTypeHandle<Owner> ownerTypeHandle;

        public BufferTypeHandle<Cell> cellBufferTypeHandle;

        public ComponentLookup<Owner> ownerComponentLookup;
        [ReadOnly]
        protected ComponentLookup<Curve> curveComponentLookup;
        private ComponentLookup<ZoningInfo> zoningInfoComponentLookup;
        private ComponentLookup<Deleted> deletedLookup;
        private ComponentLookup<Applied> appliedLookup;
        private ComponentLookup<Updated> updatedLookup;
        private ComponentLookup<ZoningInfoUpdated> zoningInfoUpdatedLookup;
        private ModificationBarrier4B modificationBarrier4B;
        internal ZoningMode zoningMode;


        protected override void OnCreate()
        {
            this.getLogger().Info("Creating ZoningToolkitMod GameSystem.");
            base.OnCreate();

            this.newEntityQuery = this.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] {
                    ComponentType.ReadWrite<Block>(),
                    ComponentType.ReadWrite<Owner>(),
                    ComponentType.ReadOnly<Cell>(),
                    ComponentType.ReadOnly<ValidArea>()
                },
                Any = new ComponentType[] {
                    ComponentType.ReadOnly<Created>(),
                    ComponentType.ReadOnly<Deleted>()
                }
            });

            this.updateEntityQuery = this.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] {
                    ComponentType.ReadWrite<Block>(),
                    ComponentType.ReadWrite<Owner>(),
                    ComponentType.ReadOnly<Cell>(),
                    ComponentType.ReadOnly<ValidArea>(),
                    ComponentType.ReadOnly<ZoningInfoUpdated>(),
                },
            });

            // Component to use
            this.blockComponentTypeHandle = this.GetComponentTypeHandle<Block>();
            this.ownerComponentLookup = this.GetComponentLookup<Owner>();
            this.curveComponentLookup = this.GetComponentLookup<Curve>(true);
            this.zoningInfoComponentLookup = this.GetComponentLookup<ZoningInfo>();
            this.entityTypeHandle = this.GetEntityTypeHandle();
            this.validAreaComponentTypeHandle = this.GetComponentTypeHandle<ValidArea>();
            this.deletedTypeHandle = this.GetComponentTypeHandle<Deleted>();
            this.ownerTypeHandle = this.GetComponentTypeHandle<Owner>();
            this.deletedLookup = this.GetComponentLookup<Deleted>();
            this.cellBufferTypeHandle = this.GetBufferTypeHandle<Cell>();
            this.appliedLookup = this.GetComponentLookup<Applied>();
            this.updatedLookup = this.GetComponentLookup<Updated>();
            this.zoningInfoUpdatedLookup = this.GetComponentLookup<ZoningInfoUpdated>();

            // other systems to use
            this.modificationBarrier4B = World.GetOrCreateSystemManaged<ModificationBarrier4B>();

            this.RequireAnyForUpdate(this.newEntityQuery, this.updateEntityQuery);
        }

        [Preserve]
        protected override void OnUpdate()
        {
            this.getLogger().Info("On Update ZoneToolkit System.");
            this.blockComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.ownerComponentLookup.Update(ref this.CheckedStateRef);
            this.curveComponentLookup.Update(ref this.CheckedStateRef);
            this.zoningInfoComponentLookup.Update(ref this.CheckedStateRef);
            this.entityTypeHandle.Update(ref this.CheckedStateRef);
            this.validAreaComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.deletedTypeHandle.Update(ref CheckedStateRef);
            this.ownerTypeHandle.Update(ref CheckedStateRef);
            this.deletedLookup.Update(ref CheckedStateRef);
            this.cellBufferTypeHandle.Update(ref CheckedStateRef);
            this.appliedLookup.Update(ref CheckedStateRef);
            this.updatedLookup.Update(ref CheckedStateRef);
            this.zoningInfoUpdatedLookup.Update(ref CheckedStateRef);

            EntityCommandBuffer entityCommandBuffer = this.modificationBarrier4B.CreateCommandBuffer();

            NativeParallelHashMap<float2, Entity> deletedEntitiesByStartPoint = new NativeParallelHashMap<float2, Entity>(32, Allocator.Temp);
            NativeParallelHashMap<float2, Entity> deletedEntitiesByEndPoint = new NativeParallelHashMap<float2, Entity>(32, Allocator.Temp);

            JobHandle outJobHandle = this.Dependency;

            JobHandle collectDeletedEntities = new CollectDeletedCurves()
            {
                curveLookup = curveComponentLookup,
                deletedTypeHandle = deletedTypeHandle,
                ownerTypeHandle = ownerTypeHandle,
                deletedLookup = deletedLookup,
                curvesByStartPoint = deletedEntitiesByStartPoint,
                curvesByEndPoint = deletedEntitiesByEndPoint
            }.Schedule(this.newEntityQuery, outJobHandle);
            outJobHandle = JobHandle.CombineDependencies(collectDeletedEntities, outJobHandle);

            if (!this.newEntityQuery.IsEmptyIgnoreFilter)
            {
                this.getLogger().Info("Updating zoning for newly created roads.");
                JobHandle jobHandle = new UpdateZoneData()
                {
                    blockComponentTypeHandle = this.blockComponentTypeHandle,
                    validAreaComponentTypeHandle = this.validAreaComponentTypeHandle,
                    curveComponentLookup = this.curveComponentLookup,
                    entityCommandBuffer = entityCommandBuffer,
                    entityTypeHandle = this.entityTypeHandle,
                    ownerComponentLookup = this.ownerComponentLookup,
                    zoningMode = this.zoningMode,
                    zoningInfoComponentLookup = this.zoningInfoComponentLookup,
                    bufferTypeHandle = this.cellBufferTypeHandle,
                    appliedLookup = this.appliedLookup,
                    entitiesByStartPoint = deletedEntitiesByStartPoint,
                    entitiesByEndPoint = deletedEntitiesByEndPoint,
                }.Schedule(this.newEntityQuery, outJobHandle);
                outJobHandle = JobHandle.CombineDependencies(outJobHandle, jobHandle);
            }

            if (!this.updateEntityQuery.IsEmptyIgnoreFilter)
            {
                this.getLogger().Info("Updating zoning for existing roads.");
                JobHandle jobHandle = new UpdateZoningInfo()
                {
                    blockComponentTypeHandle = this.blockComponentTypeHandle,
                    validAreaComponentTypeHandle = this.validAreaComponentTypeHandle,
                    curveComponentLookup = this.curveComponentLookup,
                    entityCommandBuffer = entityCommandBuffer,
                    entityTypeHandle = this.entityTypeHandle,
                    ownerComponentLookup = this.ownerComponentLookup,
                    zoningMode = this.zoningMode,
                    zoningInfoComponentLookup = this.zoningInfoComponentLookup,
                    zoningInfoUpdateComponentLookup = this.zoningInfoUpdatedLookup,
                    bufferTypeHandle = this.cellBufferTypeHandle,
                    updatedLookup = this.updatedLookup
                }.Schedule(this.updateEntityQuery, outJobHandle);
                outJobHandle = JobHandle.CombineDependencies(outJobHandle, jobHandle);
            }

            JobHandle disposeMaps = new DisposeHashMaps()
            {
                toDispose1 = deletedEntitiesByStartPoint,
                toDispose2 = deletedEntitiesByEndPoint
            }.Schedule(outJobHandle);

            this.Dependency = JobHandle.CombineDependencies(disposeMaps, outJobHandle);

            this.modificationBarrier4B.AddJobHandleForProducer(this.Dependency);
        }

        public void setZoningMode(string zoningMode)
        {
            this.getLogger().Info($"Changing zoning mode to ${zoningMode}");
            switch (zoningMode)
            {
                case "Left":
                    this.zoningMode = ZoningMode.Left;
                    break;
                case "Right":
                    this.zoningMode = ZoningMode.Right;
                    break;
                case "Default":
                    this.zoningMode = ZoningMode.Default;
                    break;
                case "None":
                    this.zoningMode = ZoningMode.None;
                    break;
                default:
                    this.zoningMode = ZoningMode.Default;
                    break;
            }
        }

        public Vector2 GetTangent(Bezier4x2 curve, float t)
        {
            // Calculate the derivative of the Bezier curve
            float2 derivative = 3 * math.pow(1 - t, 2) * (curve.b - curve.a) +
                                    6 * (1 - t) * t * (curve.c - curve.b) +
                                    3 * math.pow(t, 2) * (curve.d - curve.c);
            return new Vector2(derivative.x, derivative.y);
        }

        private struct UpdateZoningInfo : IJobChunk
        {
            [ReadOnly]
            public ZoningMode zoningMode;
            [ReadOnly]
            public EntityTypeHandle entityTypeHandle;
            public ComponentTypeHandle<Block> blockComponentTypeHandle;
            public ComponentTypeHandle<ValidArea> validAreaComponentTypeHandle;
            public BufferTypeHandle<Cell> bufferTypeHandle;
            public ComponentLookup<Owner> ownerComponentLookup;
            [ReadOnly]
            public ComponentLookup<Curve> curveComponentLookup;
            public ComponentLookup<ZoningInfo> zoningInfoComponentLookup;
            public ComponentLookup<ZoningInfoUpdated> zoningInfoUpdateComponentLookup;
            public EntityCommandBuffer entityCommandBuffer;
            public ComponentLookup<Updated> updatedLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var blocks = chunk.GetNativeArray(ref blockComponentTypeHandle);
                var entities = chunk.GetNativeArray(entityTypeHandle);
                var cellBufs = chunk.GetBufferAccessor(ref bufferTypeHandle);
                var validAreas = chunk.GetNativeArray(ref validAreaComponentTypeHandle);

                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    if (!ownerComponentLookup.HasComponent(entity)) continue;

                    var owner = ownerComponentLookup[entity];
                    if (!zoningInfoComponentLookup.HasComponent(owner.m_Owner)) continue;
                    if (!zoningInfoUpdateComponentLookup.HasComponent(entity)) continue;
                    if (!curveComponentLookup.HasComponent(owner.m_Owner)) continue;

                    var curve = curveComponentLookup[owner.m_Owner];
                    var block = blocks[i];
                    var cells = cellBufs[i];
                    var validArea = validAreas[i];

                    var dot = BlockUtils.blockCurveDotProduct(block, curve);
                    var zi = zoningInfoComponentLookup[owner.m_Owner];

                    if (!BlockUtils.isAnyCellOccupied(ref cells, ref block, ref validArea))
                    {
                        BlockUtils.editBlockSizes(dot, zi, validArea, block, entity, entityCommandBuffer);
                        entityCommandBuffer.AddComponent(owner.m_Owner, zi);
                    }

                    entityCommandBuffer.RemoveComponent<ZoningInfoUpdated>(entity);
                }
            }


        }

        // [BurstCompile]
        private struct UpdateZoneData : IJobChunk
        {
            [ReadOnly]
            public ZoningMode zoningMode;
            [ReadOnly]
            public EntityTypeHandle entityTypeHandle;
            public ComponentTypeHandle<Block> blockComponentTypeHandle;

            public ComponentTypeHandle<ValidArea> validAreaComponentTypeHandle;

            public ComponentTypeHandle<Deleted> deletedTypeHandle;
            public BufferTypeHandle<Cell> bufferTypeHandle;
            public ComponentLookup<Owner> ownerComponentLookup;
            [ReadOnly]
            public ComponentLookup<Curve> curveComponentLookup;
            public ComponentLookup<ZoningInfo> zoningInfoComponentLookup;

            public ComponentLookup<Applied> appliedLookup;
            public EntityCommandBuffer entityCommandBuffer;
            public NativeParallelHashMap<float2, Entity> entitiesByStartPoint;
            public NativeParallelHashMap<float2, Entity> entitiesByEndPoint;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var blocks = chunk.GetNativeArray(ref blockComponentTypeHandle);
                var entities = chunk.GetNativeArray(entityTypeHandle);
                var cellBufs = chunk.GetBufferAccessor(ref bufferTypeHandle);
                var validAreas = chunk.GetNativeArray(ref validAreaComponentTypeHandle);

                if (chunk.Has(ref deletedTypeHandle))
                    return;

                for (int i = 0; i < blocks.Length; i++)
                {
                    var entity = entities[i];
                    var zi = new ZoningInfo { zoningMode = zoningMode };

                    if (!ownerComponentLookup.HasComponent(entity)) continue;
                    var owner = ownerComponentLookup[entity];
                    if (!curveComponentLookup.HasComponent(owner.m_Owner)) continue;
                    var curve = curveComponentLookup[owner.m_Owner];

                    if (!appliedLookup.HasComponent(owner.m_Owner))
                    {
                        zi = zoningInfoComponentLookup.HasComponent(owner.m_Owner)
                            ? zoningInfoComponentLookup[owner.m_Owner]
                            : new ZoningInfo { zoningMode = ZoningMode.Default };
                    }
                    else
                    {
                        if (entitiesByStartPoint.TryGetValue(curve.m_Bezier.a.xz, out var s) &
                            entitiesByEndPoint.TryGetValue(curve.m_Bezier.d.xz, out var e))
                        {
                            if (s == e && zoningInfoComponentLookup.HasComponent(s))
                            {
                                zi = zoningInfoComponentLookup[s];
                            }
                            else if (curveComponentLookup.HasComponent(s) && curveComponentLookup.HasComponent(e))
                            {
                                var sZI = zoningInfoComponentLookup.HasComponent(s) ? zoningInfoComponentLookup[s] : default;
                                var eZI = zoningInfoComponentLookup.HasComponent(e) ? zoningInfoComponentLookup[e] : default;
                                zi = (sZI.Equals(eZI)) ? sZI : new ZoningInfo { zoningMode = ZoningMode.Default };
                            }
                        }
                        else if (entitiesByEndPoint.TryGetValue(curve.m_Bezier.d.xz, out var eOnly) &&
                                 zoningInfoComponentLookup.HasComponent(eOnly))
                        {
                            zi = zoningInfoComponentLookup[eOnly];
                        }
                        else if (entitiesByStartPoint.TryGetValue(curve.m_Bezier.a.xz, out var sOnly) &&
                                 zoningInfoComponentLookup.HasComponent(sOnly))
                        {
                            zi = zoningInfoComponentLookup[sOnly];
                        }

                        if (zoningInfoComponentLookup.HasComponent(owner.m_Owner))
                            zi = zoningInfoComponentLookup[owner.m_Owner];
                    }

                    var block = blocks[i];
                    var cells = cellBufs[i];
                    var validArea = validAreas[i];

                    var dot = BlockUtils.blockCurveDotProduct(block, curve);
                    if (BlockUtils.isAnyCellOccupied(ref cells, ref block, ref validArea))
                        continue;

                    BlockUtils.editBlockSizes(dot, zi, validArea, block, entity, entityCommandBuffer);
                    entityCommandBuffer.AddComponent(owner.m_Owner, zi);
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
                this.getLogger().Info("Executing Collect Deleted Curves Job.");
                NativeArray<Owner> owners = chunk.GetNativeArray(ref this.ownerTypeHandle);

                if (chunk.Has(ref this.deletedTypeHandle))
                {
                    for (int i = 0; i < owners.Length; i++)
                    {
                        Owner owner = owners[i];

                        if (curveLookup.HasComponent(owner.m_Owner))
                        {
                            this.getLogger().Info("Adding curve to hash maps.");

                            Curve curve = curveLookup[owner.m_Owner];

                            curvesByStartPoint.Add(curve.m_Bezier.a.xz, owner.m_Owner);
                            curvesByEndPoint.Add(curve.m_Bezier.d.xz, owner.m_Owner);
                        }
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
                toDispose1.Clear();
                toDispose2.Clear();
                toDispose1.Dispose();
                toDispose2.Dispose();
            }

        }
    }
}