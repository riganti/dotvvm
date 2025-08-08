# Svelte library

Everything you need to build a Svelte library, powered by [`sv`](https://npmjs.com/package/sv). Read more about creating a library [in the docs](https://svelte.dev/docs/kit/packaging).

Everything inside `src/lib` is part of your library.

## Building

To build your library:

```sh
npm pack
```

## Publishing

Go into the `package.json` and give your package the desired name through the `"name"` option. Also consider adding a `"license"` field and point it to a `LICENSE` file which you can create from a template (one popular option is the [MIT license](https://opensource.org/license/mit/)).

To publish your library to [npm](https://www.npmjs.com):

```sh
npm publish
```
