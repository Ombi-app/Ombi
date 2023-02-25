import { SonarrState, SONARR_STATE_TOKEN } from "./types";
import { Selector } from "@ngxs/store";
import { ISonarrSettings } from "../../interfaces";

export class SonarrSelectors {

    @Selector([SONARR_STATE_TOKEN])
    public static state(state: SonarrState): SonarrState {
        return state;
    }

    @Selector([SonarrSelectors.state])
    public static version(state: SonarrState): string {
        return state.version;
    }

    @Selector([SonarrSelectors.state])
    public static settings(state: SonarrState): ISonarrSettings {
        return state.settings;
    }

    @Selector([SonarrSelectors.state])
    public static isEnabled(state: SonarrState): boolean {
        return state.settings?.enabled ?? false;
    }
}