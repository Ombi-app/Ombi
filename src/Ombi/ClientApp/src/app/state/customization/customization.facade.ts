import { CustomizationSelectors } from "./customization.selectors";
import { ICustomizationSettings } from "../../interfaces";
import { Injectable } from "@angular/core";
import { LoadSettings } from "./customization.actions";
import { Observable } from "rxjs";
import { Store } from "@ngxs/store";

@Injectable({
	providedIn: 'root',
})
export class CustomizationFacade {

	public constructor(private store: Store) {}

	public settings$ = (): Observable<ICustomizationSettings> => this.store.select(CustomizationSelectors.customizationSettings);

    public loadCustomziationSettings = (): Observable<unknown> => this.store.dispatch(new LoadSettings());
}