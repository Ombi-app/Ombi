import { IFeatureEnablement } from "../../interfaces";

export class LoadFeatures {
	public static readonly type = '[Features] LoadAll';
}
export class EnableFeature {
	public static readonly type = '[Features] Enable';

	constructor(public feature: IFeatureEnablement) { }
}
export class DisableFeature {
	public static readonly type = '[Features] Disable';

	constructor(public feature: IFeatureEnablement) { }
}