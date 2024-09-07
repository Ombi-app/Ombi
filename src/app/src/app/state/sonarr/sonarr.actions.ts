import { ISonarrSettings } from "../../interfaces";

export class LoadSettings {
	public static readonly type = '[Sonarr] LoadSettings';
}

export class UpdateSettings {
	public static readonly type = '[Sonarr] UpdateSettings';
	constructor(public settings: ISonarrSettings) { }
}