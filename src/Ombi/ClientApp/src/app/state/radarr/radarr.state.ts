import { Action, State, StateContext } from "@ngxs/store";

import { RadarrState, RADARR_STATE_TOKEN } from "./types";
import { SettingsService } from "../../services";
import { AuthService } from "../../auth/auth.service";
import { Injectable } from "@angular/core";
import { combineLatest, Observable, of } from "rxjs";
import { map, tap } from "rxjs/operators";
import { IRadarrCombined } from "../../interfaces";
import { LoadSettings, UpdateSettings } from "./radarr.actions";

@State({
    name: RADARR_STATE_TOKEN
})
@Injectable()
export class RadarrSettingsState {
    constructor(private settingsService: SettingsService, private authService:  AuthService) { }

    @Action(LoadSettings)
    public load({ setState }: StateContext<RadarrState>): Observable<RadarrState> {
        const isAdmin = this.authService.isAdmin();
        const calls = isAdmin ? [this.settingsService.getRadarr()] : [of({})];

        return combineLatest(calls).pipe(
            tap(([settings]) =>
            {
                setState({settings: settings as IRadarrCombined});
            }),
            map((result) => <RadarrState>{settings: result[0]})
        );
    }

    @Action(UpdateSettings)
    public enable(ctx: StateContext<RadarrState>, { settings }: UpdateSettings): Observable<RadarrState> {
        const state = ctx.getState();
        return this.settingsService.saveRadarr(settings).pipe(
            tap((_) => ctx.setState({...state, settings})),
            map(_ =>  <RadarrState>{...state, settings})
        );
    }
}