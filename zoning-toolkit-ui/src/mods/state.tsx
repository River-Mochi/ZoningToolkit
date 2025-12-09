// src/mods/state.tsx
// Global UI state for Zone Tools (Zustand store + Cohtml event wiring + HOC).

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

// Keep track of all Cohtml subscriptions so we can clean up.
const allSubscriptions = new Map<string, () => void>();

// Central Zustand store used everywhere.
export const useModUIStore = create<ModUIState>((set) => ({
    uiVisible: false,
    photomodeActive: false,
    zoningMode: "Default",
    isFocused: false,
    isEnabled: false,
    isToolEnabled: false,

    updateUiVisible: (newValue: boolean) => {
        console.log(`Updating uiVisible = ${newValue}`);
        set({ uiVisible: newValue });
    },

    updatePhotomodeActive: (newValue: boolean) => {
        console.log(`Updating photomodeActive = ${newValue}`);
        set({ photomodeActive: newValue });
    },

    updateIsToolEnabled: (newValue: boolean) => {
        console.log(`Updating isToolEnabled = ${newValue}`);
        // Tell C# side to enable/disable tool.
        sendDataToCSharp("zoning_adjuster_ui_namespace", "tool_enabled", newValue);
        set({ isToolEnabled: newValue });
    },

    updateZoningMode: (newValue: string) => {
        console.log(`Updating zoningMode = ${newValue}`);
        // Tell C# side the zoning mode changed.
        sendDataToCSharp("zoning_adjuster_ui_namespace", "zoning_mode_update", newValue);
        set({ zoningMode: newValue });
    },
}));

// Called from index.tsx::componentDidMount
export const setupSubscriptions = () => {
    console.log("Creating subscriptions for Zone Tools UI.");

    // zoning_mode (string)
    const zoningModeEventKey = "zoning_adjuster_ui_namespace.zoning_mode";
    if (!allSubscriptions.has(zoningModeEventKey)) {
        const subscription = updateEventFromCSharp<string>(
            "zoning_adjuster_ui_namespace",
            "zoning_mode",
            (zoningMode) => {
                console.log(`zoning_mode update from C#: ${zoningMode}`);
                useModUIStore.getState().updateZoningMode(zoningMode);
            }
        );
        allSubscriptions.set(zoningModeEventKey, subscription);
    }

    // tool_enabled (bool)
    const toolEnabledEventKey = "zoning_adjuster_ui_namespace.tool_enabled";
    if (!allSubscriptions.has(toolEnabledEventKey)) {
        const subscription = updateEventFromCSharp<boolean>(
            "zoning_adjuster_ui_namespace",
            "tool_enabled",
            (toolEnabled) => {
                console.log(`tool_enabled update from C#: ${toolEnabled}`);
                useModUIStore.getState().updateIsToolEnabled(toolEnabled);
            }
        );
        allSubscriptions.set(toolEnabledEventKey, subscription);
    }

    // visible (bool) – currently unused by the React side, but wired for future use.
    /*
    const visibleEventKey = "zoning_adjuster_ui_namespace.visible";
    if (!allSubscriptions.has(visibleEventKey)) {
        const subscription = updateEventFromCSharp<boolean>(
            "zoning_adjuster_ui_namespace",
            "visible",
            (visible) => {
                console.log(`visible update from C#: ${visible}`);
                useModUIStore.getState().updateUiVisible(visible);
            }
        );
        allSubscriptions.set(visibleEventKey, subscription);
    }
    */

    // photomode (bool)
    const photomodeEventKey = "zoning_adjuster_ui_namespace.photomode";
    if (!allSubscriptions.has(photomodeEventKey)) {
        const subscription = updateEventFromCSharp<boolean>(
            "zoning_adjuster_ui_namespace",
            "photomode",
            (photomodeEnabled) => {
                console.log(`photomode update from C#: ${photomodeEnabled}`);
                useModUIStore.getState().updatePhotomodeActive(photomodeEnabled);
            }
        );
        allSubscriptions.set(photomodeEventKey, subscription);
    }
};

// Called from index.tsx::componentWillUnmount
export const teardownSubscriptions = () => {
    console.log("Destroying subscriptions for Zone Tools UI.");

    allSubscriptions.forEach((unsubscribe, eventKey) => {
        console.log(`Unsubscribing from event ${eventKey}`);
        unsubscribe();
    });

    allSubscriptions.clear();
};

// Generic helper: subscribe to a Cohtml event from C# side.
export function updateEventFromCSharp<T>(
    ns: string,
    eventName: string,
    callback: (value: T) => void
): () => void {
    console.log(`Subscribing to ${ns}.${eventName} events from C#.`);

    const updateEvent = `${ns}.${eventName}.update`;
    const subscribeEvent = `${ns}.${eventName}.subscribe`;
    const unsubscribeEvent = `${ns}.${eventName}.unsubscribe`;

    const sub: EventHandle = engine.on(updateEvent, callback);
    engine.trigger(subscribeEvent);

    return () => {
        engine.trigger(unsubscribeEvent);
        sub.clear();
    };
}

// Trigger an event from JS -> C#
export function sendDataToCSharp<T>(
    ns: string,
    eventName: string,
    newValue: T
): void {
    console.log(`Sending to C#: ${ns}.${eventName} =`, newValue);
    engine.trigger(`${ns}.${eventName}`, newValue);
}

// Very simple HOC: wraps a component that expects ModUIState props
// and returns a no-props component that reads from the Zustand store.
export function withStore(
    WrappedComponent: React.ComponentType<ModUIState>
): React.FC {
    const WithStore: React.FC = () => {
        const storeState = useModUIStore();
        return <WrappedComponent {...storeState} />;
    };

    WithStore.displayName = `WithStore(${WrappedComponent.displayName ?? WrappedComponent.name ?? "Component"
        })`;

    return WithStore;
}
