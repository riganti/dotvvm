import typescript from '@rollup/plugin-typescript'
import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import replace from '@rollup/plugin-replace';
import sveltePreprocess from 'svelte-preprocess';
import svelte from 'rollup-plugin-svelte';
import ts from 'typescript'
//import livereload from '@rollup/plugin-livereload';
//import { terser } from '@rollup/plugin-terser';
const production = !process.env.ROLLUP_WATCH;
export default [{
    input: './Scripts/react/react-app.tsx',
    output: {
        format: 'esm',
        file: './script/react-app.js',
        sourcemap: !production
    },
    plugins: [
        typescript({
            tsconfig: "tsconfig.react.json",
            typescript: ts
        }),
        resolve({ browser: true }),
        commonjs({
            include: 'node_modules/**'
        }),
        replace({
            'process.env.NODE_ENV': JSON.stringify('production')
        })
    ]
},{
    input: './Scripts/svelte/svelte-app.ts',
    output: {
        format: 'esm',
        file: './script/svelte-app.js',
        sourcemap: !production
    },
    plugins: [
        svelte({
            dev: !production,
            css: css => {
                css.write("svelte-app.css");
            },
            preprocess: sveltePreprocess(),
        }),
        resolve({ browser: true, dedupe: ['svelte'] }),
        commonjs({
            include: 'node_modules/**'
        }),
        typescript({
            tsconfig: "tsconfig.svelte.json",
            typescript: ts
        }),
        replace({
            'process.env.NODE_ENV': JSON.stringify('production')
        })
    ]
}]

