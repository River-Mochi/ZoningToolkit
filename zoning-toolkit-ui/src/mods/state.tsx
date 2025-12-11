// src/mods/state.tsx
// Global UI state for Zone Tools (Zustand store + Cohtml event wiring + HOC).

import engine, { EventHandle } from "cohtml/cohtml";
import { create } from "zustand";
import React from "react";

// Shape of the UI store that panel + button read from.
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

// Keep track of all Cohtml subscriptions so they can be cleaned up.
const allSubscriptions = new Map<string, () => void>();

// Namespace shared with C# ZoneToolBridgeUI.
const NS = "zoning_adjuster_ui_namespace";

// Central Zustand store used everywhere.
export const useModUIStore = create<ModUIState>((set) => ({
    uiVisible: false,
    photomodeActive: false,
    zoningMode: "Default",
    isFocused: false,
    isEnabled: false,
    isToolEnabled: false,

    updateUiVisible: (newValue: boolean) => {
        console.log(`[ZoneTools] Updating uiVisible = ${newValue}`);
        set({ uiVisible: newValue });
        // NOTE: visible is currently driven from C# → JS; we do not send it back.
        // If needed later we can add a C# trigger binding for NS.visible.
    },

    updatePhotomodeActive: (newValue: boolean) => {
        console.log(`[ZoneTools] Updating photomodeActive = ${newValue}`);
        set({ photomodeActive: newValue });
    },

    updateIsToolEnabled: (newValue: boolean) => {
        console.log(`[ZoneTools] Updating isToolEnabled = ${newValue}`);
        // Tell C# side to enable/disable update tool.
        sendDataToCSharp(NS, "tool_enabled", newValue);
        set({ isToolEnabled: newValue });
    },

    updateZoningMode: (newValue: string) => {
        console.log(`[ZoneTools] Updating zoningMode = ${newValue}`);
        // Tell C# side the zoning mode changed.
        sendDataToCSharp(NS, "zoning_mode_update", newValue);
        set({ zoningMode: newValue });
    },
}));

// Called from index.tsx when ZoningToolkitUi mounts.
export const setupSubscriptions = () => {
    console.log("[ZoneTools] Creating subscriptions for Zone Tools UI.");

    // zoning_mode (string)
    const zoningModeEventKey = `${NS}.zoning_mode`;
    if (!allSubscriptions.has(zoningModeEventKey)) {
        const subscription = updateEventFromCSharp<string>(
            NS,
            "zoning_mode",
            (zoningMode) => {
                console.log(`[ZoneTools] zoning_mode update from C#: ${zoningMode}`);
                useModUIStore.getState().updateZoningMode(zoningMode);
            },
        );
        allSubscriptions.set(zoningModeEventKey, subscription);
    }

    // tool_enabled (bool)
    const toolEnabledEventKey = `${NS}.tool_enabled`;
    if (!allSubscriptions.has(toolEnabledEventKey)) {
        const subscription = updateEventFromCSharp<boolean>(
            NS,
            "tool_enabled",
            (toolEnabled) => {
                console.log(`[ZoneTools] tool_enabled update from C#: ${toolEnabled}`);
                // Avoid re-triggering C# trigger; just update store.
                useModUIStore.setState({ isToolEnabled: toolEnabled });
            },
        );
        allSubscriptions.set(toolEnabledEventKey, subscription);
    }

    // visible (bool) – drives panel show/hide (Shift+Z + tool logic).
    const visibleEventKey = `${NS}.visible`;
    if (!allSubscriptions.has(visibleEventKey)) {
        const subscription = updateEventFromCSharp<boolean>(
            NS,
            "visible",
            (visible) => {
                console.log(`[ZoneTools] visible update from C#: ${visible}`);
                useModUIStore.getState().updateUiVisible(visible);
            },
        );
        allSubscriptions.set(visibleEventKey, subscription);
    }

    // photomode (bool)
    const photomodeEventKey = `${NS}.photomode`;
    if (!allSubscriptions.has(photomodeEventKey)) {
        const subscription = updateEventFromCSharp<boolean>(
            NS,
            "photomode",
            (photomodeEnabled) => {
                console.log(
                    `[ZoneTools] photomode update from C#: ${photomodeEnabled}`,
                );
                useModUIStore.getState().updatePhotomodeActive(photomodeEnabled);
            },
        );
        allSubscriptions.set(photomodeEventKey, subscription);
    }
};

// Called from index.tsx when ZoningToolkitUi unmounts.
export const teardownSubscriptions = () => {
    console.log("[ZoneTools] Destroying subscriptions for Zone Tools UI.");

    allSubscriptions.forEach((unsubscribe, eventKey) => {
        console.log(`[ZoneTools] Unsubscribing from event ${eventKey}`);
        unsubscribe();
    });

    allSubscriptions.clear();
};

// Generic helper: subscribe to a Cohtml event from C# side.
export function updateEventFromCSharp<T>(
    ns: string,
    eventName: string,
    callback: (value: T) => void,
): () => void {
    console.log(`[ZoneTools] Subscribing to ${ns}.${eventName}.update events from C#.`);

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
    newValue: T,
): void {
    console.log(`[ZoneTools] Sending to C#: ${ns}.${eventName} =`, newValue);
    engine.trigger(`${ns}.${eventName}`, newValue);
}

// Very simple HOC: wraps a component that expects (optionally) ModUIState props
// and returns a no-props component that reads from the Zustand store.
export function withStore(
    WrappedComponent: React.ComponentType<Partial<ModUIState>>,
): React.FC {
    const WithStore: React.FC = () => {
        const storeState = useModUIStore();
        return <WrappedComponent {...storeState} />;
    };

    WithStore.displayName = `WithStore(${WrappedComponent.displayName ?? WrappedComponent.name ?? "Component"})`;

    return WithStore;
}
