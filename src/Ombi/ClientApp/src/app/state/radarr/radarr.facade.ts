import { IRadarrCombined } from "../../interfaces";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Store } from "@ngxs/store";
import { RadarrState } from "./types";
import { RadarrSelectors } from "./radarr.selectors";
import { LoadSettings, UpdateSettings } from "./radarr.actions";

@Injectable({
	providedIn: 'root',
})
export class RadarrFacade {

	public constructor(private store: Store) {}

	public state$ = (): Observable<RadarrState> => this.store.select(RadarrSelectors.state);

    public updateSettings = (settings: IRadarrCombined): Observable<unknown> => this.store.dispatch(new UpdateSettings(settings));

    public load = (): Observable<unknown> => this.store.dispatch(new LoadSettings());

    public settings = (): IRadarrCombined => this.store.selectSnapshot(RadarrSelectors.settings);

    public isEnabled = (): boolean => this.store.selectSnapshot(RadarrSelectors.isEnabled);

    public is4KEnabled = (): boolean => this.store.selectSnapshot(RadarrSelectors.is4KEnabled);
}