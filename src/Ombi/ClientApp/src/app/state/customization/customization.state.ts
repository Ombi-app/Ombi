import { Action, State, StateContext } from "@ngxs/store";
import { LoadSettings, UpdateSettings } from "./customization.actions";

import { CUSTOMIZATION_STATE_TOKEN } from "./types";
import { ICustomizationSettings } from "../../interfaces";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { SettingsService } from "../../services";
import { tap } from "rxjs/operators";

@State({
    name: CUSTOMIZATION_STATE_TOKEN
})
@Injectable()
export class CustomizationState {
    constructor(private settingsService: SettingsService) { }

    @Action(LoadSettings)
    public load({ setState }: StateContext<ICustomizationSettings>): Observable<ICustomizationSettings> {
        return this.settingsService.getCustomization().pipe(
            tap(settings =>
                setState(settings)
                )
        );
    }

    @Action(UpdateSettings)
    public update({ setState }: StateContext<ICustomizationSettings>, { settings }: UpdateSettings): Observable<boolean> {
        return this.settingsService.saveCustomization(settings).pipe(
            tap(() => setState(settings))
        );
    }
}