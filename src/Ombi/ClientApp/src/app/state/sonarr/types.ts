import { ISonarrSettings } from "../../interfaces";
import { StateToken } from "@ngxs/store";

export const SONARR_STATE_TOKEN = new StateToken<SonarrState>('SonarrState');

export interface SonarrState {
    settings: ISonarrSettings;
    version: string;
}