const esbuild = require('esbuild')

let printStats = Boolean(process.env.PRINT_STATS)

let results = {}

async function build({ debug, spa, output, input = "dotvvm-root.ts" }) {
    return results[output] = await esbuild.build({
        format: 'esm',
        bundle: true,
        entryPoints: [`./Resources/Scripts/${input}`],
        outfile: `./obj/javascript/${output}/dotvvm-root.js`,
        define: {
            "compileConstants.isSpa": spa,
            "compileConstants.debug": debug,
        },
        target: [
            'es2020'
        ],
        sourcemap: true,
        treeShaking: true,
        minify: !debug,
        metafile: printStats,
        mangleProps: debug ? undefined : /^_/, // allow renaming of properties starting with _
    }).catch(() => process.exit(1))
}

async function buildDebugAndProd(options) {
    await build({ ...options, debug: true, output: options.output + "-debug" })
    await build({ ...options, debug: false, output: options.output })
}

async function main() {
    await buildDebugAndProd({ spa: false, output: "root-only" })
    await buildDebugAndProd({ spa: true, output: "root-spa" })

    if (printStats) {
        const fs = require('fs')
        const zlib = require('zlib')
        const outputs = [
            'root-spa',
            'root-only'
        ]

        for (const f of outputs) {
            fs.writeFileSync(`./obj/javascript/${f}/meta.json`,
                JSON.stringify(results[f].metafile))

            const file = fs.readFileSync(`./obj/javascript/${f}/dotvvm-root.js`)
            const gzipCompressed = zlib.gzipSync(file).byteLength
            const brotliCompressed = zlib.brotliCompressSync(file).byteLength
            console.log(`${f}\t ${file.byteLength.toLocaleString()} bytes\t ${gzipCompressed.toLocaleString()} gzip bytes\t ${brotliCompressed.toLocaleString()} brotli bytes`)
        }

        console.log("");

        console.log(`Written out ./obj/javascript/root-only/meta.json file, use https://bundle-buddy.com/esbuild to analyze it.`)
    }


}

main()
