// eslint.config.mjs
// Flat config for Zone Tools UI (ESLint 9 + React + TypeScript).
// IMPORTANT: We only lint mod source under /src.
// We explicitly ignore Node/CommonJS build scripts (webpack.config.js, tools/*) and toolchain types.

import eslintJs from "@eslint/js";
import globals from "globals";
import tseslint from "typescript-eslint";
import reactPlugin from "eslint-plugin-react";
import reactHooksPlugin from "eslint-plugin-react-hooks";

export default tseslint.config(
    // Ignore everything we do NOT want VS/ESLint to lint
    {
        ignores: [
            "**/node_modules/**",
            "**/build/**",
            "**/types/**",
            "**/tools/**",
            "webpack.config.js",
            "webpack.*.js",
            "**/*.d.ts",
        ],
    },

    // Base JS recommended rules
    eslintJs.configs.recommended,

    // Base TS rules (no type-checking)
    ...tseslint.configs.recommended,

    // Lint ONLY your actual mod source
    {
        files: ["src/**/*.{ts,tsx,js,jsx}"],
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
            "react-hooks": reactHooksPlugin,
        },
        settings: {
            react: { version: "detect" },
        },
        rules: {
            // Prefer TS unused-vars rule
            "no-unused-vars": "off",
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

            // React rules that don’t apply with modern JSX transform / TS
            "react/react-in-jsx-scope": "off",
            "react/prop-types": "off",

            // Hooks rules (same as plugin:react-hooks/recommended)
            ...reactHooksPlugin.configs.recommended.rules,
        },
    },
);
