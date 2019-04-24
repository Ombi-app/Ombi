import { PlatformLocation } from "@angular/common";
import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";

@Component({
    templateUrl: "./welcome.component.html",
})
export class WelcomeComponent implements OnInit {
    
    public baseUrl: string;
    
    constructor(private router: Router, private location: PlatformLocation) { }

    public ngOnInit(): void {
        const base = this.location.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.baseUrl = base;
        }
    }

    public next() {
        this.router.navigate(["Wizard/MediaServer"]);
    }
}
