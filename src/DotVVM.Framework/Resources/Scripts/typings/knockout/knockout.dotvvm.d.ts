interface KnockoutStatic {
    delaySync: KnockoutDelaySync;
}
interface KnockoutDelaySync {
    pause(): void;
    isPaused: boolean;
    resume(): void;
    run(action: () => void): void;
}
