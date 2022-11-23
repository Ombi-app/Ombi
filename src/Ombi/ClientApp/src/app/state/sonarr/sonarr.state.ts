import { Action, State, StateContext } from "@ngxs/store";

import { SonarrState, SONARR_STATE_TOKEN } from "./types";
import { SettingsService, SonarrService } from "../../services";
import { AuthService } from "../../auth/auth.service";
import { Injectable } from "@angular/core";
import { combineLatest, Observable, of } from "rxjs";
import { map, tap } from "rxjs/operators";
import { LoadSettings, UpdateSettings } from "./sonarr.actions";
import { ISonarrSettings } from "../../interfaces";

@State({
    name: SONARR_STATE_TOKEN
})
@Injectable()
export class SonarrSettingsState {
    constructor(private sonarrService: SonarrService, private settingsService: SettingsService, private authService:  AuthService) { }

    @Action(LoadSettings)
    public load({ setState }: StateContext<SonarrState>): Observable<SonarrState> {
        const isAdmin = this.authService.isAdmin();
        const calls = isAdmin ? [this.sonarrService.getVersion(), this.settingsService.getSonarr()] : [this.sonarrService.getVersion(), of({})];

        return combineLatest(calls).pipe(
            tap(([version, settings]) =>
            {
                setState({settings: settings as ISonarrSettings, version: version as string});
            }),
            map((result) => <SonarrState>{settings: result[1], version: result[0]})
        );
    }

    @Action(UpdateSettings)
    public enable(ctx: StateContext<SonarrState>, { settings }: UpdateSettings): Observable<SonarrState> {
        const state = ctx.getState();
        return this.settingsService.saveSonarr(settings).pipe(
            tap((_) => ctx.setState({...state, settings})),
            map(_ =>  <SonarrState>{...state, settings})
        );
    }
}