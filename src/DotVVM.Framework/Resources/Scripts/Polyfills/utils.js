if (!Math.trunc) {
    Math.trunc = function (v) {
        return v < 0 ? Math.ceil(v) : Math.floor(v);
    };
}