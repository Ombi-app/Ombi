import { IFeatureEnablement } from "../../interfaces";

export class LoadFeatures {
	public static readonly type = '[Features] LoadAll';
}
export class UpdateFeature {
	public static readonly type = '[Features] Update';

	constructor(public feature: IFeatureEnablement) { }
}