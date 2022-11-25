import { RadarrState, RADARR_STATE_TOKEN } from "./types";
import { Selector } from "@ngxs/store";
import { IRadarrCombined } from "../../interfaces";

export class RadarrSelectors {

    @Selector([RADARR_STATE_TOKEN])
    public static state(state: RadarrState): RadarrState {
        return state;
    }

    @Selector([RadarrSelectors.state])
    public static settings(state: RadarrState): IRadarrCombined {
        return state.settings;
    }

    @Selector([RadarrSelectors.settings])
    public static isEnabled(settings: IRadarrCombined): boolean {
        return settings?.radarr?.enabled ?? false;
    }

    @Selector([RadarrSelectors.settings])
    public static is4KEnabled(settings: IRadarrCombined): boolean {
        return settings?.radarr4K?.enabled ?? false;
    }
}