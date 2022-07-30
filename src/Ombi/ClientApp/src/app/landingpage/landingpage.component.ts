import { APP_BASE_HREF } from "@angular/common";
import { Component, OnInit, Inject } from "@angular/core";

import { IMediaServerStatus } from "../interfaces";
import { ICustomizationSettings, ILandingPageSettings } from "../interfaces";
import { LandingPageService } from "../services";
import { SettingsService } from "../services";

import { CustomizationFacade } from "../state/customization";

@Component({
    templateUrl: "./landingpage.component.html",
    styleUrls: ["./landingpage.component.scss"],
})
export class LandingPageComponent implements OnInit {

    public customizationSettings: ICustomizationSettings;
    public landingPageSettings: ILandingPageSettings;
    public mediaServerStatus: IMediaServerStatus;
    public baseUrl: string;

    private href: string;

    constructor(private settingsService: SettingsService,
                private landingPageService: LandingPageService,
                private customizationFacade: CustomizationFacade,
                @Inject(APP_BASE_HREF) href :string) { this.href = href }

    public ngOnInit() {
        this.customizationFacade.settings$().subscribe(x => this.customizationSettings = x);
        this.settingsService.getLandingPage().subscribe(x => this.landingPageSettings = x);

        const base = this.href;
        if (base.length > 1) {
            this.baseUrl = base;
        }

        this.landingPageService.getServerStatus().subscribe(x => {
            this.mediaServerStatus = x;
        });
    }
}
