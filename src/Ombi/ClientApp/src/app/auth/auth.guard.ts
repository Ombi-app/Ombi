
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";

import { AuthService } from "./auth.service";
import { StorageService } from "../shared/storage/storage-service";

@Injectable()
export class AuthGuard  {

    constructor(private auth: AuthService, private router: Router,
                private store: StorageService) { }

    public canActivate() {
        if (this.auth.loggedIn()) {
            return true;
        } else {
            this.store.remove("token");
            this.router.navigate(["login"]);
            return false;
        }
    }
}
