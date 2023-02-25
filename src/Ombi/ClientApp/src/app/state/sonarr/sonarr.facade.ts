import { ISonarrSettings } from "../../interfaces";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Store } from "@ngxs/store";
import { SonarrState } from "./types";
import { SonarrSelectors } from "./sonarr.selectors";
import { LoadSettings, UpdateSettings } from "./sonarr.actions";

@Injectable({
	providedIn: 'root',
})
export class SonarrFacade {

	public constructor(private store: Store) {}

	public sonarrState$ = (): Observable<SonarrState> => this.store.select(SonarrSelectors.state);

    public updateSettings = (settings: ISonarrSettings): Observable<unknown> => this.store.dispatch(new UpdateSettings(settings));

    public load = (): Observable<unknown> => this.store.dispatch(new LoadSettings());

    public version = (): string => this.store.selectSnapshot(SonarrSelectors.version);

    public settings = (): ISonarrSettings => this.store.selectSnapshot(SonarrSelectors.settings);

    public isEnabled = (): boolean => this.store.selectSnapshot(SonarrSelectors.isEnabled);

}