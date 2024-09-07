import { ICustomizationSettings } from "../../interfaces";

export class LoadSettings {
		public static readonly type = '[Customization] LoadSettings';
	}
	export class UpdateSettings {
		public static readonly type = '[Customization] UpdateSettings';

        constructor(public settings: ICustomizationSettings) { }
	}

