import { CUSTOMIZATION_STATE_TOKEN } from "./types";
import { ICustomizationSettings } from "../../interfaces";
import { Selector } from "@ngxs/store";

export class CustomizationSelectors {

    @Selector([CUSTOMIZATION_STATE_TOKEN])
    public static customizationSettings(settings: ICustomizationSettings): ICustomizationSettings {
        return settings;
    }
}