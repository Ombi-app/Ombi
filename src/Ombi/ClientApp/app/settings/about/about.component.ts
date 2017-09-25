import { Component, OnInit } from "@angular/core";
import { IAbout } from "../../interfaces/ISettings";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./about.component.html",
})
export class AboutComponent implements OnInit {

    public about: IAbout;

    constructor(private settingsService: SettingsService) { }

    public ngOnInit() {
        this.settingsService.about().subscribe(x => this.about = x);
    }
}
