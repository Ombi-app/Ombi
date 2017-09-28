import { Component, OnInit } from "@angular/core";
import { IAbout } from "../../interfaces/ISettings";
import { JobService, SettingsService } from "../../services";

@Component({
    templateUrl: "./about.component.html",
})
export class AboutComponent implements OnInit {

    public about: IAbout;
    public newUpdate: boolean;

    constructor(private readonly settingsService: SettingsService,
                private readonly jobService: JobService) { }

    public ngOnInit() {
        this.settingsService.about().subscribe(x => this.about = x);
        this.jobService.checkForNewUpdate().subscribe(x => {
            if (x === true) {
                this.newUpdate = true;
            }
        });

    }
}
