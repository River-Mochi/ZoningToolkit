// src/mods/zoning-toolkit-utils.ts
// Shared zoning-mode helpers and icon mapping.

import replaceToolIcon from "../../assets/icons/replace_tool_icon.svg";

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

// Right now all modes share the same icon; you can split later if needed.
export const zoneModeIconMap: Record<ZoningMode, string> = {
    Default: replaceToolIcon,
    None: replaceToolIcon,
    Left: replaceToolIcon,
    Right: replaceToolIcon,
};

export function getModeFromString(
    value: string | null | undefined
): ZoningMode {
    switch (value) {
        case "None":
            return ZONING_MODE_NONE;
        case "Left":
            return ZONING_MODE_LEFT;
        case "Right":
            return ZONING_MODE_RIGHT;
        case "Default":
        default:
            return ZONING_MODE_DEFAULT;
    }
}
