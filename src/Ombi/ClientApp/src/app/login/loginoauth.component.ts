import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { AuthService } from "../auth/auth.service";
import { NotificationService } from "../services";
import { StorageService } from "../shared/storage/storage-service";

@Component({
    templateUrl: "./loginoauth.component.html",
})
export class LoginOAuthComponent implements OnInit {
    public pin: number;
    public error: string;

    constructor(private authService: AuthService, private router: Router,
                private route: ActivatedRoute, private notify: NotificationService,
                private store: StorageService) {
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
          if (x.access_token) {
            this.store.save("id_token", x.access_token);

            if (this.authService.loggedIn()) {
                this.router.navigate(["search"]);
                return;
            }
        }
          if (x.errorMessage) {
            this.error = x.errorMessage;
        }

        }, err => {
            this.notify.error(err.statusText);
            this.router.navigate(["login"]);
        });
    }
}
