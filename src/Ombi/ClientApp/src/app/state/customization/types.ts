import { ICustomizationSettings } from "../../interfaces";
import { StateToken } from "@ngxs/store";

export const CUSTOMIZATION_STATE_TOKEN = new StateToken<ICustomizationSettings>('customization');