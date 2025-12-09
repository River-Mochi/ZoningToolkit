// eslint.config.mjs
// Flat config for Zone Tools UI (React + TypeScript, ESLint 9).

import eslintJs from "@eslint/js";
import globals from "globals";
import tseslint from "typescript-eslint";
import reactPlugin from "eslint-plugin-react";

// Use typescript-eslint's flat-config helper to compose configs.
export default tseslint.config(
    // Base JS recommended rules
    eslintJs.configs.recommended,

    // Base TS rules (no type-checking, fine for this UI mod)
    ...tseslint.configs.recommended,

    // Project-specific config: React + browser globals + custom rules
    {
        files: ["**/*.{ts,tsx,js,jsx}"],
        languageOptions: {
            ecmaVersion: "latest",
            sourceType: "module",
            globals: {
                ...globals.browser,
                console: "readonly",
            },
        },
        plugins: {
            react: reactPlugin,
        },
        settings: {
            react: {
                version: "detect",
            },
        },
        rules: {
            // Turn off base rule and use TS version instead
            "no-unused-vars": "off",

            // Allow leading "_" for intentionally unused vars/args
            "@typescript-eslint/no-unused-vars": [
                "error",
                {
                    args: "all",
                    argsIgnorePattern: "^_",
                    varsIgnorePattern: "^_",
                    caughtErrors: "all",
                    caughtErrorsIgnorePattern: "^_",
                    ignoreRestSiblings: true,
                },
            ],

            // React rules that don't make sense for CS2 UI
            "react/react-in-jsx-scope": "off",
            "react/prop-types": "off",
        },
    },
);
