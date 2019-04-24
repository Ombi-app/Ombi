import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { ServiceHelpers } from "./service.helpers";
import { ICalendarModel } from "../interfaces/ICalendar";

@Injectable()
export class CalendarService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v2/Calendar/", href);
    }
    public getCalendarEntries(): Promise<ICalendarModel[]> {
        return this.http.get<ICalendarModel[]>(`${this.url}`, {headers: this.headers}).toPromise();
    }
}
