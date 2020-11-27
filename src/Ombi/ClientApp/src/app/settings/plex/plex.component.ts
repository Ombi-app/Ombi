import { Component, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";

import { IPlexLibrariesSettings, IPlexServer, IPlexServerResponse, IPlexServerViewModel, IPlexSettings } from "../../interfaces";
import { JobService, NotificationService, PlexService, SettingsService, TesterService } from "../../services";
import { MatTabChangeEvent, MatTabGroup } from "@angular/material/tabs";
import {FormControl} from '@angular/forms';

@Component({
    templateUrl: "./plex.component.html",
    styleUrls: ["./plex.component.scss"]
})
export class PlexComponent implements OnInit, OnDestroy {
    public settings: IPlexSettings;
    public loadedServers: IPlexServerViewModel; // This comes from the api call for the user to select a server
    public username: string;
    public password: string;
    public serversButton = false;
    selected = new FormControl(0);
    @ViewChild("tabGroup", {static: false}) public tagGroup: MatTabGroup;

    public advanced = false;

    private subscriptions = new Subject<void>();

    constructor(
        private settingsService: SettingsService,
        private notificationService: NotificationService,
        private plexService: PlexService,
        private testerService: TesterService,
        private jobService: JobService) { }

    public ngOnInit() {
        this.settingsService.getPlex().subscribe(x => {
            this.settings = x;
        });
    }

    public requestServers(server: IPlexServer) {
        this.plexService.getServers(this.username, this.password).pipe(
            takeUntil(this.subscriptions),
        ).subscribe(x => {
            if (x.success) {
                this.loadedServers = x;
                this.serversButton = true;
                this.notificationService.success("Found the servers! Please select one!");
            } else {
                this.notificationService.warning("Error When Requesting Plex Servers", "Please make sure your username and password are correct");
            }
        });
    }

    public selectServer(selectedServer: IPlexServerResponse, server: IPlexServer) {
        server.ip = selectedServer.localAddresses.split(",")[0];
        server.name = selectedServer.name;
        server.machineIdentifier = selectedServer.machineIdentifier;
        server.plexAuthToken = selectedServer.accessToken;
        server.port = parseInt(selectedServer.port);
        server.ssl = selectedServer.scheme === "http" ? false : true;

        this.notificationService.success(`Selected ${server.name}!`);
    }

    public testPlex(server: IPlexServer) {
        this.testerService.plexTest(server).subscribe(x => {
            if (x === true) {
                this.notificationService.success(`Successfully connected to the Plex server ${server.name}!`);
            } else {
                this.notificationService.error(`We could not connect to the Plex server  ${server.name}!`);
            }
        });
    }

    public addTab(event: MatTabChangeEvent) {
        
        const tabName = event.tab.textLabel;
        if (tabName == "Add Server"){ 
            
            if (this.settings.servers == null) {
            this.settings.servers = [];
            }
            this.settings.servers.push(<IPlexServer> { name: "New" + this.settings.servers.length + "*", id: Math.floor(Math.random() * (99999 - 0 + 1) + 1) });

            //this.tagGroup.selectedIndex = (0);
            this.selected.setValue(this.settings.servers.length - 1);
            }
    }

    public removeServer(server: IPlexServer) {
        const index = this.settings.servers.indexOf(server, 0);
        if (index > -1) {
            this.settings.servers.splice(index, 1);
            this.selected.setValue(this.settings.servers.length - 1);
        }
    }

    public loadLibraries(server: IPlexServer) {
        if (server.ip == null) {
            this.notificationService.error("Plex is not yet configured correctly");
            return;
        }
        this.plexService.getLibraries(server).subscribe(x => {
            server.plexSelectedLibraries = [];
            if (x.successful) {
                x.data.mediaContainer.directory.forEach((item) => {
                    const lib: IPlexLibrariesSettings = {
                        key: item.key,
                        title: item.title,
                        enabled: false,
                    };
                    server.plexSelectedLibraries.push(lib);
                });
            } else {
                this.notificationService.error(x.message);
            }
        },
            err => { this.notificationService.error(err); });
    }

    public save() {
        const filtered = this.settings.servers.filter(x => x.name !== "");
        this.settings.servers = filtered;
        this.settingsService.savePlex(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Plex settings");
            } else {
                this.notificationService.success("There was an error when saving the Plex settings");
            }
        });
    }

    public runCacher(): void {
        this.jobService.runPlexCacher().subscribe(x => {
            if (x) {
                this.notificationService.success("Triggered the Plex Full Sync");
            }
        });
    }

    public runRecentlyAddedCacher(): void {
        this.jobService.runPlexRecentlyAddedCacher().subscribe(x => {
            if (x) {
                this.notificationService.success("Triggered the Plex Recently Added Sync");
            }
        });
    }

    public ngOnDestroy() {
        this.subscriptions.next();
        this.subscriptions.complete();
    }
}
