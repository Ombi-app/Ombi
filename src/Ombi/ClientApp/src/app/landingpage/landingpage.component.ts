import { APP_BASE_HREF, CommonModule } from "@angular/common";
import { Component, OnInit, Inject } from "@angular/core";

import { IMediaServerStatus } from "../interfaces";
import { ICustomizationSettings, ILandingPageSettings } from "../interfaces";
import { LandingPageService } from "../services";
import { SettingsService } from "../services";

import { CustomizationFacade } from "../state/customization";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { WikiComponent } from "app/settings/wiki.component";
import { ImageBackgroundComponent } from "../components";
import { RouterModule } from "@angular/router";

@Component({
        standalone: true,
    templateUrl: "./landingpage.component.html",
    styleUrls: ["./landingpage.component.scss"],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        ImageBackgroundComponent,
        MatInputModule,
        RouterModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        WikiComponent
    ],
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
