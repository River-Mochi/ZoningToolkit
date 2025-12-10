// src/mods/zoning-toolkit-utils.ts
// Shared zoning-mode helpers + icon mapping for the Zone Tools UI.
import ReplaceToolIcon from "../../assets/icons/replace_tool_icon.svg";
import ModeIconDefault from "../../assets/icons/mode_icon_default.svg";
import ModeIconNone from "../../assets/icons/mode_icon_none.svg";
import ModeIconLeft from "../../assets/icons/mode_icon_left.svg";
import ModeIconRight from "../../assets/icons/mode_icon_right.svg";

// The zoning mode string values are aligned with what C# sends
// (e.g. "Default", "None", "Left", "Right").
export type ZoningMode = "Default" | "None" | "Left" | "Right";

export const ZONING_MODE_DEFAULT: ZoningMode = "Default";
export const ZONING_MODE_NONE: ZoningMode = "None";
export const ZONING_MODE_LEFT: ZoningMode = "Left";
export const ZONING_MODE_RIGHT: ZoningMode = "Right";

export const ALL_ZONING_MODES: ZoningMode[] = [
    ZONING_MODE_DEFAULT,
    ZONING_MODE_NONE,
    ZONING_MODE_LEFT,
    ZONING_MODE_RIGHT,
];

// Header icon used by the panel (if needed later).
export { ReplaceToolIcon };

// Per-mode icons used for the four zoning buttons in the panel.
export const zoneModeIconMap: Record<ZoningMode, string> = {
    Default: ModeIconDefault,
    None: ModeIconNone,
    Left: ModeIconLeft,
    Right: ModeIconRight,
};

/**
 * Defensive helper so a valid ZoningMode is always returned
 * even if C# or the UI sends something unexpected.
 */
export function getModeFromString(value: string): ZoningMode {
    switch (value) {
        case "Default":
            return ZONING_MODE_DEFAULT;
        case "None":
            return ZONING_MODE_NONE;
        case "Left":
            return ZONING_MODE_LEFT;
        case "Right":
            return ZONING_MODE_RIGHT;
        default:
            // Fallback if something weird comes through
            return ZONING_MODE_DEFAULT;
    }
}
