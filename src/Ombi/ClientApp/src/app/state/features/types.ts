import { IFeatureEnablement } from "../../interfaces";
import { StateToken } from "@ngxs/store";

export const FEATURES_STATE_TOKEN = new StateToken<IFeatureEnablement[]>('featureEnablement');