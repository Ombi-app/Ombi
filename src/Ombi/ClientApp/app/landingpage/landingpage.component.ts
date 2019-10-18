import { PlatformLocation } from "@angular/common";
import { Component, OnDestroy, OnInit } from "@angular/core";

import { IMediaServerStatus } from "../interfaces";
import { ICustomizationSettings, ILandingPageSettings } from "../interfaces";
import { LandingPageService } from "../services";
import { SettingsService } from "../services";

import { DomSanitizer } from "@angular/platform-browser";
import { ImageService } from "../services";

import { fadeInOutAnimation } from "../animations/fadeinout";

@Component({
    templateUrl: "./landingpage.component.html",
    animations: [fadeInOutAnimation],
    styleUrls: ["./landingpage.component.scss"],
})
export class LandingPageComponent implements OnDestroy, OnInit {

    public customizationSettings: ICustomizationSettings;
    public landingPageSettings: ILandingPageSettings;
    public background: any;
    public mediaServerStatus: IMediaServerStatus;
    public baseUrl: string;
    private timer: any;

    constructor(private settingsService: SettingsService,
                private images: ImageService, private sanitizer: DomSanitizer, private landingPageService: LandingPageService,
                private location: PlatformLocation) { }

    public ngOnInit() {
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
        this.settingsService.getLandingPage().subscribe(x => this.landingPageSettings = x);
        this.images.getRandomBackground().subscribe(x => {
            this.background = this.sanitizer.bypassSecurityTrustStyle("linear-gradient(-10deg, transparent 20%, rgba(0,0,0,0.7) 20.0%, rgba(0,0,0,0.7) 80.0%, transparent 80%), url(" + x.url + ")");
        });
        this.timer = setInterval(() => {
            this.cycleBackground();
        }, 15000);

        const base = this.location.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.baseUrl = base;
        }

        this.landingPageService.getServerStatus().subscribe(x => {
            this.mediaServerStatus = x;
        });
    }

    public ngOnDestroy() {
        clearInterval(this.timer);
    }

    public cycleBackground() {
            this.images.getRandomBackground().subscribe(x => {
                this.background = "";
            });
            this.images.getRandomBackground().subscribe(x => {
                this.background = this.sanitizer
                    .bypassSecurityTrustStyle("linear-gradient(-10deg, transparent 20%, rgba(0,0,0,0.7) 20.0%, rgba(0,0,0,0.7) 80.0%, transparent 80%), url(" + x.url + ")");
            });
    }
}
