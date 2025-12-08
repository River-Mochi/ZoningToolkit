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
        private ZoningToolkitModToolSystem m_Tool = null!;
        private PhotoModeRenderSystem m_PhotoMode = null!;

        private bool m_ActivateUI;
        private bool m_DeactivateUI;
        private UIState m_UIState;

        public override GameMode gameMode => GameMode.Game;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_ZoningSystem = World.GetOrCreateSystemManaged<ZoneToolSystemCore>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_PhotoMode = World.GetOrCreateSystemManaged<PhotoModeRenderSystem>();
            m_Tool = World.GetOrCreateSystemManaged<ZoningToolkitModToolSystem>();

            m_UIState = new UIState
            {
                visible = false,
                zoningMode = ZoningMode.Default,
                applyToNewRoads = false,
                toolEnabled = false
            };

            // React to tool / prefab changes
            m_ToolSystem.EventPrefabChanged =
                (Action<PrefabBase>)Delegate.Combine(m_ToolSystem.EventPrefabChanged, new Action<PrefabBase>(OnPrefabChanged));
            m_ToolSystem.EventToolChanged =
                (Action<ToolBaseSystem>)Delegate.Combine(m_ToolSystem.EventToolChanged, new Action<ToolBaseSystem>(OnToolChanged));

            // C# -> UI bindings
            AddUpdateBinding(new GetterValueBinding<string>(kGroup, "zoning_mode", () => m_UIState.zoningMode.ToString()));
            AddUpdateBinding(new GetterValueBinding<bool>(kGroup, "tool_enabled", () => m_UIState.toolEnabled));
            AddUpdateBinding(new GetterValueBinding<bool>(kGroup, "visible", () => m_UIState.visible));
            AddUpdateBinding(new GetterValueBinding<bool>(kGroup, "photomode", () => m_PhotoMode.Enabled));

            // UI -> C# bindings
            AddBinding(new TriggerBinding<string>(kGroup, "zoning_mode_update", zoningModeString =>
            {
                if (Enum.TryParse<ZoningMode>(zoningModeString, out ZoningMode mode))
                {
                    Mod.s_Log.Info($"Zone Tools UI: zoning mode updated to {mode}");
                    m_UIState.zoningMode = mode;
                }
            }));

            AddBinding(new TriggerBinding<bool>(kGroup, "tool_enabled", enabled =>
            {
                Mod.s_Log.Info($"Zone Tools UI: tool_enabled set to {enabled}");
                ToggleTool(enabled);
            }));
        }

        // Called from Mod.cs when Shift+Z is pressed.
        // This should ONLY toggle the panel, not the tool.
        internal void TogglePanelFromHotkey()
        {
            m_UIState.visible = !m_UIState.visible;
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
            // Only show UI when the active tool is a net tool with a zoning-capable road prefab
            if (tool is NetToolSystem netTool &&
                netTool.GetPrefab() is RoadPrefab roadPrefab &&
                roadPrefab.m_ZoneBlock != null)
            {
                m_ActivateUI = true;
            }
            else
            {
                m_DeactivateUI = true;
            }
        }

        private void OnPrefabChanged(PrefabBase prefab)
        {
            if (prefab is RoadPrefab roadPrefab && roadPrefab.m_ZoneBlock != null)
            {
                m_ActivateUI = true;
            }
            else
            {
                m_DeactivateUI = true;
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Apply pending show/hide state driven by tool / prefab changes
            if (m_ActivateUI)
            {
                m_ActivateUI = false;
                if (!m_UIState.visible)
                {
                    m_UIState.visible = true;
                }
            }

            if (m_DeactivateUI)
            {
                m_DeactivateUI = false;
                if (m_UIState.visible)
                {
                    m_UIState.visible = false;

                    // When hiding because prefab/tool changed, also disable the tool for safety.
                    if (m_Tool.toolEnabled)
                    {
                        m_Tool.DisableTool();
                    }
                }
            }

            // Sync UI -> tool/system
            if (m_UIState.zoningMode != m_Tool.workingState.zoningMode)
            {
                ZoningToolkitModToolSystem.WorkingState ws = m_Tool.workingState;
                ws.zoningMode = m_UIState.zoningMode;
                m_Tool.workingState = ws;
            }

            if (m_UIState.zoningMode != m_ZoningSystem.zoningMode)
            {
                m_ZoningSystem.zoningMode = m_UIState.zoningMode;
            }

            // Sync tool -> UI (tool enabled flag)
            if (m_UIState.toolEnabled != m_Tool.toolEnabled)
            {
                m_UIState.toolEnabled = m_Tool.toolEnabled;
            }
        }
    }
}
