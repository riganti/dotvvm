import typescript from '@rollup/plugin-typescript'
import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import replace from '@rollup/plugin-replace';
import { sveltePreprocess } from 'svelte-preprocess';
import svelte from 'rollup-plugin-svelte';
import ts from 'typescript'
//import livereload from '@rollup/plugin-livereload';
//import { terser } from '@rollup/plugin-terser';
const production = false;
export default [{
    input: './Scripts/react/react-app.tsx',
    preserveSymlinks: true,
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
        commonjs(),
        replace({
            'process.env.NODE_ENV': JSON.stringify('production'),
            preventAssignment: true
        })
    ]
},{
    input: './Scripts/svelte/svelte-app.ts',
    external: ['Scripts/svelte/Chart.css'],
    preserveSymlinks: true,
    output: {
        format: 'esm',
        file: './script/svelte-app.js',
        sourcemap: !production
    },
    plugins: [
        svelte({
            compilerOptions: {
                dev: !production,
                css: "injected",
            },
            emitCss: false,
            preprocess: sveltePreprocess(),
        }),
        resolve({ 
            browser: true, 
            dedupe: ['svelte'],
            alias: {
                'svelte': 'svelte'
            }
        }),
        commonjs(),
        typescript({
            tsconfig: "tsconfig.svelte.json",
            typescript: ts,
            sourceMap: !production
        }),
        replace({
            'process.env.NODE_ENV': JSON.stringify('production'),
            preventAssignment: true
        })
    ]
}]

