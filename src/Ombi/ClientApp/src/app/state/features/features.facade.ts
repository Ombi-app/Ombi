import { LoadFeatures, UpdateFeature } from "./features.actions";

import { FeaturesSelectors } from "./features.selectors";
import { IFeatureEnablement } from "../../interfaces";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Store } from "@ngxs/store";

@Injectable({
	providedIn: 'root',
})
export class FeaturesFacade {

	public constructor(private store: Store) {}

	public features$ = (): Observable<IFeatureEnablement[]> => this.store.select(FeaturesSelectors.features);

    public update = (feature: IFeatureEnablement): Observable<unknown> => this.store.dispatch(new UpdateFeature(feature));

    public loadFeatures = (): Observable<unknown> => this.store.dispatch(new LoadFeatures());

    public is4kEnabled = (): boolean => this.store.selectSnapshot(FeaturesSelectors.is4kEnabled);

}