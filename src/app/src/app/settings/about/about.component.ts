import { Component, Inject, OnInit } from "@angular/core";
import { HubService, SettingsService, SystemService } from "../../services";
import { IAbout, IUpdateModel } from "../../interfaces/ISettings";

import { IConnectedUser } from "../../interfaces";
import { MatDialog } from "@angular/material/dialog";
import { UpdateDialogComponent } from "./update-dialog.component";
import { UpdateService } from "../../services/update.service";
import { APP_BASE_HREF } from "@angular/common";

@Component({
    templateUrl: "./about.component.html",
    styleUrls: ["./about.component.scss"]
})
export class AboutComponent implements OnInit {

    public about: IAbout;
    public newUpdate: boolean;
    public connectedUsers: IConnectedUser[];
    public newsHtml: string;
    public appstoreImage: string;

    public get usingSqliteDatabase() {
        if (this.about.ombiDatabaseType.toLowerCase() === 'sqlite'
        || this.about.externalDatabaseType.toLowerCase() === 'sqlite'
        || this.about.settingsDatabaseType.toLowerCase() === 'sqlite') {
            return true;
        }
        return false;
    }

    private update: IUpdateModel;

    constructor(private readonly settingsService: SettingsService,
        private readonly jobService: UpdateService,
        private readonly hubService: HubService,
        private readonly systemService: SystemService,
        private readonly dialog: MatDialog,
        @Inject(APP_BASE_HREF) private readonly href:string) { }

    public async ngOnInit() {
        this.appstoreImage = "../../../images/appstore.svg";
        const base = this.href;
        if (base) {
            this.appstoreImage = "../../.." + base + "/images/appstore.svg";
        }
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
