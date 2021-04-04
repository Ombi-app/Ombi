import { Component, OnInit } from "@angular/core";
import { IAbout, IUpdateModel } from "../../interfaces/ISettings";
import { SettingsService, HubService, SystemService } from "../../services";
import { IConnectedUser } from "../../interfaces";
import { UpdateService } from "../../services/update.service";
import { MatDialog } from "@angular/material/dialog";
import { UpdateDialogComponent } from "./update-dialog.component";

@Component({
    templateUrl: "./about.component.html",
    styleUrls: ["./about.component.scss"]
})
export class AboutComponent implements OnInit {

    public about: IAbout;
    public newUpdate: boolean;
    public connectedUsers: IConnectedUser[];
    public newsHtml: string;

    private update: IUpdateModel;

    constructor(private readonly settingsService: SettingsService,
        private readonly jobService: UpdateService,
        private readonly hubService: HubService,
        private readonly systemService: SystemService,
        private readonly dialog: MatDialog) { }

    public async ngOnInit() {
        this.settingsService.about().subscribe(x => this.about = x);
        this.newsHtml = await this.systemService.getNews().toPromise();

        this.jobService.checkForUpdate().subscribe(x => {
            this.update = x;
            if (x.updateAvailable) {
                this.newUpdate = true;
            }
        });

        this.connectedUsers = await this.hubService.getConnectedUsers();
    }

    public openUpdate() {
        this.dialog.open(UpdateDialogComponent, { width: "700px", data: this.update, panelClass: 'modal-panel' });
    }
}
