declare namespace jest {
    interface Matchers<R = {}> {
        observable(): R;
        observableArray(): R;
    }
}
