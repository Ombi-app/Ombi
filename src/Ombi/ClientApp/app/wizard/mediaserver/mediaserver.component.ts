import { Component } from "@angular/core";
import { Router } from "@angular/router";

@Component({
    templateUrl: "./mediaserver.component.html",
})
export class MediaServerComponent {
    constructor(private router: Router) { }

    public plex() {
        this.router.navigate(["Wizard/Plex"]);
    }

    public emby() {
        this.router.navigate(["Wizard/Emby"]);
    }

    public skip() {
        this.router.navigate(["Wizard/CreateAdmin"]);
    }
}
