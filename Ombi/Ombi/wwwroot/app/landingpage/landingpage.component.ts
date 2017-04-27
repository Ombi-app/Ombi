import { Component, OnInit } from '@angular/core';
import { SettingsService } from '../services/settings.service';
import { ILandingPageSettings, ICustomizationSettings } from '../interfaces/ISettings';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './landingpage.component.html',
    styleUrls: ['./landingpage.component.css']
})
export class LandingPageComponent implements OnInit {

    constructor(private settingsService: SettingsService) { }

    customizationSettings : ICustomizationSettings;
    landingPageSettings: ILandingPageSettings;

    ngOnInit(): void {
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
        this.settingsService.getLandingPage().subscribe(x => this.landingPageSettings = x);
    }
}