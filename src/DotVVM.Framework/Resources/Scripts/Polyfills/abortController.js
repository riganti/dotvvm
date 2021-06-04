// MIT License, Copyright (c) 2020 Ben Lesh
// Source: https://github.com/benlesh/abort-controller-polyfill
// Version: 0.0.4
(function () {
  const root =
    (typeof globalThis !== "undefined" && globalThis) ||
    (typeof self !== "undefined" && self) ||
    (typeof global !== "undefined" && global);

  if (typeof root.AbortController === "undefined") {
    const SECRET = {};

    root.AbortSignal = (function () {
      function AbortSignal(secret) {
        if (secret !== SECRET) {
          throw new TypeError("Illegal constructor.");
        }
        EventTarget.call(this);
        this._aborted = false;
      }

      AbortSignal.prototype = Object.create(EventTarget.prototype);
      AbortSignal.prototype.constructor = AbortSignal;

      Object.defineProperty(AbortSignal.prototype, "onabort", {
        get: function () {
          return this._onabort;
        },
        set: function (callback) {
          const existing = this._onabort;
          if (existing) {
            this.removeEventListener("abort", existing);
          }
          this._onabort = callback;
          this.addEventListener("abort", callback);
        },
      });

      Object.defineProperty(AbortSignal.prototype, "aborted", {
        get: function () {
          return this._aborted;
        },
      });

      return AbortSignal;
    })();

    root.AbortController = (function () {
      function AbortController() {
        this._signal = new AbortSignal(SECRET);
      }

      AbortController.prototype = Object.create(Object.prototype);

      Object.defineProperty(AbortController.prototype, "signal", {
        get: function () {
          return this._signal;
        },
      });

      AbortController.prototype.abort = function () {
        const signal = this.signal;
        if (!signal.aborted) {
          signal._aborted = true;
          signal.dispatchEvent(new Event("abort"));
        }
      };

      return AbortController;
    })();
  }
})();