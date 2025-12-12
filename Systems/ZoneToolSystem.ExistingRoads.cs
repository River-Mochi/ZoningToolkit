// Systems/ZoneToolSystem.ExistingRoads.cs
// Update tool Existing Roads: hover, drag-select existing roads, and apply Zone Tools modes.

namespace ZoningToolkit.Systems
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Game.Areas;
    using Game.Common;
    using Game.Input;
    using Game.Net;
    using Game.Prefabs;
    using Game.Tools;
    using Game.Zones;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using ZoningToolkit.Components;
    using ZoningToolkit.Utils;

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
    /// Job: for older saves that don't have ZoningInfo, infer zoning from existing blocks.
    /// (Currently unused, but kept for compatibility if needed later.)
    /// </summary>
    public struct BackwardsCompatibilityZoningInfo : IJob
    {
        public Entity BackwardsCompatibilityEntity;
        public ComponentLookup<Curve> CurveLookup;
        public ComponentLookup<ZoningInfo> ZoningInfoLookup;
        public BufferLookup<SubBlock> SubBlockBufferLookup;
        public ComponentLookup<Block> BlockLookup;
        public EntityCommandBuffer CommandBuffer;

        public void Execute()
        {
            if (ZoningInfoLookup.HasComponent(BackwardsCompatibilityEntity))
            {
                return;
            }

            if (!CurveLookup.HasComponent(BackwardsCompatibilityEntity))
            {
                return;
            }

            Curve curve = CurveLookup[BackwardsCompatibilityEntity];
            bool leftBlock = false;
            bool rightBlock = false;

            if (SubBlockBufferLookup.HasBuffer(BackwardsCompatibilityEntity))
            {
                DynamicBuffer<SubBlock> subBlocks = SubBlockBufferLookup[BackwardsCompatibilityEntity];

                foreach (SubBlock item in subBlocks)
                {
                    Block block = BlockLookup[item.m_SubBlock];
                    float dot = BlockUtils.blockCurveDotProduct(block, curve);

                    if (dot > 0f)
                    {
                        if (block.m_Size.y > 0)
                        {
                            leftBlock = true;
                        }
                    }
                    else
                    {
                        if (block.m_Size.y > 0)
                        {
                            rightBlock = true;
                        }
                    }
                }
            }

            ZoningMode mode;
            if (leftBlock && rightBlock)
            {
                mode = ZoningMode.Default;
            }
            else if (rightBlock)
            {
                mode = ZoningMode.Right;
            }
            else if (leftBlock)
            {
                mode = ZoningMode.Left;
            }
            else
            {
                mode = ZoningMode.None;
            }

            CommandBuffer.AddComponent(
                BackwardsCompatibilityEntity,
                new ZoningInfo { zoningMode = mode });
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

    /// <summary>
    /// Main Zone Tools "update existing roads" tool.
    /// </summary>
    internal sealed partial class ZoneToolSystemExistingRoads : ToolBaseSystem
    {
        /// <summary>
        /// Per-frame scratch data (jobs & ECB).
        /// </summary>
        private struct OnUpdateMemory
        {
            public JobHandle CurrentInputDeps;
            public EntityCommandBuffer CommandBuffer;
        }

        /// <summary>
        /// State kept while the tool is active.
        /// </summary>
        internal struct WorkingState
        {
            internal Entity lastRaycastEntity;
            internal NativeHashSet<Entity> lastRaycastEntities;
            internal ZoningMode zoningMode;
        }

        private ToolOutputBarrier m_ToolOutputBarrier = null!;
        private NetToolSystem m_NetToolSystem = null!;
        private ToolBaseSystem? m_PreviousToolSystem;
        private ZoneToolSystemExistingRoadsStateMachine m_StateMachine = null!;
        private TypeHandle m_TypeHandle;
        private OnUpdateMemory m_OnUpdateMemory;
        private ZoneToolBridgeUI m_UISystem = null!;

        internal bool toolEnabled
        {
            get; private set;
        }

        internal WorkingState workingState;

        public override string toolID => "Zone Tools Zoning Tool";

        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            m_TypeHandle.__AssignHandles(ref CheckedStateRef);
        }

        protected override void OnCreate()
        {
            Mod.s_Log.Info($"Creating {toolID}.");
            base.OnCreate();

            Enabled = false;

            m_ToolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_UISystem = World.GetOrCreateSystemManaged<ZoneToolBridgeUI>();

            toolEnabled = false;

            workingState.lastRaycastEntity = Entity.Null;
            workingState.lastRaycastEntities = default;
            workingState.zoningMode = ZoningMode.Default;

            // Simple click / drag state machine using the vanilla Apply action (LMB by default).
            m_StateMachine = new ZoneToolSystemExistingRoadsStateMachine(
                new Dictionary<(ZoneToolSystemExistingRoadsState previous, ZoneToolSystemExistingRoadsState next), StateCallback>
                {
                    { (ZoneToolSystemExistingRoadsState.Default,   ZoneToolSystemExistingRoadsState.Selected),  EntityHighlighted },
                    { (ZoneToolSystemExistingRoadsState.Default,   ZoneToolSystemExistingRoadsState.Default),   HoverUpdate      },
                    { (ZoneToolSystemExistingRoadsState.Default,   ZoneToolSystemExistingRoadsState.Selecting), StartDragSelect  },
                    { (ZoneToolSystemExistingRoadsState.Selecting, ZoneToolSystemExistingRoadsState.Selecting), KeepDragSelect   },
                    { (ZoneToolSystemExistingRoadsState.Selecting, ZoneToolSystemExistingRoadsState.Selected),  StopDragSelect   },
                });

            // Ensure the tool is registered only once.
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

            Mod.s_Log.Info($"Done creating {toolID}.");
        }

        protected override void OnStartRunning()
        {
            Mod.s_Log.Info($"Started running tool {toolID}");
            base.OnStartRunning();

            toolEnabled = true;

            // Enable Apply (LMB) and Secondary Apply (RMB) actions
            applyAction.shouldBeEnabled = true;
            secondaryApplyAction.shouldBeEnabled = true;

            m_OnUpdateMemory = default;
            workingState.lastRaycastEntity = Entity.Null;
            workingState.lastRaycastEntities = new NativeHashSet<Entity>(32, Allocator.Persistent);
            workingState.zoningMode = m_UISystem.CurrentZoningMode;

            m_StateMachine.Reset();
        }

        protected override void OnStopRunning()
        {
            Mod.s_Log.Info($"Stopped running tool {toolID}");
            base.OnStopRunning();

            toolEnabled = false;

            applyAction.shouldBeEnabled = false;
            secondaryApplyAction.shouldBeEnabled = false;

            if (workingState.lastRaycastEntities.IsCreated)
            {
                workingState.lastRaycastEntities.Dispose();
            }

            m_OnUpdateMemory.CurrentInputDeps.Complete();
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
            // If focus changed or the tool isn't active, do nothing.
            if (m_FocusChanged || !toolEnabled)
            {
                return inputDeps;
            }

            applyMode = ApplyMode.Clear;

            requireZones = true;
            requireAreas |= AreaTypeMask.Lots;

            // Keep component lookups fresh.
            m_TypeHandle.__UpdateComponents(ref CheckedStateRef);

            m_OnUpdateMemory = new OnUpdateMemory
            {
                CurrentInputDeps = inputDeps,
                CommandBuffer = m_ToolOutputBarrier.CreateCommandBuffer()
            };

            // RMB / Secondary Apply: cycle zoning mode
            if (secondaryApplyAction.WasPressedThisFrame())
            {
                CycleZoningMode();
            }

            // LMB / Apply: drive the click/drag state machine
            applyMode = m_StateMachine.Transition(applyAction);

            m_ToolOutputBarrier.AddJobHandleForProducer(m_OnUpdateMemory.CurrentInputDeps);
            return m_OnUpdateMemory.CurrentInputDeps;
        }

        public override PrefabBase GetPrefab()
        {
            // Tool doesn't use a prefab directly; returning null is OK in practice.
            return null!;
        }

        public override bool TrySetPrefab(PrefabBase prefab)
        {
            // No prefab selection support for this tool.
            return false;
        }

        internal void EnableTool()
        {
            if (!toolEnabled)
            {
                toolEnabled = true;

                ToolBaseSystem current = m_ToolSystem.activeTool;
                m_PreviousToolSystem = current;

                // If the current tool is the road placement tool, do NOT return to it later
                // (it pops open the vanilla road UI and can steal LMB behavior).
                if (current is NetToolSystem)
                {
                    m_PreviousToolSystem = FindReturnTool();
                }

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
                    ToolBaseSystem? returnTool = m_PreviousToolSystem;

                    if (returnTool == null || returnTool == this || returnTool is NetToolSystem)
                    {
                        returnTool = FindReturnTool();
                    }

                    if (returnTool != null)
                    {
                        m_ToolSystem.activeTool = returnTool;
                    }
                }

                m_PreviousToolSystem = null;
            }
        }

        private ToolBaseSystem? FindReturnTool()
        {
            // Prefer any non-Net tool (avoids popping open the road UI when turning our helper off).
            List<ToolBaseSystem> tools = m_ToolSystem.tools;

            ToolBaseSystem? fallback = null;

            foreach (ToolBaseSystem tool in tools)
            {
                if (tool == null || tool == this)
                {
                    continue;
                }

                fallback ??= tool;

                if (tool is not NetToolSystem)
                {
                    return tool;
                }
            }

            // Fallback: any other tool at all.
            if (fallback != null)
            {
                return fallback;
            }

            // Last resort: if active tool isn't us, return it.
            ToolBaseSystem active = m_ToolSystem.activeTool;
            if (active != null && active != this)
            {
                return active;
            }

            return null;
        }


        private void CycleZoningMode()
        {
            ZoningMode current = m_UISystem.CurrentZoningMode;
            ZoningMode next = current switch
            {
                ZoningMode.Default => ZoningMode.Left,
                ZoningMode.Left => ZoningMode.Right,
                ZoningMode.Right => ZoningMode.None,
                ZoningMode.None => ZoningMode.Default,
                _ => ZoningMode.Default
            };

            m_UISystem.SetZoningModeFromTool(next);
        }

        // --- State-machine callbacks --------------------------------------------------------

        private JobHandle StopDragSelect(ZoneToolSystemExistingRoadsState previous, ZoneToolSystemExistingRoadsState next)
        {
            JobHandle job = new UpdateZoningInfo
            {
                CurveLookup = m_TypeHandle.__Game_Net_Curve_RW_ComponentLookup,
                ZoningInfoLookup = m_TypeHandle.__Game_Zoning_Info_RW_ComponentLookup,
                CommandBuffer = m_OnUpdateMemory.CommandBuffer,
                EntitySet = workingState.lastRaycastEntities,
                SubBlockBufferLookup = m_TypeHandle.__Game_SubBlock_RW_BufferLookup,
                EdgeLookup = m_TypeHandle.__Game_Edge_RW_ComponentLookup,
                NewZoningInfo = new ZoningInfo
                {
                    zoningMode = workingState.zoningMode
                }
            }.Schedule(m_OnUpdateMemory.CurrentInputDeps);

            m_OnUpdateMemory.CurrentInputDeps =
                JobHandle.CombineDependencies(m_OnUpdateMemory.CurrentInputDeps, job);
            return m_OnUpdateMemory.CurrentInputDeps;
        }

        private JobHandle HoverUpdate(ZoneToolSystemExistingRoadsState previous, ZoneToolSystemExistingRoadsState next)
        {
            EntityHighlighted(previous, next);
            return m_OnUpdateMemory.CurrentInputDeps;
        }

        private JobHandle StartDragSelect(ZoneToolSystemExistingRoadsState previous, ZoneToolSystemExistingRoadsState next)
        {
            SelectEntity(previous, next);
            return m_OnUpdateMemory.CurrentInputDeps;
        }

        private JobHandle KeepDragSelect(ZoneToolSystemExistingRoadsState previous, ZoneToolSystemExistingRoadsState next)
        {
            SelectEntity(previous, next);
            return m_OnUpdateMemory.CurrentInputDeps;
        }

        private JobHandle SelectEntity(ZoneToolSystemExistingRoadsState previous, ZoneToolSystemExistingRoadsState next)
        {
            if (GetRaycastResult(out Entity entity, out RaycastHit _))
            {
                if (!workingState.lastRaycastEntities.Contains(entity))
                {
                    workingState.lastRaycastEntities.Add(entity);

                    JobHandle job = new HighlightEntitiesJob
                    {
                        EntityToHighlight = entity,
                        CommandBuffer = m_OnUpdateMemory.CommandBuffer,
                        EdgeLookup = m_TypeHandle.__Game_Edge_RW_ComponentLookup
                    }.Schedule(m_OnUpdateMemory.CurrentInputDeps);

                    m_OnUpdateMemory.CurrentInputDeps =
                        JobHandle.CombineDependencies(job, m_OnUpdateMemory.CurrentInputDeps);
                }
            }

            return m_OnUpdateMemory.CurrentInputDeps;
        }

        private JobHandle EntityHighlighted(ZoneToolSystemExistingRoadsState previous, ZoneToolSystemExistingRoadsState next)
        {
            Entity previousEntity = workingState.lastRaycastEntity;

            if (GetRaycastResult(out Entity hitEntity, out RaycastHit _))
            {
                if (workingState.lastRaycastEntity != hitEntity)
                {
                    // Unhighlight previous.
                    if (previousEntity != Entity.Null)
                    {
                        JobHandle unhighlight = new UnHighlightEntitiesJob
                        {
                            CommandBuffer = m_OnUpdateMemory.CommandBuffer,
                            EntityToUnhighlight = previousEntity,
                            EdgeLookup = m_TypeHandle.__Game_Edge_RW_ComponentLookup
                        }.Schedule(m_OnUpdateMemory.CurrentInputDeps);

                        m_OnUpdateMemory.CurrentInputDeps =
                            JobHandle.CombineDependencies(unhighlight, m_OnUpdateMemory.CurrentInputDeps);
                    }

                    // Highlight new.
                    workingState.lastRaycastEntity = hitEntity;

                    JobHandle highlight = new HighlightEntitiesJob
                    {
                        EntityToHighlight = workingState.lastRaycastEntity,
                        CommandBuffer = m_OnUpdateMemory.CommandBuffer,
                        EdgeLookup = m_TypeHandle.__Game_Edge_RW_ComponentLookup
                    }.Schedule(m_OnUpdateMemory.CurrentInputDeps);

                    m_OnUpdateMemory.CurrentInputDeps =
                        JobHandle.CombineDependencies(highlight, m_OnUpdateMemory.CurrentInputDeps);
                }
            }
            else
            {
                // Nothing under cursor: clear highlight.
                workingState.lastRaycastEntity = Entity.Null;

                if (previousEntity != Entity.Null)
                {
                    JobHandle unhighlight = new UnHighlightEntitiesJob
                    {
                        CommandBuffer = m_OnUpdateMemory.CommandBuffer,
                        EntityToUnhighlight = previousEntity,
                        EdgeLookup = m_TypeHandle.__Game_Edge_RW_ComponentLookup
                    }.Schedule(m_OnUpdateMemory.CurrentInputDeps);

                    m_OnUpdateMemory.CurrentInputDeps =
                        JobHandle.CombineDependencies(unhighlight, m_OnUpdateMemory.CurrentInputDeps);
                }
            }

            return m_OnUpdateMemory.CurrentInputDeps;
        }

        // --- Component lookups --------------------------------------------------------------

        private struct TypeHandle
        {
            [ReadOnly]
            public ComponentLookup<Curve> __Game_Net_Curve_RW_ComponentLookup;
            public ComponentLookup<ZoningInfo> __Game_Zoning_Info_RW_ComponentLookup;
            public BufferLookup<SubBlock> __Game_SubBlock_RW_BufferLookup;
            public ComponentLookup<Block> __Game_Block_RW_ComponentLookup;
            public ComponentLookup<Edge> __Game_Edge_RW_ComponentLookup;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                __Game_Net_Curve_RW_ComponentLookup = state.GetComponentLookup<Curve>(isReadOnly: true);
                __Game_Zoning_Info_RW_ComponentLookup = state.GetComponentLookup<ZoningInfo>(isReadOnly: false);
                __Game_SubBlock_RW_BufferLookup = state.GetBufferLookup<SubBlock>(isReadOnly: false);
                __Game_Block_RW_ComponentLookup = state.GetComponentLookup<Block>(isReadOnly: true);
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

    // --- Simple state machine for click / drag behaviour -------------------------------------

    internal delegate JobHandle StateCallback(
        ZoneToolSystemExistingRoadsState previousState,
        ZoneToolSystemExistingRoadsState nextState);

    internal enum ZoneToolSystemExistingRoadsState
    {
        Default,
        Selecting,
        Selected
    }

    internal sealed class ZoneToolSystemExistingRoadsStateMachine
    {
        private ZoneToolSystemExistingRoadsState m_CurrentState;
        private readonly Dictionary<(ZoneToolSystemExistingRoadsState previous, ZoneToolSystemExistingRoadsState next), StateCallback> m_Transitions;

        internal ZoneToolSystemExistingRoadsStateMachine(
            Dictionary<(ZoneToolSystemExistingRoadsState previous, ZoneToolSystemExistingRoadsState next), StateCallback> transitions)
        {
            m_CurrentState = ZoneToolSystemExistingRoadsState.Default;
            m_Transitions = transitions;
        }

        internal ApplyMode Transition(IProxyAction applyAction)
        {
            ZoneToolSystemExistingRoadsState previousState = m_CurrentState;

            switch (m_CurrentState)
            {
                case ZoneToolSystemExistingRoadsState.Default:
                    if (applyAction.WasPressedThisFrame() && applyAction.WasReleasedThisFrame())
                    {
                        // Single quick click.
                        m_CurrentState = ZoneToolSystemExistingRoadsState.Selected;
                        TryRunCallback(previousState, m_CurrentState);
                        return ApplyMode.Apply;
                    }
                    else if (applyAction.WasPressedThisFrame() && !applyAction.WasReleasedThisFrame())
                    {
                        // Press start (drag select).
                        m_CurrentState = ZoneToolSystemExistingRoadsState.Selecting;
                        TryRunCallback(previousState, m_CurrentState);
                        return ApplyMode.None;
                    }
                    else if (applyAction.IsPressed())
                    {
                        // Holding mouse while dragging.
                        m_CurrentState = ZoneToolSystemExistingRoadsState.Selecting;
                        TryRunCallback(previousState, m_CurrentState);
                        return ApplyMode.None;
                    }
                    else if (applyAction.WasReleasedThisFrame())
                    {
                        // Mouse released -> apply.
                        m_CurrentState = ZoneToolSystemExistingRoadsState.Selected;
                        TryRunCallback(previousState, m_CurrentState);
                        return ApplyMode.Apply;
                    }

                    break;

                case ZoneToolSystemExistingRoadsState.Selecting:
                    if (applyAction.IsPressed())
                    {
                        // Continue drag select.
                        m_CurrentState = ZoneToolSystemExistingRoadsState.Selecting;
                        TryRunCallback(previousState, m_CurrentState);
                        return ApplyMode.None;
                    }
                    else if (!applyAction.IsPressed() && applyAction.WasReleasedThisFrame())
                    {
                        // Drag finished; selection is done, but zoning will be applied by Selected state.
                        m_CurrentState = ZoneToolSystemExistingRoadsState.Selected;
                        TryRunCallback(previousState, m_CurrentState);
                        return ApplyMode.None;
                    }

                    break;

                case ZoneToolSystemExistingRoadsState.Selected:
                    // After applying, go back to idle.
                    m_CurrentState = ZoneToolSystemExistingRoadsState.Default;
                    TryRunCallback(previousState, m_CurrentState);
                    return ApplyMode.Apply;
            }

            return ApplyMode.None;
        }

        internal void Reset()
        {
            m_CurrentState = ZoneToolSystemExistingRoadsState.Default;
        }

        private void TryRunCallback(ZoneToolSystemExistingRoadsState previous, ZoneToolSystemExistingRoadsState next)
        {
            if (m_Transitions.TryGetValue((previous, next), out StateCallback callback))
            {
                callback(previous, next);
            }
        }
    }
}
