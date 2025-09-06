using System;
using Colossal.UI.Binding;
using Game;
using Game.Prefabs;
using Game.Rendering;
using Game.Tools;
using Game.UI;
using Unity.Collections;
using Unity.Entities;
using ZoningToolkit.Components;

namespace ZoningToolkit.Systems
{
    internal struct UIState
    {
        public bool visible;
        public ZoningMode zoningMode;
        public bool applyToNewRoads;
        public bool toolEnabled;
    }
    partial class ZoningToolkitModUISystem : UISystemBase
    {
        private string kGroup = "zoning_adjuster_ui_namespace";
        private ZoningToolkitModSystem zoningToolkitModSystem;
        private bool activateModUI = false;
        private bool deactivateModUI = false;
        private UIState uiState;
        private ToolSystem toolSystem;
        private NativeQueue<Entity> entitiesToUpdate;
        private ZoningToolkitModToolSystem zoningToolkitModToolSystem;
        private PhotoModeRenderSystem photoModeRenderSystem;
        private GetterValueBinding<bool> toolEnabledBinding;

        public override GameMode gameMode => GameMode.Game;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.zoningToolkitModSystem = World.GetExistingSystemManaged<ZoningToolkitModSystem>();
            this.toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            this.photoModeRenderSystem = World.GetOrCreateSystemManaged<PhotoModeRenderSystem>();
            this.zoningToolkitModToolSystem = World.GetOrCreateSystemManaged<ZoningToolkitModToolSystem>();
            this.uiState = new UIState()
            {
                visible = false,
                zoningMode = ZoningMode.Default,
                applyToNewRoads = false,
                toolEnabled = false
            };

            this.toolSystem.EventPrefabChanged = (Action<PrefabBase>)Delegate.Combine(toolSystem.EventPrefabChanged, new Action<PrefabBase>(OnPrefabChanged));
            this.toolSystem.EventToolChanged = (Action<ToolBaseSystem>)Delegate.Combine(toolSystem.EventToolChanged, new Action<ToolBaseSystem>(OnToolChange));

            this.AddUpdateBinding(new GetterValueBinding<string>(this.kGroup, "zoning_mode", () => this.uiState.zoningMode.ToString()));
            toolEnabledBinding = new GetterValueBinding<bool>(kGroup, "tool_enabled", () => this.uiState.toolEnabled);
            this.AddUpdateBinding(toolEnabledBinding);
            this.AddUpdateBinding(new GetterValueBinding<bool>(this.kGroup, "visible", () => this.uiState.visible));
            this.AddUpdateBinding(new GetterValueBinding<bool>(this.kGroup, "photomode", () => this.photoModeRenderSystem.Enabled));

            this.AddBinding(new TriggerBinding<string>(this.kGroup, "zoning_mode_update", zoningMode =>
            {
                this.getLogger().Info($"Zoning mode updated to ${zoningMode}.");
                this.uiState.zoningMode = (ZoningMode)Enum.Parse(typeof(ZoningMode), zoningMode);
            })
            );
            this.AddBinding(new TriggerBinding<bool>(this.kGroup, "tool_enabled", tool_enabled =>
            {
                this.getLogger().Info($"Tool Enabled updated to ${tool_enabled}.");
                toggleTool(tool_enabled);
            })
            );


        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.entitiesToUpdate.Dispose();
        }

        private void toggleTool(bool enableTool)
        {
            if (enableTool)
            {
                this.zoningToolkitModToolSystem.EnableTool();
            }
            else
            {
                this.zoningToolkitModToolSystem.DisableTool();
            }
        }

        private void OnToolChange(ToolBaseSystem tool)
        {
            this.getLogger().Info("Tool changed!");

            if (tool is NetToolSystem)
            {
                if (tool.GetPrefab() is RoadPrefab)
                {
                    this.getLogger().Info("Prefab is RoadPrefab!");
                    RoadPrefab roadPrefab = (RoadPrefab)tool.GetPrefab();
                    this.getLogger().Info($"Road prefab information.");
                    this.getLogger().Info($"Road Type {roadPrefab.m_RoadType}.");
                    this.getLogger().Info($"Road Zone Block {roadPrefab.m_ZoneBlock}.");

                    if (roadPrefab.m_ZoneBlock != null)
                    {
                        activateModUI = true;
                        return;
                    }
                }
            }

            deactivateModUI = true;
        }

        private void OnPrefabChanged(PrefabBase prefabBase)
        {
            this.getLogger().Info("Prefab changed!");

            if (prefabBase is RoadPrefab)
            {
                this.getLogger().Info("Prefab is RoadPrefab!");
                RoadPrefab roadPrefab = (RoadPrefab)prefabBase;
                this.getLogger().Info($"Road prefab information.");
                this.getLogger().Info($"Road Type {roadPrefab.m_RoadType}.");
                this.getLogger().Info($"Road Zone Block {roadPrefab.m_ZoneBlock}.");

                if (roadPrefab.m_ZoneBlock != null)
                {
                    activateModUI = true;
                    return;
                }
            }
            else
            {
                this.getLogger().Info("Prefab is not RoadPrefab!");
                deactivateModUI = true;
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // BEGIN: apply pending UI state changes
            if (activateModUI)
            {
                activateModUI = false;
                if (!uiState.visible)
                {
                    uiState.visible = true;
                    // If UISystemBase has a way to force a refresh, can call it here.
                    // this.MarkDirty(this);
                }
            }

            if (deactivateModUI)
            {
                deactivateModUI = false;
                if (uiState.visible)
                {
                    uiState.visible = false;
                    // Safety: if we hide the panel because prefab is not a zoning road, also disable the tool.
                    if (zoningToolkitModToolSystem.toolEnabled)
                        zoningToolkitModToolSystem.DisableTool();
                    // Same note as above: a forced UI refresh is optional.
                }
            }
            // --- END: apply pending UI state changes ---

            // Update Tool and System info from UI (keep original logic below)
            if (this.uiState.zoningMode != this.zoningToolkitModToolSystem.workingState.zoningMode)
            {
                this.getLogger().Info("Updating Tool System Zoning mode");
                this.zoningToolkitModToolSystem.workingState.zoningMode = this.uiState.zoningMode;
            }

            if (this.uiState.zoningMode != this.zoningToolkitModSystem.zoningMode)
            {
                this.getLogger().Info("Updating Mod System Zoning mode");
                this.zoningToolkitModSystem.zoningMode = this.uiState.zoningMode;
            }

            if (this.uiState.toolEnabled != this.zoningToolkitModToolSystem.toolEnabled)
            {
                this.getLogger().Info("Syncing Enabling/Disabling tool status");
                this.uiState.toolEnabled = this.zoningToolkitModToolSystem.toolEnabled;
                // If keep toolEnabledBinding, then push it here:
                // toolEnabledBinding.TriggerUpdate();
            }
        }
    }
}
