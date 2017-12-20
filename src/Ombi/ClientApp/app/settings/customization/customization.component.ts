﻿import { Component, OnInit } from "@angular/core";

import { ICustomizationSettings, IThemes } from "../../interfaces";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./customization.component.html",
})
export class CustomizationComponent implements OnInit {

    public settings: ICustomizationSettings;
    public themes: IThemes[];

    constructor(private settingsService: SettingsService, private notificationService: NotificationService) {  }

    public ngOnInit() {
        this.settingsService.getCustomization().subscribe(x => {
            this.settings = x;
            this.settingsService.getThemes().subscribe(t => {
                this.themes = t;

                const existingTheme = this.themes.filter((item) => {
                    return item.fullName === this.settings.presetThemeName;
                })[0];

                if(existingTheme) {
                    const index = this.themes.indexOf(existingTheme, 0);
                    if (index > -1) {
                        this.themes.splice(index, 1);
                    }
                }
                if(x.hasPresetTheme) {
                    this.themes.unshift({displayName: x.presetThemeDisplayName, fullName: x.presetThemeName, url: existingTheme.url, version: x.presetThemeVersion});
                    this.themes.unshift({displayName: "None", fullName: "None", url: "", version: ""});
                } else {
                    this.themes.unshift({displayName: "Please Select", fullName: "-1", url: "-1", version: ""});
                }
            });
        });

    }

    public save() {
        this.settingsService.saveCustomization(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Ombi settings");
            } else {
                this.notificationService.success("There was an error when saving the Ombi settings");
            }
        });
    }

    public dropDownChange(event: any): void {
        const selectedThemeFullName = <string>event.target.value;
        const selectedTheme = this.themes.filter((val) => {
            return val.fullName === selectedThemeFullName;
        })[0];

        if(selectedTheme.fullName === this.settings.presetThemeName) {
            return;
        }
        
        if(selectedTheme.fullName === "None" || selectedTheme.fullName === "-1") {
            this.settings.presetThemeName = "";
            this.settings.presetThemeContent = "";
            return;
        }

        this.settings.presetThemeName = selectedThemeFullName;
        this.settingsService.getThemeContent(selectedTheme.url).subscribe(x => {
            this.settings.presetThemeContent = x;
        });
    }
}
