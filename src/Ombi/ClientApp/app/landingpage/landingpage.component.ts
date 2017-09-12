import { Component, OnInit } from '@angular/core';
import { SettingsService } from '../services/settings.service';
import { LandingPageService } from '../services/landingpage.service';
import { ILandingPageSettings, ICustomizationSettings } from '../interfaces/ISettings';
import { IMediaServerStatus } from '../interfaces/IMediaServerStatus';

import { DomSanitizer } from '@angular/platform-browser';
import { ImageService } from '../services/image.service';

@Component({
  
    templateUrl: './landingpage.component.html',
    styleUrls: ['./landingpage.component.scss']
})
export class LandingPageComponent implements OnInit {

    constructor(private settingsService: SettingsService,
        private images: ImageService, private sanitizer: DomSanitizer, private landingPageService: LandingPageService) { }

    customizationSettings : ICustomizationSettings;
    landingPageSettings: ILandingPageSettings;
    background: any;

    mediaServerStatus: IMediaServerStatus;

    ngOnInit(): void {
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
        this.settingsService.getLandingPage().subscribe(x => this.landingPageSettings = x);
        this.images.getRandomBackground().subscribe(x => {
            this.background = this.sanitizer.bypassSecurityTrustStyle('linear-gradient(-10deg, transparent 20%, rgba(0,0,0,0.7) 20.0%, rgba(0,0,0,0.7) 80.0%, transparent 80%), url(' + x.url + ')');
            
        });

        this.landingPageService.getServerStatus().subscribe(x => this.mediaServerStatus = x);
    }
}