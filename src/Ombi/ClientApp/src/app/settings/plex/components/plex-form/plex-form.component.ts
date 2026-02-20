import { Component, EventEmitter, Input, Output } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { IPlexServer, IPlexDeviceResponse, IPlexServerViewModel } from "app/interfaces";
import { PlexCreds, PlexSyncType } from "../models";
import { PlexFormFieldComponent } from "../form-field/plex-form-field.component";

@Component({
        standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        MatButtonModule,
        MatFormFieldModule,
        MatSelectModule,
        MatSlideToggleModule,
        PlexFormFieldComponent,
    ],
    templateUrl: "./plex-form.component.html",
    styleUrls: ["./plex-form.component.scss"],
    selector: "settings-plex-form"
})
export class PlexFormComponent {

    @Input() public server: IPlexServer;
    @Input() public advancedEnabled: boolean = false;
    @Input() public loadedServers: IPlexServerViewModel;

    @Output() public loadLibraries = new EventEmitter();
    @Output() public loadServers = new EventEmitter<PlexCreds>();
    @Output() public selectServer = new EventEmitter<IPlexDeviceResponse>();
    @Output() public test = new EventEmitter();
    @Output() public runSync = new EventEmitter<PlexSyncType>();

    public username: string;
    public password: string;
    public PlexSyncType = PlexSyncType;
}
