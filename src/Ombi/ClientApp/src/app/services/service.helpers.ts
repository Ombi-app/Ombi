import { PlatformLocation } from "@angular/common";
import { HttpClient, HttpHeaders } from "@angular/common/http";

export class ServiceHelpers {

    protected headers = new HttpHeaders();
    constructor(protected http: HttpClient, protected url: string, protected href: string) {
        const base = href;
        this.headers = new HttpHeaders().set("Content-Type","application/json");
        if (base.length > 1) {
            this.url = base + this.url;
        }
    }
}
