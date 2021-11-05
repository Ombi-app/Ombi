import { LoadSettings, UpdateSettings } from "./customization.actions";

import { CustomizationSelectors } from "./customization.selectors";
import { ICustomizationSettings } from "../../interfaces";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Store } from "@ngxs/store";

@Injectable({
	providedIn: 'root',
})
export class CustomizationFacade {

	public constructor(private store: Store) {}

	public settings$ = (): Observable<ICustomizationSettings> => this.store.select(CustomizationSelectors.customizationSettings);

    public loadCustomziationSettings = (): Observable<unknown> => this.store.dispatch(new LoadSettings());

    public logo = (): string => this.store.selectSnapshot(CustomizationSelectors.logo);

    public appName = (): string => this.store.selectSnapshot(CustomizationSelectors.applicationName);

    public appUrl = (): string => this.store.selectSnapshot(CustomizationSelectors.applicationUrl);

    public saveSettings = (settings: ICustomizationSettings): Observable<unknown> => this.store.dispatch(new UpdateSettings(settings));
}