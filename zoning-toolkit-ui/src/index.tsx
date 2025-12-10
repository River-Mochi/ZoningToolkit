// zoning-toolkit-ui/src/index.tsx
// Entry point for the Zone Tools UI (panel + GameTopLeft button).

import React from "react";
import { ModRegistrar } from "cs2/modding";
import { ZoningToolkitPanel } from "mods/zoning-toolkit-panel";
import { ZoningToolkitMenuButton } from "./mods/zoning-toolkit-button";
import { setupSubscriptions, teardownSubscriptions } from "./mods/state";

const register: ModRegistrar = (moduleRegistry) => {
    console.log("ZoningToolkit: Registering modules");

    moduleRegistry.find(".*").forEach((each) => {
        console.log(`Module: ${each}`);
    });

    // Mount React UI into GameTopLeft.
    moduleRegistry.append("GameTopLeft", () => <ZoningToolkitUi />);
};

class ZoningToolkitUi extends React.Component<Record<string, never>> {
    componentDidMount() {
        setupSubscriptions();
    }

    componentWillUnmount() {
        teardownSubscriptions();
    }

    render() {
        return (
            <>
                <ZoningToolkitPanel />
                <ZoningToolkitMenuButton />
            </>
        );
    }
}

export default register;
