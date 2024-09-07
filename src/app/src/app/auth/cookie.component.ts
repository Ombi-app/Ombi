import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { CookieService } from "ng2-cookies";
import { StorageService } from "../shared/storage/storage-service";

@Component({
    templateUrl: "cookie.component.html",
})
export class CookieComponent implements OnInit {
    constructor(private readonly cookieService: CookieService,
                private readonly router: Router,
                private store: StorageService) { }

    public ngOnInit() {
        const cookie = this.cookieService.getAll();
        if (cookie.Auth) {
            const jwtVal = cookie.Auth;
            this.store.save("id_token", jwtVal);
            this.router.navigate(["discover"]);
        } else {
            this.router.navigate(["login"]);
        }
    }
}
