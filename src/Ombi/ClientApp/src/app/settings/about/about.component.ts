import { Component, OnInit } from "@angular/core";
import { IAbout } from "../../interfaces/ISettings";
import { JobService, SettingsService, HubService, SystemService } from "../../services";
import { IConnectedUser } from "../../interfaces";

@Component({
    templateUrl: "./about.component.html",
    styleUrls: ["./about.component.scss"]
})
export class AboutComponent implements OnInit {

    public about: IAbout;
    public newUpdate: boolean;
    public connectedUsers: IConnectedUser[];
    public newsHtml: string;

    constructor(private readonly settingsService: SettingsService,
        private readonly jobService: JobService,
        private readonly hubService: HubService,
        private readonly systemService: SystemService) { }

    public async ngOnInit() {
        this.settingsService.about().subscribe(x => this.about = x);
        this.newsHtml = await this.systemService.getNews().toPromise();

        // TODO
        // this.jobService.getCachedUpdate().subscribe(x => {
        //     if (x === true) {
        //         // this.newUpdate = true; // TODO
        //     }
        // });

        this.connectedUsers = await this.hubService.getConnectedUsers();
    }
}
