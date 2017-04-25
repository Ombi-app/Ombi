import { Component, OnInit } from '@angular/core';
import { SettingsService } from '../services/settings.service';
import { ILandingPageSettings } from '../interfaces/ISettings';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './landingpage.component.html',
    styleUrls: ['./landingpage.component.css']
})
export class LandingPageComponent implements OnInit {

    constructor(private settingsService: SettingsService) { }

    websiteName: string;
    landingPageSettings: ILandingPageSettings;

    ngOnInit(): void {
        this.settingsService.getLandingPage().subscribe(x => {
            this.landingPageSettings = x;
            console.log(x);
        });
        this.websiteName = "Ombi";
    }


}