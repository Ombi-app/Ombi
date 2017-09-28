import { Injectable } from "@angular/core";
import { Http } from "@angular/http";
import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { ServiceAuthHelpers } from "../service.helpers";

import { IEmbySettings, IUsersModel } from "../../interfaces";

@Injectable()
export class EmbyService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, private regularHttp: Http) {
        super(http, "/api/v1/Emby/");
    }

    public logIn(settings: IEmbySettings): Observable<IEmbySettings> {
        return this.regularHttp.post(`${this.url}`, JSON.stringify(settings), { headers: this.headers }).map(this.extractData);
    }
    public getUsers(): Observable<IUsersModel[]> {
        return this.http.get(`${this.url}users`, { headers: this.headers }).map(this.extractData);
    }

}
