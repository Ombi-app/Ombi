import { Component, OnInit } from '@angular/core';
import { SettingsService } from '../services/settings.service';
import { RequestService } from '../services/request.service';
import { ILandingPageSettings, ICustomizationSettings } from '../interfaces/ISettings';
import { IRequestCountModel } from '../interfaces/IRequestModel';

import { DomSanitizer } from '@angular/platform-browser';
import { ImageService } from '../services/image.service';

@Component({
  
    templateUrl: './landingpage.component.html',
    styleUrls: ['./landingpage.component.scss']
})
export class LandingPageComponent implements OnInit {

    constructor(private settingsService: SettingsService, private requestService: RequestService,
        private images: ImageService, private sanitizer: DomSanitizer) { }

    customizationSettings : ICustomizationSettings;
    landingPageSettings: ILandingPageSettings;
    requestCount: IRequestCountModel;
    background: any;

    mediaServerStatus: boolean;

    ngOnInit(): void {
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
        this.settingsService.getLandingPage().subscribe(x => this.landingPageSettings = x);
        this.requestService.getRequestsCount().subscribe(x => this.requestCount = x);
        this.images.getRandomBackground().subscribe(x => {
            this.background = this.sanitizer.bypassSecurityTrustStyle('linear-gradient(-10deg, transparent 20%, rgba(0,0,0,0.7) 20.0%, rgba(0,0,0,0.7) 80.0%, transparent 80%), url(' + x.url + ')');
            
        });
        this.mediaServerStatus = true;
    }
}