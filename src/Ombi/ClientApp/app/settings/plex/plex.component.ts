import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import "rxjs/add/operator/takeUntil";

import { IPlexSettings, IPlexLibraries, IPlexServer } from '../../interfaces/ISettings';
import { IPlexServerResponse, IPlexServerViewModel } from '../../interfaces/IPlex'

import { SettingsService } from '../../services/settings.service';
import { PlexService } from '../../services/applications/plex.service';
import { NotificationService } from "../../services/notification.service";

@Component({
    templateUrl: './plex.component.html',
})
export class PlexComponent implements OnInit, OnDestroy {
    constructor(private settingsService: SettingsService, private notificationService: NotificationService, private plexService: PlexService) { }

    settings: IPlexSettings;
    loadedServers: IPlexServerViewModel; // This comes from the api call for the user to select a server
    private subscriptions = new Subject<void>();
    username: string;
    password: string;
    advanced = false;
    serversButton = false;

    ngOnInit(): void {
        this.settingsService.getPlex().subscribe(x => {
            this.settings = x;
        }
        );
    }

    requestServers(server: IPlexServer): void {
        this.plexService.getServers(this.username, this.password)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                if (x.success) {
                    this.loadedServers = x;
                    this.serversButton = true;
                    this.notificationService.success("Loaded", "Found the servers! Please select one!")
                } else {
                    this.notificationService.warning("Error When Requesting Plex Servers", x.message);
                }
            });
    }

    selectServer(selectedServer: IPlexServerResponse, server: IPlexServer) {
        server.ip = selectedServer.localAddresses.split(',')[0];
        server.name = selectedServer.name;
        server.machineIdentifier = selectedServer.machineIdentifier;
        server.plexAuthToken = selectedServer.accessToken;
        server.port = parseInt(selectedServer.port);
        server.ssl = selectedServer.scheme === "http" ? false : true;

        this.notificationService.success("Success", `Selected ${server.name}!`)
    }

    testPlex() {
        // TODO Plex Service
    }

    addTab() {
        if (this.settings.servers == null) {
            this.settings.servers = [];
            this.settings.servers.push(<IPlexServer>{ name: "New*", id: Math.floor(Math.random() * (99999 - 0 + 1) + 1) });
        } else {
            this.notificationService.warning("Disabled", "Support for multiple servers is not available yet");
        }
    }

    removeServer(server: IPlexServer) {
        this.notificationService.warning("Disabled", "This feature is currently disabled");
        //var index = this.settings.servers.indexOf(server, 0);
        //if (index > -1) {
        //    this.settings.servers.splice(index, 1);
        //}
    }

    loadLibraries(server: IPlexServer) {
        if (server.ip == null) {
            this.notificationService.error("Not Configured", "Plex is not yet configured correctly")
            return;
        }
        this.plexService.getLibraries(server).subscribe(x => {
            server.plexSelectedLibraries = [];
            x.mediaContainer.directory.forEach((item, index) => {
                var lib: IPlexLibraries = {
                    key: item.key,
                    title: item.title,
                    enabled: false
                };
                server.plexSelectedLibraries.push(lib);
            });
        });
    }

    save() {
        var filtered = this.settings.servers.filter(x => x.name !== "");
        this.settings.servers = filtered;
        this.settingsService.savePlex(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Plex settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Plex settings");
            }
        });
    }

    ngOnDestroy(): void {
        this.subscriptions.next();
        this.subscriptions.complete();
    }
}