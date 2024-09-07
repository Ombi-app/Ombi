import { APP_INITIALIZER } from "@angular/core";
import { FeaturesFacade } from "./features.facade";
import { Observable } from "rxjs";

export const FEATURES_INITIALIZER = {
	provide: APP_INITIALIZER,
	useFactory: (featureFacade: FeaturesFacade) => (): Observable<unknown> => featureFacade.loadFeatures(),
	multi: true,
	deps: [FeaturesFacade],
};