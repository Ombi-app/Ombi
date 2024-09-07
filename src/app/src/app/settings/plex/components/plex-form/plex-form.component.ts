import { Component, EventEmitter, Input, Output } from "@angular/core";
import { IPlexServer, IPlexServerResponse, IPlexServerViewModel } from "app/interfaces";
import { PlexCreds, PlexSyncType } from "../models";

@Component({
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
    @Output() public selectServer = new EventEmitter<IPlexServerResponse>();
    @Output() public test = new EventEmitter();
    @Output() public runSync = new EventEmitter<PlexSyncType>();

    public username: string;
    public password: string;
    public PlexSyncType = PlexSyncType;
}
