import { CUSTOMIZATION_STATE_TOKEN } from "./types";
import { ICustomizationSettings } from "../../interfaces";
import { Selector } from "@ngxs/store";

export class CustomizationSelectors {

    @Selector([CUSTOMIZATION_STATE_TOKEN])
    public static customizationSettings(settings: ICustomizationSettings): ICustomizationSettings {
        return settings;
    }

    @Selector([CustomizationSelectors.customizationSettings])
    public static logo({logo}: ICustomizationSettings): string {
        return logo;
    }

    @Selector([CustomizationSelectors.customizationSettings])
    public static applicationName({applicationName}: ICustomizationSettings): string {
        return applicationName;
    }

    @Selector([CustomizationSelectors.customizationSettings])
    public static applicationUrl({applicationUrl}: ICustomizationSettings): string {
        return applicationUrl;
    }
}