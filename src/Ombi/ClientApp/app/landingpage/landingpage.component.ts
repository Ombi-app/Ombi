import { Component, OnInit } from '@angular/core';
import { SettingsService } from '../services/settings.service';
import { RequestService } from '../services/request.service';
import { ILandingPageSettings, ICustomizationSettings } from '../interfaces/ISettings';
import { IRequestCountModel } from '../interfaces/IRequestModel';

@Component({
  
    templateUrl: './landingpage.component.html',
    styleUrls: ['./landingpage.component.scss']
})
export class LandingPageComponent implements OnInit {

    constructor(private settingsService: SettingsService, private requestService : RequestService) { }

    customizationSettings : ICustomizationSettings;
    landingPageSettings: ILandingPageSettings;
    requestCount: IRequestCountModel;

    mediaServerStatus: boolean;

    ngOnInit(): void {
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
        this.settingsService.getLandingPage().subscribe(x => this.landingPageSettings = x);
        this.requestService.getRequestsCount().subscribe(x => this.requestCount = x);

        this.mediaServerStatus = true;
    }
}