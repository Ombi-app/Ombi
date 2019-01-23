import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { CookieService } from "ng2-cookies";

@Component({
    templateUrl: "cookie.component.html",
})
export class CookieComponent implements OnInit {
    constructor(private readonly cookieService: CookieService,
                private readonly router: Router) { }

    public ngOnInit() {
        const cookie = this.cookieService.getAll();
        if (cookie.Auth) {
            const jwtVal = cookie.Auth;
            localStorage.setItem("id_token", jwtVal);
            this.router.navigate(["search"]);
        } else {
            this.router.navigate(["login"]);
        }
    }
}
