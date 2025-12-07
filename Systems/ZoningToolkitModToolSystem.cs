// Systems/ZoningToolkitModToolSystem.cs

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Game;
    using Game.Areas;
    using Game.Common;
    using Game.Input;
    using Game.Net;
    using Game.Prefabs;
    using Game.Tools;
    using Game.Zones;
    using Unity.Entities;
    using Unity.Jobs;
    using UnityEngine;
    using ZoningToolkit.Components;

    // Job: highlight a net entity (and its edge endpoints)
    public struct HighlightEntitiesJob : IJob
    {
        public EntityCommandBuffer commandBuffer;
        public Entity entityToHighlight;
        public ComponentLookup<Edge> edgeLookup;

        public void Execute()
        {
            if (entityToHighlight == Entity.Null)
                return;

            commandBuffer.AddComponent<Highlighted>(entityToHighlight);
            commandBuffer.AddComponent<Updated>(entityToHighlight);

            if (edgeLookup.HasComponent(entityToHighlight))
            {
                Edge edge = edgeLookup[entityToHighlight];
                commandBuffer.AddComponent<Updated>(edge.m_Start);
                commandBuffer.AddComponent<Updated>(edge.m_End);
            }
        }
    }

    // Job: remove highlight from net entity (and mark endpoints updated)
    public struct UnHighlightEntitiesJob : IJob
    {
        public EntityCommandBuffer commandBuffer;
        public Entity entityToUnhighlight;
        public ComponentLookup<Edge> edgeLookup;

        public void Execute()
        {
            if (entityToUnhighlight == Entity.Null)
                return;

            commandBuffer.RemoveComponent<Highlighted>(entityToUnhighlight);
            commandBuffer.AddComponent<Updated>(entityToUnhighlight);

            if (edgeLookup.HasComponent(entityToUnhighlight))
            {
                Edge edge = edgeLookup[entityToUnhighlight];
                commandBuffer.AddComponent<Updated>(edge.m_Start);
                commandBuffer.AddComponent<Updated>(edge.m_End);
            }
        }
    }

    // Job: apply ZoningInfo to a single net entity and mark its blocks for update
    public struct ApplyZoningInfoJob : IJob
    {
        public Entity entity;

        public ComponentLookup<Curve> curveLookup;
        public ComponentLookup<ZoningInfo> zoningLookup;
        public BufferLookup<SubBlock> subBlockLookup;
        public ComponentLookup<Edge> edgeLookup;

        public EntityCommandBuffer ecb;
        public ZoningInfo newZoningInfo;

        public void Execute()
        {
            if (entity == Entity.Null)
                return;

            if (curveLookup.HasComponent(entity))
            {
                ecb.AddComponent(entity, newZoningInfo);

                if (subBlockLookup.HasBuffer(entity))
                {
                    DynamicBuffer<SubBlock> buffer = subBlockLookup[entity];
                    foreach (var sb in buffer)
                    {
                        ecb.AddComponent<ZoningInfoUpdated>(sb.m_SubBlock);
                    }
                }
            }

            // Clear highlight and mark updated for visual refresh
            ecb.RemoveComponent<Highlighted>(entity);
            ecb.AddComponent<Updated>(entity);

            if (edgeLookup.HasComponent(entity))
            {
                Edge edge = edgeLookup[entity];
                ecb.AddComponent<Updated>(edge.m_Start);
                ecb.AddComponent<Updated>(edge.m_End);
            }
        }
    }

    // Tool that lets the player apply a zoning mode to existing roads.
    public partial class ZoningToolkitModToolSystem : ToolBaseSystem
    {
        // Helper struct for per-frame state
        private struct OnUpdateMemory
        {
            public JobHandle currentInputDeps;
            public EntityCommandBuffer commandBufferSystem;
        }

        // Persistent tool state
        internal struct WorkingState
        {
            internal Entity lastRaycastEntity;
            internal ZoningMode zoningMode;
        }

        private ToolOutputBarrier m_ToolOutputBarrier = null!;
        private ToolSystem m_ToolSystem = null!;
        private ToolBaseSystem? m_PreviousToolSystem;
        private NetToolSystem m_NetToolSystem = null!;

        private TypeHandle m_TypeHandle;
        private OnUpdateMemory m_OnUpdateMemory;

        internal bool toolEnabled
        {
            get; private set;
        }
        internal WorkingState workingState;

        public override string toolID => "ZoneTools Zoning Tool";

        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            m_TypeHandle.__AssignHandles(ref CheckedStateRef);
        }

        protected override void OnCreate()
        {
            Mod.s_Log.Info($"Creating {toolID}");
            base.OnCreate();

            Enabled = false;

            // Use the built-in Apply action from ToolBaseSystem.
            _ = new DisplayNameOverride(nameof(ZoningToolkitModToolSystem), applyAction, "ZoneTools Apply", 20);

            m_ToolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();

            toolEnabled = false;
            workingState.lastRaycastEntity = Entity.Null;
            workingState.zoningMode = ZoningMode.Default;

            // Ensure this tool appears once in the tool list
            List<ToolBaseSystem> tools = m_ToolSystem.tools;
            ToolBaseSystem? existing = null;
            foreach (ToolBaseSystem tool in tools)
            {
                if (tool == this)
                {
                    existing = tool;
                    break;
                }
            }

            if (existing != null)
            {
                tools.Remove(this);
            }
            tools.Add(this);

            Mod.s_Log.Info($"Done creating {toolID}");
        }

        protected override void OnStartRunning()
        {
            Mod.s_Log.Info($"Started running tool {toolID}");
            base.OnStartRunning();

            toolEnabled = true;
            applyAction.enabled = true;
            m_OnUpdateMemory = default;
            workingState.lastRaycastEntity = Entity.Null;
        }

        protected override void OnStopRunning()
        {
            Mod.s_Log.Info($"Stopped running tool {toolID}");
            base.OnStopRunning();

            m_OnUpdateMemory.currentInputDeps.Complete();
            toolEnabled = false;
            workingState.lastRaycastEntity = Entity.Null;
        }

        public override void InitializeRaycast()
        {
            base.InitializeRaycast();

            m_ToolRaycastSystem.typeMask = TypeMask.Lanes | TypeMask.Net;
            m_ToolRaycastSystem.netLayerMask = Layer.Road;
            m_ToolRaycastSystem.areaTypeMask = AreaTypeMask.Surfaces;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (m_FocusChanged || !toolEnabled)
                return inputDeps;

            applyMode = ApplyMode.Clear;

            requireZones = true;
            requireAreas |= AreaTypeMask.Lots;

            if (GetPrefab() != null)
            {
                UpdateInfoview(m_ToolSystem.actionMode.IsEditor()
                    ? Entity.Null
                    : m_PrefabSystem.GetEntity(GetPrefab()));
            }

            m_TypeHandle.__UpdateComponents(ref CheckedStateRef);
            m_OnUpdateMemory = new OnUpdateMemory
            {
                currentInputDeps = inputDeps,
                commandBufferSystem = m_ToolOutputBarrier.CreateCommandBuffer()
            };

            // 1) Hover highlight
            HandleHoverHighlight();

            // 2) Apply when user clicks (on mouse release)
            if (applyAction.WasReleasedThisFrame())
            {
                ApplyZoningToCurrentEntity();
            }

            m_ToolOutputBarrier.AddJobHandleForProducer(m_OnUpdateMemory.currentInputDeps);
            return m_OnUpdateMemory.currentInputDeps;
        }

        private void HandleHoverHighlight()
        {
            Entity previous = workingState.lastRaycastEntity;

            if (GetRaycastResult(out Entity hitEntity, out RaycastHit _))
            {
                if (hitEntity != previous)
                {
                    // Unhighlight previous
                    if (previous != Entity.Null)
                    {
                        var unhighlightJob = new UnHighlightEntitiesJob
                        {
                            commandBuffer = m_OnUpdateMemory.commandBufferSystem,
                            entityToUnhighlight = previous,
                            edgeLookup = m_TypeHandle.__Game_Edge_RW_ComponentLookup
                        }.Schedule(m_OnUpdateMemory.currentInputDeps);

                        m_OnUpdateMemory.currentInputDeps =
                            JobHandle.CombineDependencies(m_OnUpdateMemory.currentInputDeps, unhighlightJob);
                    }

                    // Highlight new
                    workingState.lastRaycastEntity = hitEntity;

                    var highlightJob = new HighlightEntitiesJob
                    {
                        commandBuffer = m_OnUpdateMemory.commandBufferSystem,
                        entityToHighlight = workingState.lastRaycastEntity,
                        edgeLookup = m_TypeHandle.__Game_Edge_RW_ComponentLookup
                    }.Schedule(m_OnUpdateMemory.currentInputDeps);

                    m_OnUpdateMemory.currentInputDeps =
                        JobHandle.CombineDependencies(m_OnUpdateMemory.currentInputDeps, highlightJob);
                }
            }
            else
            {
                // No hit – clear previous highlight if any
                if (previous != Entity.Null)
                {
                    workingState.lastRaycastEntity = Entity.Null;

                    var unhighlightJob = new UnHighlightEntitiesJob
                    {
                        commandBuffer = m_OnUpdateMemory.commandBufferSystem,
                        entityToUnhighlight = previous,
                        edgeLookup = m_TypeHandle.__Game_Edge_RW_ComponentLookup
                    }.Schedule(m_OnUpdateMemory.currentInputDeps);

                    m_OnUpdateMemory.currentInputDeps =
                        JobHandle.CombineDependencies(m_OnUpdateMemory.currentInputDeps, unhighlightJob);
                }
            }
        }

        private void ApplyZoningToCurrentEntity()
        {
            Entity entity = workingState.lastRaycastEntity;
            if (entity == Entity.Null)
                return;

            Mod.s_Log.Info($"Applying zoning mode {workingState.zoningMode} to entity {entity}");

            var job = new ApplyZoningInfoJob
            {
                entity = entity,
                curveLookup = m_TypeHandle.__Game_Net_Curve_RW_ComponentLookup,
                zoningLookup = m_TypeHandle.__Game_Zoning_Info_RW_ComponentLookup,
                subBlockLookup = m_TypeHandle.__Game_SubBlock_RW_BufferLookup,
                edgeLookup = m_TypeHandle.__Game_Edge_RW_ComponentLookup,
                ecb = m_OnUpdateMemory.commandBufferSystem,
                newZoningInfo = new ZoningInfo { zoningMode = workingState.zoningMode }
            }.Schedule(m_OnUpdateMemory.currentInputDeps);

            m_OnUpdateMemory.currentInputDeps =
                JobHandle.CombineDependencies(m_OnUpdateMemory.currentInputDeps, job);

            applyMode = ApplyMode.Apply;
        }

        public override PrefabBase GetPrefab()
        {
            return null!;
        }

        public override bool TrySetPrefab(PrefabBase prefab)
        {
            return false;
        }

        internal void EnableTool()
        {
            if (!toolEnabled)
            {
                toolEnabled = true;
                m_PreviousToolSystem = m_ToolSystem.activeTool;
                m_ToolSystem.activeTool = this;
            }
        }

        internal void DisableTool()
        {
            if (toolEnabled)
            {
                toolEnabled = false;
                if (m_ToolSystem.activeTool == this)
                {
                    m_ToolSystem.activeTool = m_PreviousToolSystem;
                }
            }
        }

        private struct TypeHandle
        {
            public ComponentLookup<Curve> __Game_Net_Curve_RW_ComponentLookup;
            public ComponentLookup<ZoningInfo> __Game_Zoning_Info_RW_ComponentLookup;
            public BufferLookup<SubBlock> __Game_SubBlock_RW_BufferLookup;
            public ComponentLookup<Block> __Game_Block_RW_ComponentLookup;
            public ComponentLookup<Edge> __Game_Edge_RW_ComponentLookup;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                __Game_Net_Curve_RW_ComponentLookup = state.GetComponentLookup<Curve>(isReadOnly: false);
                __Game_Zoning_Info_RW_ComponentLookup = state.GetComponentLookup<ZoningInfo>(isReadOnly: false);
                __Game_SubBlock_RW_BufferLookup = state.GetBufferLookup<SubBlock>(isReadOnly: false);
                __Game_Block_RW_ComponentLookup = state.GetComponentLookup<Block>(isReadOnly: false);
                __Game_Edge_RW_ComponentLookup = state.GetComponentLookup<Edge>(isReadOnly: false);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __UpdateComponents(ref SystemState state)
            {
                __Game_Net_Curve_RW_ComponentLookup.Update(ref state);
                __Game_Zoning_Info_RW_ComponentLookup.Update(ref state);
                __Game_SubBlock_RW_BufferLookup.Update(ref state);
                __Game_Block_RW_ComponentLookup.Update(ref state);
                __Game_Edge_RW_ComponentLookup.Update(ref state);
            }
        }
    }
}
