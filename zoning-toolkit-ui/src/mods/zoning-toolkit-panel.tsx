// src/mods/zoning-toolkit-panel.tsx
// Floating panel for Zone Tools (zoning mode picker + icons).

import React from "react";
import { ModUIState, withStore } from "./state";
import {
    ALL_ZONING_MODES,
    ZoningMode,
    zoneModeIconMap,
    getModeFromString,
} from "./zoning-toolkit-utils";

import styles from "./zoning-toolkit-panel.module.scss";

type PanelProps = ModUIState;

const ZoningToolkitPanelInternal: React.FC<PanelProps> = (props) => {
    const { uiVisible, photomodeActive, zoningMode, updateZoningMode } = props;

    // Normalize C# sent string to our ZoningMode union.
    const currentMode: ZoningMode = getModeFromString(zoningMode);

    // Simple drag state (pixel-based)
    const [position, setPosition] = React.useState<{ x: number; y: number }>({
        x: 400,
        y: 200,
    });

    const dragStartRef = React.useRef<{ offsetX: number; offsetY: number } | null>(
        null
    );

    const handleMouseMove = (event: MouseEvent) => {
        if (!dragStartRef.current) {
            return;
        }

        const nextX = event.clientX - dragStartRef.current.offsetX;
        const nextY = event.clientY - dragStartRef.current.offsetY;

        setPosition({
            x: Math.max(0, nextX),
            y: Math.max(0, nextY),
        });
    };

    const handleMouseUp = () => {
        dragStartRef.current = null;
        window.removeEventListener("mousemove", handleMouseMove);
        window.removeEventListener("mouseup", handleMouseUp);
    };

    const handleMouseDown: React.MouseEventHandler<HTMLDivElement> = (event) => {
        // Only left-button drag.
        if (event.button !== 0) {
            return;
        }

        const rect = event.currentTarget.getBoundingClientRect();
        dragStartRef.current = {
            offsetX: event.clientX - rect.left,
            offsetY: event.clientY - rect.top,
        };

        window.addEventListener("mousemove", handleMouseMove);
        window.addEventListener("mouseup", handleMouseUp);
    };

    // Hide panel if not visible or photomode is active
    if (!uiVisible || photomodeActive) {
        return null;
    }

    return (
        <div
            className={styles.panel}
            style={{
                left: `${position.x}px`,
                top: `${position.y}px`,
            }}
            onMouseDown={handleMouseDown}
        >
            {/* Zone Tools title; Vanilla handles font and color */}
            <div className={styles.panelHeader}>Zone Tools</div>

            <div className={styles.panelBody}>
                <div className={styles.panelToolModeRow}>
                    {ALL_ZONING_MODES.map((mode) => {
                        const isActive = currentMode === mode;
                        const buttonClass = `${styles.modeButton} ${isActive ? styles.modeButtonActive : ""
                            }`.trim();

                        return (
                            <button
                                key={mode}
                                type="button"
                                className={buttonClass}
                                onClick={() => updateZoningMode(mode)}
                                title={`Change to ${mode}`}
                            >
                                <img
                                    src={zoneModeIconMap[mode]}
                                    alt={mode}
                                    className={styles.modeIcon}
                                />
                            </button>
                        );
                    })}
                </div>
            </div>
        </div>
    );
};

export const ZoningToolkitPanel = withStore(ZoningToolkitPanelInternal);
