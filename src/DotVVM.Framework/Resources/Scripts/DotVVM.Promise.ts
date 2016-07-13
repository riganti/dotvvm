enum DotvvmPromiseState {
    Pending,
    Done,
    Failed
}

interface IDotvvmPromise<TArg> {
    state: DotvvmPromiseState;
    done(callback: (arg: TArg) => void);
    fail(callback: (error: any) => void);
}

class DotvvmPromise<TArg> implements IDotvvmPromise<TArg> {
    private callbacks: Array<(arg: TArg) => void> = [];
    private errorCallbacks: Array<(error) => void> = [];


    public state: DotvvmPromiseState = DotvvmPromiseState.Pending;
    private argument: TArg;
    private error;

    done(callback: (arg: TArg) => void, forceAsync = false) {
        if (this.state === DotvvmPromiseState.Done) {
            if (forceAsync) setTimeout(() => callback(this.argument), 4);
            else callback(this.argument);
        }
        else if (this.state === DotvvmPromiseState.Pending) {
            this.callbacks.push(callback);
        }
    }

    fail(callback: (error) => void, forceAsync = false) {
        if (this.state === DotvvmPromiseState.Failed) {
            if (forceAsync) setTimeout(() => callback(this.error), 4);
            else callback(this.error);
        }
        else if (this.state === DotvvmPromiseState.Pending) {
            this.errorCallbacks.push(callback);
        }
        return this;
    }

    resolve(arg: TArg) {
        if (this.state !== DotvvmPromiseState.Pending) throw new Error(`Can not resolve ${ this.state } promise.`)
        this.state = DotvvmPromiseState.Done;
        this.argument = arg;
        for (var c of this.callbacks) {
            c(arg);
        }
        this.callbacks = null;
        this.errorCallbacks = null;
        return this;
    }

    reject(error) {
        if (this.state != DotvvmPromiseState.Pending) throw new Error(`Can not reject ${ this.state } promise.`)
        this.state = DotvvmPromiseState.Failed;
        this.error = error;
        for (var c of this.errorCallbacks) {
            c(error);
        }
        this.callbacks = null;
        this.errorCallbacks = null;
        return this;
    }

    chainFrom(promise: IDotvvmPromise<TArg>) {
        promise.done(a => this.resolve(a));
        promise.fail(e => this.fail(e));
        return this;
    } 
}