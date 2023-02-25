import { APP_INITIALIZER } from "@angular/core";
import { Observable } from "rxjs";
import { SonarrFacade } from "./sonarr.facade";

export const SONARR_INITIALIZER = {
	provide: APP_INITIALIZER,
	useFactory: (sonarrFacade: SonarrFacade) => (): Observable<unknown> => {
		return sonarrFacade.load();
	},
	multi: true,
	deps: [SonarrFacade],
};