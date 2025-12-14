// tools/patch-tsconfig.js
// Purpose: Fix tsconfig.json after CO toolchain update.
// - Restores React/@types lookup by adding node_modules/@types to typeRoots
// - Keeps CS2 injected types from ./types
// - Removes trailing commas (to silence JSON warnings in editors)

const fs = require("fs");
const path = require("path");

const tsconfigPath = path.join(process.cwd(), "tsconfig.json");

function stripTrailingCommas(jsonText) {
    // Remove trailing commas before } or ]
    // Works for tsconfig-style JSON that only has data (no string edge cases here).
    return jsonText.replace(/,\s*([}\]])/g, "$1");
}

function readTsconfig() {
    if (!fs.existsSync(tsconfigPath)) {
        throw new Error(`tsconfig.json not found at: ${tsconfigPath}`);
    }

    const raw = fs.readFileSync(tsconfigPath, "utf8");

    // tsconfig is JSONC-ish, but your file is plain JSON with a trailing comma.
    // Clean commas so JSON.parse won’t choke and editors won’t warn.
    const cleaned = stripTrailingCommas(raw);

    let parsed;
    try {
        parsed = JSON.parse(cleaned);
    } catch (e) {
        // If something else is wrong, print a helpful message.
        throw new Error(
            `Failed to parse tsconfig.json even after stripping trailing commas.\n` +
            `Path: ${tsconfigPath}\n` +
            `Error: ${e.message}`,
        );
    }

    return { parsed, cleaned };
}

function ensureArrayIncludes(arr, value) {
    if (!Array.isArray(arr)) return [value];
    return arr.includes(value) ? arr : [...arr, value];
}

function patchTsconfig(tsconfig) {
    if (!tsconfig.compilerOptions) tsconfig.compilerOptions = {};
    const co = tsconfig.compilerOptions;

    // Ensure JSX runtime is enabled (you already have this; we keep it)
    co.jsx = co.jsx ?? "react-jsx";

    // The important fix:
    // Keep CS2 injected typings AND restore normal @types lookup.
    // Using relative paths that work from repo root.
    const desiredTypeRoots = ["./types", "./node_modules/@types"];

    if (Array.isArray(co.typeRoots)) {
        // Preserve any extra roots they might add, but ensure these two exist.
        co.typeRoots = ensureArrayIncludes(co.typeRoots, "./types");
        co.typeRoots = ensureArrayIncludes(co.typeRoots, "./node_modules/@types");
    } else {
        co.typeRoots = desiredTypeRoots;
    }

    // Optional: make sure moduleResolution stays stable (CO sets Node)
    co.moduleResolution = co.moduleResolution ?? "Node";

    return tsconfig;
}

function writeTsconfig(tsconfig) {
    const pretty = JSON.stringify(tsconfig, null, 2) + "\n";
    fs.writeFileSync(tsconfigPath, pretty, "utf8");
}

function main() {
    const { parsed } = readTsconfig();
    const patched = patchTsconfig(parsed);
    writeTsconfig(patched);

    console.log("[patch-tsconfig] Updated tsconfig.json");
    console.log("  - typeRoots now includes: ./types and ./node_modules/@types");
    console.log("  - trailing commas removed");
}

main();
