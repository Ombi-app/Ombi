import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { AuthService } from "../auth/auth.service";

@Component({
    templateUrl: "./loginoauth.component.html",
})
export class LoginOAuthComponent implements OnInit {
    public pin: number;

    constructor(private authService: AuthService, private router: Router,
                private route: ActivatedRoute) {
        this.route.params
            .subscribe((params: any) => {
                this.pin = params.pin;
                
            });
    }

    public ngOnInit(): void {
        this.auth();
    }

    public auth() {
      this.authService.oAuth(this.pin).subscribe(x => {
            localStorage.setItem("id_token", x.access_token);

            if (this.authService.loggedIn()) {
                this.router.navigate(["search"]);
            } 

        });
    }
}
