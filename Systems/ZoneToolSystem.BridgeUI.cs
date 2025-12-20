// Systems/ZoneToolSystem.BridgeUI.cs
// Bridges Zone Tools ECS state with the in-game UI (panel, menu button, and tool enable/disable).
// Auto open/close the Zone Tools panel when selecting/leaving zonable road tools (optional via settings).

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

        private ZoneToolSystemCore? m_ZoningSystem;
        private ToolSystem? m_ToolSystem;
        private ZoneToolSystemExistingRoads? m_Tool;
        private PhotoModeRenderSystem? m_PhotoMode;

        private UIState m_UIState;

        // If true, the panel was opened automatically due to a zonable road tool selection,
        // then auto-close it when leaving the zonable road tool.
        private bool m_AutoOpenedForRoadTools;

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

            m_AutoOpenedForRoadTools = false;

            // React to tool / prefab changes.
            if (m_ToolSystem != null)
            {
                m_ToolSystem.EventPrefabChanged =
                    (Action<PrefabBase>)Delegate.Combine(
                        m_ToolSystem.EventPrefabChanged,
                        new Action<PrefabBase>(OnPrefabChanged));

                m_ToolSystem.EventToolChanged =
                    (Action<ToolBaseSystem>)Delegate.Combine(
                        m_ToolSystem.EventToolChanged,
                        new Action<ToolBaseSystem>(OnToolChanged));
            }

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
                () => m_PhotoMode?.Enabled ?? false));

            // UI -> C# bindings.
            AddBinding(new TriggerBinding<string>(
                kGroup,
                "zoning_mode_update",
                zoningModeString =>
                {
                    if (Enum.TryParse<ZoningMode>(zoningModeString, out ZoningMode mode))
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

                    // If tool refuses to enable (e.g., null-prefab safety), we must reflect reality back to UI.
                    ToggleTool(enabled);

                    if (m_Tool != null)
                    {
                        m_UIState.toolEnabled = m_Tool.toolEnabled;
                    }
                    else
                    {
                        m_UIState.toolEnabled = false;
                    }
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

            // Manual user action overrides the "auto-opened" latch.
            m_AutoOpenedForRoadTools = false;

            Mod.Debug($"{Mod.ModTag} TogglePanelFromHotkey: m_UIState.visible = {m_UIState.visible}");

            // If the panel is being hidden, also disable the update tool so input/overlays stop.
            if (!newVisible && m_Tool != null && m_Tool.toolEnabled)
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
            if (m_Tool == null)
            {
                return;
            }

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
            if (m_ToolSystem == null || m_Tool == null)
            {
                return;
            }

            // Always disable the existing-road helper when a zonable road building tool is selected.
            bool isZonableRoadTool = IsZonableRoadTool(tool);

            if (isZonableRoadTool)
            {
                if (m_Tool.toolEnabled)
                {
                    Mod.Debug($"{Mod.ModTag} OnToolChanged: Net road tool selected -> disabling Zone Tool helper.");
                    m_Tool.DisableTool();
                }
            }

            HandleAutoPanelForRoadTools(isZonableRoadTool, "tool change");
        }

        private void OnPrefabChanged(PrefabBase prefab)
        {
            if (m_ToolSystem == null)
            {
                return;
            }

            // Prefab changes matter mainly while NetTool is active (switching small road <-> highway, etc.).
            if (m_ToolSystem.activeTool is not NetToolSystem)
            {
                return;
            }

            bool isZonableRoadPrefab =
                prefab is RoadPrefab roadPrefab &&
                roadPrefab.m_ZoneBlock != null;

            HandleAutoPanelForRoadTools(isZonableRoadPrefab, "prefab change");
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (m_ToolSystem == null || m_Tool == null || m_PhotoMode == null || m_ZoningSystem == null)
            {
                return;
            }

            // If the panel is not visible (or photo mode is on), the update tool must not remain active.
            if ((!m_UIState.visible || m_PhotoMode.Enabled) && m_Tool.toolEnabled)
            {
                Mod.Debug($"{Mod.ModTag} OnUpdate: panel hidden/photomode -> disabling update tool.");
                ToggleTool(false);
                m_UIState.toolEnabled = false;
            }

            // Safety: if we auto-opened due to a road tool, but we're no longer in a zonable road tool,
            // auto-close even if an event was missed.
            bool autoOpen = Mod.Settings?.AutoOpenPanelForRoadTools ?? true;
            if (autoOpen && m_AutoOpenedForRoadTools)
            {
                bool stillZonable = IsZonableRoadTool(m_ToolSystem.activeTool);
                if (!stillZonable)
                {
                    HandleAutoPanelForRoadTools(false, "update check");
                }
            }

            // Sync UI -> tool/system.
            // Existing-roads tool reads mode from UI system / core; no per-tool state sync needed.

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

        // ----- Helpers --------------------------------------------------------

        private static bool IsZonableRoadTool(ToolBaseSystem tool)
        {
            if (tool == null)
            {
                return false;
            }

            if (tool is not NetToolSystem netTool)
            {
                return false;
            }

            if (netTool.GetPrefab() is not RoadPrefab roadPrefab)
            {
                return false;
            }

            return roadPrefab.m_ZoneBlock != null;
        }

        private void HandleAutoPanelForRoadTools(bool isZonableRoadTool, string reason)
        {
            bool autoOpen = Mod.Settings?.AutoOpenPanelForRoadTools ?? true;
            if (!autoOpen)
            {
                // If user disabled the option, don't keep an auto-open latch around.
                m_AutoOpenedForRoadTools = false;
                return;
            }

            if (isZonableRoadTool)
            {
                // Auto-open only if it was previously closed.
                if (!m_UIState.visible)
                {
                    Mod.Debug($"{Mod.ModTag} Auto-open ZT panel ({reason}).");
                    m_UIState.visible = true;
                    m_AutoOpenedForRoadTools = true;
                }
            }
            else
            {
                // Auto-close only if we were the ones who auto-opened it.
                if (m_AutoOpenedForRoadTools && m_UIState.visible)
                {
                    Mod.Debug($"{Mod.ModTag} Auto-close ZT panel ({reason}).");
                    m_UIState.visible = false;
                    m_AutoOpenedForRoadTools = false;

                    if (m_Tool != null && m_Tool.toolEnabled)
                    {
                        ToggleTool(false);
                        m_UIState.toolEnabled = false;
                    }
                }
            }
        }
    }
}
