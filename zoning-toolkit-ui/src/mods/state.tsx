// zoning-toolkit-ui/src/mods/state.tsx

import engine, { EventHandle } from "cohtml/cohtml";
import React from "react";
import { create } from "zustand";

export interface ModUIState {
    uiVisible: boolean;
    photomodeActive: boolean;
    zoningMode: string;
    isFocused: boolean;
    isEnabled: boolean;
    isToolEnabled: boolean;
    updateZoningMode: (newValue: string) => void;
    updateIsToolEnabled: (newValue: boolean) => void;
    updatePhotomodeActive: (newValue: boolean) => void;
    updateUiVisible: (newValue: boolean) => void;
}

const allSubscriptions: Map<string, () => void> = new Map();

export const setupSubscriptions = (): void => {
    console.log("Creating subscriptions.");

    // Init subscriptions from Mod UI System
    const subscriptionZoningModeEventString = "zoning_adjuster_ui_namespace.zoning_mode";
    if (!allSubscriptions.has(subscriptionZoningModeEventString)) {
        const subscription = updateEventFromCSharp<string>(
            "zoning_adjuster_ui_namespace",
            "zoning_mode",
            (zoningMode) => {
                console.log(`Zoning mode fetched ${zoningMode}`);
                useModUIStore.getState().updateZoningMode(zoningMode);
            }
        );
        allSubscriptions.set(subscriptionZoningModeEventString, subscription);
    }

    const subscriptionToolEnabledEventString = "zoning_adjuster_ui_namespace.tool_enabled";
    if (!allSubscriptions.has(subscriptionToolEnabledEventString)) {
        const subscription = updateEventFromCSharp<boolean>(
            "zoning_adjuster_ui_namespace",
            "tool_enabled",
            (toolEnabled) => {
                console.log(`Tool Enabled Toggled ${toolEnabled}`);
                useModUIStore.getState().updateIsToolEnabled(toolEnabled);
            }
        );
        allSubscriptions.set(subscriptionToolEnabledEventString, subscription);
    }

    /*
    const subscriptionVisibleEventString = "zoning_adjuster_ui_namespace.visible";
    if (!allSubscriptions.has(subscriptionVisibleEventString)) {
        const subscription = updateEventFromCSharp<boolean>(
            "zoning_adjuster_ui_namespace",
            "visible",
            (visible) => {
                console.log(`UI visibility changed to ${visible}`);
                useModUIStore.getState().updateUiVisible(visible);
            }
        );
        allSubscriptions.set(subscriptionVisibleEventString, subscription);
    }
    */

    const photomodeEventString = "zoning_adjuster_ui_namespace.photomode";
    if (!allSubscriptions.has(photomodeEventString)) {
        const subscription = updateEventFromCSharp<boolean>(
            "zoning_adjuster_ui_namespace",
            "photomode",
            (photomodeEnabled) => {
                console.log(`Photomode turned on to ${photomodeEnabled}`);
                useModUIStore.getState().updatePhotomodeActive(photomodeEnabled);
            }
        );
        allSubscriptions.set(photomodeEventString, subscription);
    }
};

export const teardownSubscriptions = (): void => {
    console.log("Destroying subscriptions.");

    // Unsubscribe by calling the callbacks
    allSubscriptions.forEach((callback, eventString) => {
        console.log(`Unsubscribing from event ${eventString}`);
        callback();
    });
};

const setupStore = () => {
    console.log("Initializing store.");
    const useModUIStore = create<ModUIState>((set) => ({
        uiVisible: false,
        photomodeActive: false,
        zoningMode: "Default",
        isFocused: false,
        isEnabled: false,
        isToolEnabled: false,
        updateUiVisible: (newValue: boolean) =>
            set(() => {
                console.log(`Updating UI Visible to ${newValue}`);
                return {
                    uiVisible: newValue,
                };
            }),
        updatePhotomodeActive: (newValue: boolean) =>
            set(() => {
                console.log(`Updating Photomode Active to ${newValue}`);
                return {
                    photomodeActive: newValue,
                };
            }),
        updateIsToolEnabled: (newValue: boolean) =>
            set(() => {
                console.log(`Updating IsToolEnabled ${newValue}`);
                sendDataToCSharp("zoning_adjuster_ui_namespace", "tool_enabled", newValue);
                return {
                    isToolEnabled: newValue,
                };
            }),
        updateZoningMode: (newValue: string) =>
            set(() => {
                console.log(`Updating ZoningMode ${newValue}`);
                sendDataToCSharp("zoning_adjuster_ui_namespace", "zoning_mode_update", newValue);
                return {
                    zoningMode: newValue,
                };
            }),
    }));

    console.log("Store initialized.");
    return useModUIStore;
};

export const useModUIStore = setupStore();

export function updateEventFromCSharp<T>(
    namespace: string,
    event: string,
    callback: (input: T) => void
): () => void {
    console.log("Subscribing to update events from game. Event " + event);
    const updateEvent = `${namespace}.${event}.update`;
    const subscribeEvent = `${namespace}.${event}.subscribe`;
    const unsubscribeEvent = `${namespace}.${event}.unsubscribe`;

    const sub: EventHandle = engine.on(updateEvent, callback);
    engine.trigger(subscribeEvent);
    return () => {
        engine.trigger(unsubscribeEvent);
        sub.clear();
    };
}

export function sendDataToCSharp<T>(namespace: string, event: string, newValue: T): void {
    console.log(`Event triggered. Sending new value ${newValue}`);
    engine.trigger(`${namespace}.${event}`, newValue);
}

/**
 * HOC: injects ModUIState into a wrapped component.
 * The wrapped component must accept its own props P plus ModUIState.
 */
export function withStore<P>(
    WrappedComponent: React.ComponentType<P & ModUIState>
): React.ComponentType<P> {
    console.log("Creating HOC.");

    return class WithStore extends React.Component<P, ModUIState> {
        state: ModUIState = useModUIStore.getState();

        private unsubscribe?: () => void;

        componentDidMount() {
            this.unsubscribe = useModUIStore.subscribe((storeState) => {
                this.setState(storeState);
            });
        }

        componentWillUnmount() {
            if (this.unsubscribe) {
                this.unsubscribe();
            }
        }

        render() {
            return <WrappedComponent {...this.props} {...this.state} />;
        }
    };
}
