// eslint.config.mjs
import js from "@eslint/js";
import globals from "globals";
import tseslint from "typescript-eslint";
import pluginReact from "eslint-plugin-react";
import { defineConfig } from "eslint/config";

export default defineConfig([
    // 1) Global ignores
    {
        ignores: [
            "build/**",
            "dist/**",
            "node_modules/**",
        ],
    },

    // 2) Base JS/TS files + browser globals
    {
        files: ["**/*.{js,mjs,cjs,ts,mts,cts,jsx,tsx}"],
        languageOptions: {
            globals: {
                ...globals.browser,
            },
        },
    },

    // 3) TypeScript recommended (already flat-config ready)
    ...tseslint.configs.recommended,

    // 4) React plugin wired up in flat style
    {
        plugins: {
            react: pluginReact,
        },
        settings: {
            react: {
                // Fixes: "React version not specified"
                version: "detect",
            },
        },
        rules: {
            // Base JS recommended rules
            ...js.configs.recommended.rules,

            // React recommended rules, but only the rules object (no legacy `plugins` key)
            ...pluginReact.configs.recommended.rules,

            // You’re on React 17+ with JSX transform; no need for React in scope
            "react/react-in-jsx-scope": "off",
        },
    },
]);
