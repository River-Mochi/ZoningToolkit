// src/mods/zoning-toolkit-panel.tsx
// Floating panel for Zone Tools (zoning mode picker + update tool toggle).

import React, { CSSProperties } from "react";
import Draggable from "react-draggable";
import { Panel, PanelSection, PanelSectionRow } from "cs2/ui";

import updateToolIcon from "../../assets/icons/replace_tool_icon.svg";
import { useModUIStore, withStore } from "./state";
import panelStyles from "./zoning-toolkit-panel.module.scss";
import VanillaBindings from "./vanilla-bindings";
import {
    getModeFromString,
    zoneModeIconMap,
    ZoningMode,
} from "./zoning-toolkit-utils";

const { ToolButton } = VanillaBindings.components;

interface ZoningModeButtonConfig {
    icon: string;
    mode: ZoningMode;
    tooltip?: string;
}

const zoningModeButtonConfigs: ZoningModeButtonConfig[] = [
    {
        icon: zoneModeIconMap[ZoningMode.DEFAULT],
        mode: ZoningMode.DEFAULT,
        tooltip: "Default (both)",
    },
    {
        icon: zoneModeIconMap[ZoningMode.LEFT],
        mode: ZoningMode.LEFT,
        tooltip: "Left",
    },
    {
        icon: zoneModeIconMap[ZoningMode.RIGHT],
        mode: ZoningMode.RIGHT,
        tooltip: "Right",
    },
    {
        icon: zoneModeIconMap[ZoningMode.NONE],
        mode: ZoningMode.NONE,
        tooltip: "None",
    },
];

export class ZoningToolkitPanelInternal extends React.Component {
    handleZoneModeSelect(zoningMode: ZoningMode) {
        useModUIStore.getState().updateZoningMode(zoningMode.toString());
    }

    handleZoneToolSelect(enabled: boolean) {
        useModUIStore.getState().updateIsToolEnabled(enabled);
    }

    render() {
        const store = useModUIStore.getState();
        const currentZoningMode = getModeFromString(store.zoningMode);
        const isToolEnabled = store.isToolEnabled;
        const uiVisible = store.uiVisible;
        const photomodeActive = store.photomodeActive;

        const panelStyle: CSSProperties = {
            // Panel is hidden in photo mode or when not visible.
            display: !uiVisible || photomodeActive ? "none" : undefined,
        };

        return (
            <Draggable

                bounds="parent" grid={[5, 5]}
            >
                <Panel
                    className={panelStyles.panel}
                    header="Zone Tools"
                    style={panelStyle}
                >
                    <PanelSection>
                        <PanelSectionRow
                            left="Pick One"
                            right={
                                <div className={panelStyles.panelToolModeRow}>
                                    {zoningModeButtonConfigs.map((config) => (
                                        <ToolButton
                                            key={config.mode}
                                            focusKey={
                                                VanillaBindings.common.focus
                                                    .disabled
                                            }
                                            selected={
                                                currentZoningMode ===
                                                config.mode
                                            }
                                            src={config.icon}
                                            tooltip={config.tooltip}
                                            onSelect={() =>
                                                this.handleZoneModeSelect(
                                                    config.mode,
                                                )
                                            }
                                        />
                                    ))}
                                </div>
                            }
                        />
                        <PanelSectionRow
                            left="Update Tool"
                            right={
                                <ToolButton
                                    focusKey={
                                        VanillaBindings.common.focus.disabled
                                    }
                                    selected={isToolEnabled}
                                    src={updateToolIcon}
                                    tooltip="Toggle update tool (for existing roads). Roads with zoned buildings are skipped."
                                    onSelect={() =>
                                        this.handleZoneToolSelect(!isToolEnabled)
                                    }
                                />
                            }
                        />
                    </PanelSection>
                </Panel>
            </Draggable>
        );
    }
}

export const ZoningToolkitPanel = withStore(ZoningToolkitPanelInternal);
