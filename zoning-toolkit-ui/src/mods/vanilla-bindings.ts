// src/mods/vanilla-bindings.ts
// Thin wrappers over vanilla UI components (tooltip + tool button + focus keys).

import { getModule } from "cs2/modding";
import { ButtonProps, TooltipProps } from "cs2/ui";

export interface DescriptionTooltipProps
    extends Omit<TooltipProps, "tooltip"> {
    title: string | null;
    description: string | null;
    content?: unknown;
}

/** Tooltip with title and description (vanilla component). */
const DescriptionTooltip = getModule(
    "game-ui/common/tooltip/description-tooltip/description-tooltip.tsx",
    "DescriptionTooltip",
);

export interface ToolButtonProps extends ButtonProps {
    /** Icon source */
    src: string;
    tooltip?: string;
}

/** Toolbar icon button (with selection state, vanilla component). */
const ToolButton = getModule(
    "game-ui/game/components/tool-options/tool-button/tool-button.tsx",
    "ToolButton",
);

/** Manually exported/bound modules that are not exported directly by CS2; use with caution! */
const VanillaBindings = {
    components: {
        DescriptionTooltip,
        ToolButton,
    },
    common: {
        focus: {
            auto: getModule("game-ui/common/focus/focus-key.ts", "FOCUS_AUTO"),
            disabled: getModule(
                "game-ui/common/focus/focus-key.ts",
                "FOCUS_DISABLED",
            ),
        },
    },
};

export default VanillaBindings;
