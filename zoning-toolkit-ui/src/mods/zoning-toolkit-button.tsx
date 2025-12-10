// src/mods/zoning-toolkit-button.tsx
// GameTopLeft icon button that toggles the Zone Tools zoning tool on/off.

import React from "react";
import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { useModUIStore } from "./state";
import menuIcon from "../../assets/icons/menu_icon.svg";

import styles from "./zoning-toolkit-button.module.scss";

export const ZoningToolkitMenuButton: React.FC = () => {
    const { translate } = useLocalization();

    const isToolEnabled = useModUIStore((state) => state.isToolEnabled);
    const updateIsToolEnabled = useModUIStore(
        (state) => state.updateIsToolEnabled,
    );

    const tooltipLabel = translate("ZoneTools.ToolName", "Zone Tools");

    const handleClick = () => {
        // Toggle the visibility of the panel.
        useModUIStore.getState().updateUiVisible(!useModUIStore.getState().uiVisible);
    };

    return (
        <Button
            variant="icon"
            src={menuIcon}
            tooltipLabel={tooltipLabel}
            onClick={handleClick}
            className={styles.menuIcon}
        />
    );
};
