import { DotvvmEvent } from "../events";

export const spaNavigating = new DotvvmEvent<DotvvmSpaNavigatingEventArgs>("dotvvm.events.spaNavigating");
export const spaNavigated = new DotvvmEvent<DotvvmSpaNavigatedEventArgs>("dotvvm.events.spaNavigated");
export const spaNavigationFailed = new DotvvmEvent<DotvvmSpaNavigationFailedEventArgs>("dotvvm.events.spaNavigationFailed");
