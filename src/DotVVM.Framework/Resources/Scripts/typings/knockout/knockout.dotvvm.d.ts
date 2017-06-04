interface KnockoutStatic {
    delaySync: KnockoutDelaySync;
}
interface KnockoutDelaySync {
    pause(): void;
    resume(): void;
    run(action: () => void): void;
}