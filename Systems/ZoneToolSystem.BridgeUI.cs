// Systems/ZoneToolSystem.BridgeUI.cs
// Bridges Zone Tools ECS state with the in-game UI (panel, menu button, and tool enable/disable).

namespace ZoningToolkit.Systems
{
    using System;
    using Colossal.UI.Binding;
    using Game;
    using Game.Prefabs;
    using Game.Rendering;
    using Game.Tools;
    using Game.UI;
    using Unity.Entities;
    using ZoningToolkit.Components;

    internal struct UIState
    {
        public bool visible;
        public ZoningMode zoningMode;
        public bool applyToNewRoads;
        public bool toolEnabled;
    }

    internal sealed partial class ZoneToolBridgeUI : UISystemBase
    {
        private const string kGroup = "zoning_adjuster_ui_namespace";

        private ZoneToolSystemCore m_ZoningSystem = null!;
        private ToolSystem m_ToolSystem = null!;
        private ZoneToolSystemExistingRoads m_Tool = null!;
        private PhotoModeRenderSystem m_PhotoMode = null!;

        private UIState m_UIState;

        public override GameMode gameMode => GameMode.Game;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_ZoningSystem = World.GetOrCreateSystemManaged<ZoneToolSystemCore>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_PhotoMode = World.GetOrCreateSystemManaged<PhotoModeRenderSystem>();
            m_Tool = World.GetOrCreateSystemManaged<ZoneToolSystemExistingRoads>();

            m_UIState = new UIState
            {
                visible = false,
                zoningMode = ZoningMode.Default,
                applyToNewRoads = false,
                toolEnabled = false
            };

            // React to tool / prefab changes.
            m_ToolSystem.EventPrefabChanged =
                (Action<PrefabBase>)Delegate.Combine(
                    m_ToolSystem.EventPrefabChanged,
                    new Action<PrefabBase>(OnPrefabChanged));
            m_ToolSystem.EventToolChanged =
                (Action<ToolBaseSystem>)Delegate.Combine(
                    m_ToolSystem.EventToolChanged,
                    new Action<ToolBaseSystem>(OnToolChanged));

            // C# -> UI bindings.
            AddUpdateBinding(new GetterValueBinding<string>(
                kGroup,
                "zoning_mode",
                () => m_UIState.zoningMode.ToString()));

            AddUpdateBinding(new GetterValueBinding<bool>(
                kGroup,
                "tool_enabled",
                () => m_UIState.toolEnabled));

            AddUpdateBinding(new GetterValueBinding<bool>(
                kGroup,
                "visible",
                () => m_UIState.visible));

            AddUpdateBinding(new GetterValueBinding<bool>(
                kGroup,
                "photomode",
                () => m_PhotoMode.Enabled));

            // UI -> C# bindings.
            AddBinding(new TriggerBinding<string>(
                kGroup,
                "zoning_mode_update",
                zoningModeString =>
                {
                    if (Enum.TryParse<ZoningMode>(zoningModeString, out var mode))
                    {
                        Mod.Debug($"{Mod.ModTag} Zone Tools UI: zoning mode updated to {mode}");
                        m_UIState.zoningMode = mode;
                    }
                }));

            AddBinding(new TriggerBinding<bool>(
                kGroup,
                "tool_enabled",
                enabled =>
                {
                    Mod.Debug($"{Mod.ModTag} Zone Tools UI: tool_enabled set to {enabled}");
                    ToggleTool(enabled);
                }));

            // FAB button toggle: same behaviour as Shift+Z keybind.
            AddBinding(new TriggerBinding<bool>(
                kGroup,
                "toggle_panel",
                _ =>
                {
                    Mod.Debug($"{Mod.ModTag} Zone Tools UI: toggle_panel trigger.");
                    TogglePanelFromHotkey();
                }));
        }

        // Called from ZoneToolSystemKeybind (Shift+Z) and from UI via toggle_panel trigger.
        // This only toggles the Zone Tools panel visibility; it does not change the active tool.
        internal void TogglePanelFromHotkey()
        {
            bool newVisible = !m_UIState.visible;
            m_UIState.visible = newVisible;
            Mod.Debug($"{Mod.ModTag} TogglePanelFromHotkey: m_UIState.visible = {m_UIState.visible}");

            // If the panel is being hidden, also disable the update tool so input/overlays stop.
            if (!newVisible && m_Tool.toolEnabled)
            {
                Mod.Debug($"{Mod.ModTag} TogglePanelFromHotkey: panel hidden -> disabling update tool.");
                ToggleTool(false);
                m_UIState.toolEnabled = false;
            }
        }


        // Used by the zoning tool to read / change the active mode.
        internal ZoningMode CurrentZoningMode => m_UIState.zoningMode;

        internal void SetZoningModeFromTool(ZoningMode mode)
        {
            if (m_UIState.zoningMode != mode)
            {
                m_UIState.zoningMode = mode;
            }
        }

        private void ToggleTool(bool enable)
        {
            if (enable)
            {
                m_Tool.EnableTool();
            }
            else
            {
                m_Tool.DisableTool();
            }
        }

        private void OnToolChanged(ToolBaseSystem tool)
        {
            // When a zoneable road-building tool is selected, disable the existing-road helper
            // to avoid mixing "build new roads" and "update existing roads".
            if (tool is NetToolSystem netTool &&
                netTool.GetPrefab() is RoadPrefab roadPrefab &&
                roadPrefab.m_ZoneBlock != null)
            {
                if (m_Tool.toolEnabled)
                {
                    Mod.Debug($"{Mod.ModTag} OnToolChanged: Net road tool selected -> disabling Zone Tool helper.");
                    m_Tool.DisableTool();
                }

                // Optional convenience: auto-open the panel when entering a zonable road tool.
                if (!m_UIState.visible)
                {
                    Mod.Debug($"{Mod.ModTag} OnToolChanged: auto-opening Zone Tools panel for road tool.");
                    m_UIState.visible = true;
                }
            }
        }

        private void OnPrefabChanged(PrefabBase prefab)
        {
            // Panel visibility is not driven by prefab changes in this phase.
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Systems/ZoneToolSystem.BridgeUI.cs

            // If the panel is not visible (or photo mode is on), the update tool must not remain active.
            if ((!m_UIState.visible || m_PhotoMode.Enabled) && m_Tool.toolEnabled)
            {
                Mod.Debug($"{Mod.ModTag} OnUpdate: panel hidden/photomode -> disabling update tool.");
                ToggleTool(false);
                m_UIState.toolEnabled = false;
            }

            // Sync UI -> tool/system.
            if (m_UIState.zoningMode != m_Tool.workingState.zoningMode)
            {
                ZoneToolSystemExistingRoads.WorkingState ws = m_Tool.workingState;
                ws.zoningMode = m_UIState.zoningMode;
                m_Tool.workingState = ws;
            }

            if (m_UIState.zoningMode != m_ZoningSystem.zoningMode)
            {
                m_ZoningSystem.zoningMode = m_UIState.zoningMode;
            }

            // Sync tool -> UI (tool enabled flag).
            if (m_UIState.toolEnabled != m_Tool.toolEnabled)
            {
                m_UIState.toolEnabled = m_Tool.toolEnabled;
            }
        }
    }
}
