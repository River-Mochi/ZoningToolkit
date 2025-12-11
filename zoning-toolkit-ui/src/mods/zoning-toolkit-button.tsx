// src/mods/zoning-toolkit-button.tsx
// GameTopLeft floating action button that toggles the Zone Tools panel.

import React, { CSSProperties } from "react";
import { Button } from "cs2/ui";

import menuIcon from "../../assets/icons/menu_icon.svg";
import menuButtonStyles from "./zoning-toolkit-button.module.scss";
import { useModUIStore, withStore } from "./state";
import VanillaBindings from "./vanilla-bindings";

const { DescriptionTooltip } = VanillaBindings.components;

class ZoningToolkitMenuButtonInternal extends React.Component {
    private handleMenuButtonClick = (): void => {
        const store = useModUIStore.getState();
        store.updateUiVisible(!store.uiVisible);
    };

    public render(): JSX.Element | null {
        const store = useModUIStore.getState();
        const photomodeActive = store.photomodeActive;

        // Menu button should be hidden in photo mode.
        const buttonStyle: CSSProperties = {
            display: photomodeActive ? "none" : undefined,
        };

        return (
            <DescriptionTooltip
                description="Control/modify zoning along roads. Allows zoning on both sides of roads, on any one side, or no sides at all."
                direction="right"
                title="Zone Tools"
            >
                <Button
                    style={buttonStyle}
                    variant="floating"
                    onClick={this.handleMenuButtonClick}
                >
                    <img
                        src={menuIcon}
                        className={menuButtonStyles.menuIcon}
                    />
                </Button>
            </DescriptionTooltip>
        );
    }
}

// Wrapped with the Zustand store HOC so it can re-render when store changes,
// even though the state is read via useModUIStore.getState().
export const ZoningToolkitMenuButton = withStore(
    ZoningToolkitMenuButtonInternal,
);
