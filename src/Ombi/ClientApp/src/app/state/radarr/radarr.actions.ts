import { IRadarrCombined } from "../../interfaces";

export class LoadSettings {
	public static readonly type = '[Radarr] LoadSettings';
}

export class UpdateSettings {
	public static readonly type = '[Radarr] UpdateSettings';
	constructor(public settings: IRadarrCombined) { }
}