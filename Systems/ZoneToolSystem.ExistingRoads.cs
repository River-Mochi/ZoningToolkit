// File: Systems/ZoneToolSystem.ExistingRoads.cs
// Purpose: Update Existing Roads tool (hover + select + apply zoning mode to existing networks).

namespace ZoningToolkit.Systems
{
    using Colossal.Entities;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Net;
    using Game.Prefabs;
    using Game.Tools;
    using Game.Zones;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using ZoningToolkit.Components;
    using ZoningToolkit.Utils;

    internal sealed partial class ZoneToolSystemExistingRoads : ToolBaseSystem
    {
        private ToolSystem m_ZTToolSystem = null!;
        private DefaultToolSystem m_ZTDefaultToolSystem = null!;
        private NetToolSystem m_NetToolSystem = null!;
        private ToolOutputBarrier m_ToolOutputBarrier = null!;
        private PrefabSystem m_ZTPrefabSystem = null!;
        private ZoneToolBridgeUI m_UISystem = null!;

        private NativeHashSet<Entity> m_Selected;
        private int m_SelectedCount;
        private Entity m_Hovered;

        internal bool toolEnabled
        {
            get; private set;
        }

        private ToolBaseSystem? m_PreviousTool;

        public override string toolID => "Zone Tools Zoning Tool";

        protected override void OnCreate()
        {
            base.OnCreate();

            Enabled = false;

            m_ZTToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ZTDefaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_ToolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_ZTPrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_UISystem = World.GetOrCreateSystemManaged<ZoneToolBridgeUI>();

            m_Selected = new NativeHashSet<Entity>(128, Allocator.Persistent);
            m_SelectedCount = 0;
            m_Hovered = Entity.Null;

            toolEnabled = false;

            EnsureSafePrefabForUI();
        }

        protected override void OnDestroy()
        {
            if (m_Selected.IsCreated)
            {
                m_Selected.Dispose();
            }

            base.OnDestroy();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            toolEnabled = true;

            applyAction.shouldBeEnabled = true;
            secondaryApplyAction.shouldBeEnabled = true;

            requireNet = Layer.Road;
            requireZones = true;
            allowUnderground = true;

            EnsureSafePrefabForUI();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();

            toolEnabled = false;

            applyAction.shouldBeEnabled = false;
            secondaryApplyAction.shouldBeEnabled = false;

            ClearSelection();
            m_Hovered = Entity.Null;
        }

        public override void InitializeRaycast()
        {
            base.InitializeRaycast();

            m_ToolRaycastSystem.typeMask = TypeMask.Net | TypeMask.Lanes;
            m_ToolRaycastSystem.netLayerMask = Layer.Road;
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

#if DEBUG
            DebugDumpPrefabIds("Crosswalk");
            DebugDumpPrefabIds("Wide");
            DebugDumpPrefabIds("Sidewalk");
            DebugDumpPrefabIds("Fence");
#endif
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EnsureSafePrefabForUI();

            if (!toolEnabled)
            {
                return inputDeps;
            }

            applyMode = ApplyMode.Clear;

            if (secondaryApplyAction.WasPressedThisFrame())
            {
                CycleZoningMode();
            }

            UpdateHover();

            if (applyAction.WasPressedThisFrame() || applyAction.IsPressed())
            {
                AddHoveredToSelection();
            }

            if (applyAction.WasReleasedThisFrame())
            {
                ApplySelection();
            }

            return inputDeps;
        }

        public override PrefabBase GetPrefab()
        {
            return GetSafePrefabForUI();
        }

        public override bool TrySetPrefab(PrefabBase prefab)
        {
            return false;
        }

        internal void EnableTool()
        {
            EnsureSafePrefabForUI();

            m_PreviousTool = m_ZTToolSystem.activeTool;
            m_ZTToolSystem.activeTool = this;

            toolEnabled = true;

            Mod.s_Log.Info($"{Mod.ModTag} ExistingRoads enabled");
        }

        internal void DisableTool()
        {
            toolEnabled = false;

            ClearSelection();
            m_Hovered = Entity.Null;

            ToolBaseSystem? returnTool = m_PreviousTool;
            if (returnTool == null || returnTool == this)
            {
                returnTool = m_NetToolSystem;
            }

            m_ZTToolSystem.activeTool = returnTool;

            m_PreviousTool = null;

            Mod.s_Log.Info($"{Mod.ModTag} ExistingRoads disabled");
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

        private void UpdateHover()
        {
            Entity newHovered = TryGetRaycastRoad(out Entity e) ? e : Entity.Null;
            if (newHovered == m_Hovered)
            {
                return;
            }

            m_Hovered = newHovered;
        }

        private void AddHoveredToSelection()
        {
            if (m_Hovered == Entity.Null)
            {
                return;
            }

            if (!EntityManager.Exists(m_Hovered))
            {
                return;
            }

            if (!m_Selected.Contains(m_Hovered))
            {
                m_Selected.Add(m_Hovered);
                m_SelectedCount++;
            }
        }

        private void ClearSelection()
        {
            m_Selected.Clear();
            m_SelectedCount = 0;
        }

        private void ApplySelection()
        {
            if (m_SelectedCount == 0)
            {
                return;
            }

            ZoningMode mode = m_UISystem.CurrentZoningMode;

            EntityCommandBuffer ecb = m_ToolOutputBarrier.CreateCommandBuffer();

            foreach (Entity roadEntity in m_Selected)
            {
                AddOrSetZoningInfo(ecb, roadEntity, mode);
                TagSubBlocksForUpdate(ecb, roadEntity);
            }

            ClearSelection();
        }

        private bool TryGetRaycastRoad(out Entity entity)
        {
            entity = Entity.Null;

            if (!base.GetRaycastResult(out Entity hit, out RaycastHit _))
            {
                return false;
            }

            if (!EntityManager.HasComponent<Edge>(hit))
            {
                return false;
            }

            if (!EntityManager.HasBuffer<SubBlock>(hit))
            {
                return false;
            }

#if DEBUG
            // Dump components on the hit road entity so to see PrefabRef/Owner/etc.
            this.listEntityComponents(hit);

            // If it has PrefabRef, dump the prefab entity too.
            if (EntityManager.TryGetComponent<PrefabRef>(hit, out var pr))
            {
                Mod.s_Log.Debug($"{Mod.ModTag} Hit PrefabRef entity: {pr.m_Prefab}");
                this.listEntityComponents(pr.m_Prefab);
            }
#endif

            entity = hit;
            return true;
        }

        private void AddOrSetZoningInfo(EntityCommandBuffer ecb, Entity owner, ZoningMode mode)
        {
            ZoningInfo zi = new ZoningInfo { zoningMode = mode };

            if (EntityManager.HasComponent<ZoningInfo>(owner))
            {
                ecb.SetComponent(owner, zi);
            }
            else
            {
                ecb.AddComponent(owner, zi);
            }
        }

        private void TagSubBlocksForUpdate(EntityCommandBuffer ecb, Entity roadEntity)
        {
            if (!EntityManager.HasBuffer<SubBlock>(roadEntity))
            {
                return;
            }

            DynamicBuffer<SubBlock> subBlocks = EntityManager.GetBuffer<SubBlock>(roadEntity, isReadOnly: true);

            for (int i = 0; i < subBlocks.Length; i++)
            {
                Entity blockEntity = subBlocks[i].m_SubBlock;
                if (blockEntity == Entity.Null)
                {
                    continue;
                }

                if (!EntityManager.Exists(blockEntity))
                {
                    continue;
                }

                if (!EntityManager.HasComponent<ZoningInfoUpdated>(blockEntity))
                {
                    ecb.AddComponent<ZoningInfoUpdated>(blockEntity);
                }

                if (!EntityManager.HasComponent<Updated>(blockEntity))
                {
                    ecb.AddComponent<Updated>(blockEntity);
                }
            }
        }
    }
}
