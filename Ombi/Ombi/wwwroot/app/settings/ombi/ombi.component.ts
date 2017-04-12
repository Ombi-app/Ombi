import { Component, OnInit } from '@angular/core';

import { IOmbiSettings } from '../interfaces/ISettings'
import { SettingsService } from '../../services/settings.service';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './ombi.component.html',
})
export class OmbiComponent implements OnInit {

    constructor(private settingsService: SettingsService) {  }

    settings: IOmbiSettings;

    ngOnInit(): void {
        this.settingsService.getOmbi().subscribe(x => this.settings = x);
    }


    refreshApiKey() {
        
    }
}