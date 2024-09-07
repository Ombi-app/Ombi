import { IRadarrCombined } from "../../interfaces";
import { StateToken } from "@ngxs/store";

export const RADARR_STATE_TOKEN = new StateToken<RadarrState>('RadarrState');

export interface RadarrState {
    settings: IRadarrCombined;
}