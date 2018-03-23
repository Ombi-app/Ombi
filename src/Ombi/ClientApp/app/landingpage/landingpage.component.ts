import { PlatformLocation } from "@angular/common";
import { AfterViewInit, Component, OnDestroy, OnInit } from "@angular/core";

import { IMediaServerStatus } from "../interfaces";
import { ICustomizationSettings, ILandingPageSettings } from "../interfaces";
import { LandingPageService } from "../services";
import { SettingsService } from "../services";

import { DomSanitizer } from "@angular/platform-browser";
import { ImageService } from "../services";

import { setTimeout } from "core-js/library/web/timers";
import { fadeInOutAnimation } from "../animations/fadeinout";

@Component({
    templateUrl: "./landingpage.component.html",
    animations: [fadeInOutAnimation],
    styleUrls: ["./landingpage.component.scss"],
})
export class LandingPageComponent implements AfterViewInit, OnInit, OnDestroy {

    public customizationSettings: ICustomizationSettings;
    public landingPageSettings: ILandingPageSettings;
    public background: any;
    public mediaServerStatus: IMediaServerStatus;
    public baseUrl: string;

    constructor(private settingsService: SettingsService,
                private images: ImageService, private sanitizer: DomSanitizer, private landingPageService: LandingPageService,
                private location: PlatformLocation) { }

    public ngOnInit() {
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
        this.settingsService.getLandingPage().subscribe(x => this.landingPageSettings = x);
        this.images.getRandomBackground().subscribe(x => {
            this.background = this.sanitizer.bypassSecurityTrustStyle("linear-gradient(-10deg, transparent 20%, rgba(0,0,0,0.7) 20.0%, rgba(0,0,0,0.7) 80.0%, transparent 80%), url(" + x.url + ")");
        });

        const base = this.location.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.baseUrl = base;
        }

        this.landingPageService.getServerStatus().subscribe(x => {
            this.mediaServerStatus = x;
        });
    }

    public ngOnDestroy() {
        setTimeout(() => {
            this.images.getRandomBackground().subscribe(x => {
                this.background = "";
            });
        }, 1000);
        setTimeout(() => {
            this.images.getRandomBackground().subscribe(x => {
                this.background = this.sanitizer
                    .bypassSecurityTrustStyle("linear-gradient(-10deg, transparent 20%, rgba(0,0,0,0.7) 20.0%, rgba(0,0,0,0.7) 80.0%, transparent 80%), url(" + x.url + ")");
            });
        }, 1000);
    }

    public ngAfterViewInit() {
        setInterval(() => {
            this.ngOnDestroy();
        }, 10000);
    }
}
