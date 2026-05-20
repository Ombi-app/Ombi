import { Component, Inject, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { HubService, SettingsService } from "../../services";
import { IAbout, IUpdateModel } from "../../interfaces/ISettings";

import { IConnectedUser } from "../../interfaces";
import { MatDialog } from "@angular/material/dialog";
import { UpdateDialogComponent } from "./update-dialog.component";
import { UpdateService } from "../../services/update.service";
import { APP_BASE_HREF } from "@angular/common";
import { SettingsMenuComponent } from "../settingsmenu.component";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule
    ],
    templateUrl: "./about.component.html",
    styleUrls: ["./about.component.scss"]
})
export class AboutComponent implements OnInit {

    public about: IAbout;
    public newUpdate: boolean;
    public connectedUsers: IConnectedUser[];

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
        private readonly dialog: MatDialog,
        @Inject(APP_BASE_HREF) private readonly href:string) { }

    public async ngOnInit() {
        this.appstoreImage = "../../../images/appstore.svg";
        const base = this.href;
        if (base) {
            this.appstoreImage = "../../.." + base + "/images/appstore.svg";
        }
        this.settingsService.about().subscribe(x => this.about = x);
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
