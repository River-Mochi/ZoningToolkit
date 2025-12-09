// src/mods/zoning-toolkit-panel.tsx
// Main Zone Tools panel: shows current mode and lets you switch modes.

import React from "react";
import { withStore, ModUIState } from "./state";
import {
    ALL_ZONING_MODES,
    ZoningMode,
    zoneModeIconMap,
    getModeFromString,
} from "./zoning-toolkit-utils";

import styles from "./zoning-toolkit-panel.module.scss";

const ZoningToolkitPanelInternal: React.FC<ModUIState> = (props: ModUIState) => {
    const {
        photomodeActive,
        isToolEnabled,
        zoningMode,
        updateZoningMode,
    } = props;

    // Hide panel when tool is off or photomode is active.
    if (!isToolEnabled || photomodeActive) {
        return null;
    }

    const activeMode: ZoningMode = getModeFromString(zoningMode);

    return (
        <div className={styles.panel}>
            <div className={styles.panelToolModeRow}>
                {ALL_ZONING_MODES.map((mode) => {
                    const isActive = mode === activeMode;
                    const iconSrc = zoneModeIconMap[mode];

                    return (
                        <button
                            key={mode}
                            type="button"
                            style={{
                                border: isActive ? "2px solid #fff" : "1px solid #666",
                                marginRight: "8rem",
                                padding: "4rem 8rem",
                                background: isActive ? "#3059b5" : "#222",
                                cursor: "pointer",
                            }}
                            onClick={() => updateZoningMode(mode)}
                        >
                            {iconSrc && (
                                <img
                                    src={iconSrc}
                                    alt={mode}
                                    style={{
                                        width: "24rem",
                                        height: "24rem",
                                        marginRight: "4rem",
                                        verticalAlign: "middle",
                                    }}
                                />
                            )}
                            <span>{mode}</span>
                        </button>
                    );
                })}
            </div>
        </div>
    );
};

// Export no-props component bound to the store.
export const ZoningToolkitPanel = withStore(ZoningToolkitPanelInternal);
