import { ICustomizationSettings, IFeatureEnablement } from "../../interfaces";

import { FEATURES_STATE_TOKEN } from "./types";
import { Selector } from "@ngxs/store";

export class FeaturesSelectors {

    @Selector([FEATURES_STATE_TOKEN])
    public static features(features: IFeatureEnablement[]): IFeatureEnablement[] {
        return features;
    }

    @Selector([FeaturesSelectors.features])
    public static is4kEnabled(features: IFeatureEnablement[]): boolean {
        return features.filter(x => x.name === "Movie4KRequests")[0].enabled;
    }

}