import { Component, OnInit } from "@angular/core";

import { ITheMovieDbSettings } from "../../interfaces";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./themoviedb.component.html",
})
export class TheMovieDbComponent implements OnInit {

    public settings: ITheMovieDbSettings;
    public advanced: boolean;

    constructor(private settingsService: SettingsService, private notificationService: NotificationService) { }

    public ngOnInit() {
        this.settingsService.getTheMovieDbSettings().subscribe(x => {
            this.settings = x;
        });
    }

    public save() {
        this.settingsService.saveTheMovieDbSettings(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved The Movie Database settings");
            } else {
                this.notificationService.success("There was an error when saving The Movie Database settings");
            }
        });
    }
}
