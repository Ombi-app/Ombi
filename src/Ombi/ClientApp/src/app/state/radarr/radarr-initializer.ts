import { APP_INITIALIZER } from "@angular/core";
import { Observable } from "rxjs";
import { RadarrFacade } from "./radarr.facade";

export const RADARR_INITIALIZER = {
	provide: APP_INITIALIZER,
	useFactory: (radarrFacade: RadarrFacade) => (): Observable<unknown> => {
		return radarrFacade.load();
	},
	multi: true,
	deps: [RadarrFacade],
};