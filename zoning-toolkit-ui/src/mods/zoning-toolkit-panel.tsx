// src/mods/zoning-toolkit-panel.tsx
// Floating panel for Zone Tools (zoning mode picker + update tool toggle).

import React, { CSSProperties } from "react";
import Draggable from "react-draggable";
import { Panel, PanelSection, PanelSectionRow } from "cs2/ui";
import updateToolIcon from "../../assets/icons/replace_tool_icon.svg";
import { useModUIStore, withStore } from "./state";
import panelStyles from "./zoning-toolkit-panel.module.scss";
import VanillaBindings from "./vanilla-bindings";
import { getModeFromString, zoneModeIconMap, ZoningMode } from "./zoning-toolkit-utils";

const { ToolButton } = VanillaBindings.components;

interface ZoningModeButtonConfig {
    icon: string;
    mode: ZoningMode;
    tooltip?: string;
}

const zoningModeButtonConfigs: ZoningModeButtonConfig[] = [
    { icon: zoneModeIconMap[ZoningMode.DEFAULT], mode: ZoningMode.DEFAULT, tooltip: "Default (both)" },
    { icon: zoneModeIconMap[ZoningMode.LEFT], mode: ZoningMode.LEFT, tooltip: "Left" },
    { icon: zoneModeIconMap[ZoningMode.RIGHT], mode: ZoningMode.RIGHT, tooltip: "Right" },
    { icon: zoneModeIconMap[ZoningMode.NONE], mode: ZoningMode.NONE, tooltip: "None" },
];

export class ZoningToolkitPanelInternal extends React.Component {
    state = {
        bounds: undefined,
    };

    private nodeRef = React.createRef<HTMLDivElement>();

    private recomputeBounds = () => {
        const node = this.nodeRef.current;
        const parent = node?.parentElement;
        if (!node || !parent) return;

        const nodeRect = node.getBoundingClientRect();
        const parentRect = parent.getBoundingClientRect();

        const bounds = {
            left: parentRect.left - nodeRect.left,
            top: parentRect.top - nodeRect.top,
            right: parentRect.right - nodeRect.right,
            bottom: parentRect.bottom - nodeRect.bottom,
        };

        this.setState({ bounds });
    };

    private onDragStart = () => {
        this.recomputeBounds(); // Recompute bounds before drag starts
    };

    render() {
        const store = useModUIStore.getState();
        const currentZoningMode = getModeFromString(store.zoningMode);
        const isToolEnabled = store.isToolEnabled;
        const uiVisible = store.uiVisible;
        const photomodeActive = store.photomodeActive;

        const panelStyle: CSSProperties = {
            display: !uiVisible || photomodeActive ? "none" : undefined, // Hide the panel in photo mode or when it's not visible
        };

        return (
            <Draggable
                nodeRef={this.nodeRef}
                bounds={this.state.bounds ?? "parent"} // Ensure it stays within bounds
                grid={[5, 5]} // Add a grid for smoother dragging
                onStart={this.onDragStart}
            >
                <div ref={this.nodeRef} className={panelStyles.dragItem}>
                    <Panel className={panelStyles.panel} header="Zone Tools" style={panelStyle}>
                        <PanelSection>
                            <PanelSectionRow
                                left="Pick One"
                                right={
                                    <div className={panelStyles.panelToolModeRow}>
                                        {zoningModeButtonConfigs.map((config) => (
                                            <ToolButton
                                                key={config.mode}
                                                focusKey={VanillaBindings.common.focus.disabled}
                                                selected={currentZoningMode === config.mode}
                                                src={config.icon}
                                                tooltip={config.tooltip}
                                                onSelect={() => useModUIStore.getState().updateZoningMode(config.mode)}
                                            />
                                        ))}
                                    </div>
                                }
                            />
                            <PanelSectionRow
                                left="Update Tool"
                                right={
                                    <ToolButton
                                        focusKey={VanillaBindings.common.focus.disabled}
                                        selected={isToolEnabled}
                                        src={updateToolIcon}
                                        tooltip="Toggle update tool (for existing roads). Roads with zoned buildings are skipped."
                                        onSelect={() => useModUIStore.getState().updateIsToolEnabled(!isToolEnabled)}
                                    />
                                }
                            />
                        </PanelSection>
                    </Panel>
                </div>
            </Draggable>
        );
    }
}

export const ZoningToolkitPanel = withStore(ZoningToolkitPanelInternal);
