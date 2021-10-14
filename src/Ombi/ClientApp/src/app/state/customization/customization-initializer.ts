import { APP_INITIALIZER } from "@angular/core";
import { CustomizationFacade } from ".";
import { Observable } from "rxjs";

export const CUSTOMIZATION_INITIALIZER = {
	provide: APP_INITIALIZER,
	useFactory: (customizationFacade: CustomizationFacade) => (): Observable<unknown> => customizationFacade.loadCustomziationSettings(),
	multi: true,
	deps: [CustomizationFacade],
};