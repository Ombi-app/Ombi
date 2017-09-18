import { Component } from "@angular/core";
import { Router } from "@angular/router";

@Component({
    templateUrl: "./welcome.component.html",
})
export class WelcomeComponent {
    constructor(private router: Router) { }

    public next() {
        this.router.navigate(["Wizard/MediaServer"]);
    }
}
