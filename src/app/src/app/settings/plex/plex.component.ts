import { Component, OnDestroy, OnInit } from "@angular/core";
import { EMPTY, Subject } from "rxjs";
import { catchError, takeUntil } from "rxjs/operators";

import { IPlexServer, IPlexServerResponse, IPlexServerViewModel, IPlexSettings } from "../../interfaces";
import { JobService, NotificationService, PlexService, SettingsService } from "../../services";
import {UntypedFormControl} from '@angular/forms';
import { MatDialog } from "@angular/material/dialog";
import { PlexWatchlistComponent } from "./components/watchlist/plex-watchlist.component";
import { PlexServerDialogComponent } from "./components/plex-server-dialog/plex-server-dialog.component";
import { PlexServerDialogData, PlexSyncType } from "./components/models";

@Component({
    templateUrl: "./plex.component.html",
    styleUrls: ["./plex.component.scss"]
})
export class PlexComponent implements OnInit, OnDestroy {
    public settings: IPlexSettings;
    public loadedServers: IPlexServerViewModel; // This comes from the api call for the user to select a server
    public serversButton = false;
    selected = new UntypedFormControl(0);

    public username: string;
    public password: string;

    private subscriptions = new Subject<void>();
    public PlexSyncType = PlexSyncType;

    constructor(
        private settingsService: SettingsService,
        private notificationService: NotificationService,
        private plexService: PlexService,
        private jobService: JobService,
        private dialog: MatDialog) { }

    public ngOnInit() {
        this.settingsService.getPlex().subscribe(x => {
            this.settings = x;

            if (!this.settings.servers) {
                this.settings.servers = [];
            }
        });
    }

    public requestServers() {
        this.plexService.getServers(this.username, this.password).pipe(
            takeUntil(this.subscriptions),
            catchError(() => {
                this.notificationService.error("There was an issue. Please make sure your username and password are correct");
                return EMPTY;
            })
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

    public selectServer(selectedServer: IPlexServerResponse) {
        const server = <IPlexServer> { name: "New" + this.settings.servers.length + "*", id: Math.floor(Math.random() * (99999 - 0 + 1) + 1) };

        var splitServers = selectedServer.localAddresses.split(",");
        if (splitServers.length > 1) {
            server.ip = splitServers[splitServers.length - 1];
        } else {
            server.ip = selectedServer.localAddresses;
        }
        server.name = selectedServer.name;
        server.machineIdentifier = selectedServer.machineIdentifier;
        server.plexAuthToken = selectedServer.accessToken;
        server.port = parseInt(selectedServer.port);
        server.ssl = selectedServer.scheme === "http" ? false : true;
        server.serverHostname = "";

        this.notificationService.success(`Selected ${server.name}!`);
        this.newServer(server);
    }

    public save() {
        const filtered = this.settings.servers.filter(x => x.name !== "");
        this.settings.servers = filtered;
        let invalid = false;

        this.settings.servers.forEach(server => {
            if (server.serverHostname && server.serverHostname.length > 0 && !server.serverHostname.startsWith("http")) {
                invalid = true;
            }
        });

        if (invalid) {
            this.notificationService.error("Please ensure that your External Hostname is a full URL including the Scheme (http/https)")
            return;
        }

        this.settingsService.savePlex(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Plex settings");
            } else {
                this.notificationService.success("There was an error when saving the Plex settings");
            }
        });
    }

    public runSync(type: PlexSyncType) {
        switch (type) {
            case PlexSyncType.Full:
                this.runCacher();
                return;
            case PlexSyncType.RecentlyAdded:
                this.runRecentlyAddedCacher();
                return;
            case PlexSyncType.ClearAndReSync:
                this.clearDataAndResync();
                return;
            case PlexSyncType.WatchlistImport:
                this.runWatchlistImport();
                return;
        }
    }

    public edit(server: IPlexServer) {
        const data: PlexServerDialogData = {
            server: server,
          };
          const dialog = this.dialog.open(PlexServerDialogComponent, {
            width: "700px",
            data: data,
            panelClass: "modal-panel",
          });
          dialog.afterClosed().subscribe((x) => {
            if (x.deleted) {
                this.removeServer(server);
            }
            if (x.server) {
                var idx = this.settings.servers.findIndex(server => server.id === x.server.id);
                if (idx >= 0) {
                    this.settings.servers[idx] = x.server;
                } else {
                    this.settings.servers.push(x.server);
                }

                this.save();
            }
          });
    }

    public newServer(server: IPlexServer) {
        if(!server) {
            server = <IPlexServer> { name: "New" + this.settings.servers.length + "*", id: Math.floor(Math.random() * (99999 - 0 + 1) + 1) };
        }
       const dialog = this.dialog.open(PlexServerDialogComponent, {
            width: "700px",
            data: {server: server},
            panelClass: "modal-panel",
          });
          dialog.afterClosed().subscribe((x) => {
            if (x.closed) {
                return;
            }
            if (x.server) {
                this.settings.servers.push(x.server);
                this.save();
            }
          });
    }

    private removeServer(server: IPlexServer) {
        const index = this.settings.servers.indexOf(server, 0);
        if (index > -1) {
            this.settings.servers.splice(index, 1);
            this.selected.setValue(this.settings.servers.length - 1);
        }
        this.save();
    }

    private runCacher(): void {
        this.jobService.runPlexCacher().subscribe(x => {
            if (x) {
                this.notificationService.success("Triggered the Plex Full Sync");
            }
        });
    }

    private runRecentlyAddedCacher(): void {
        this.jobService.runPlexRecentlyAddedCacher().subscribe(x => {
            if (x) {
                this.notificationService.success("Triggered the Plex Recently Added Sync");
            }
        });
    }

    private clearDataAndResync(): void {
        this.jobService.clearMediaserverData().subscribe(x => {
            if (x) {
                this.notificationService.success("Triggered the Clear MediaServer Resync");
            }
        });
    }

    private runWatchlistImport(): void {
        this.jobService.runPlexWatchlistImport().subscribe(x => {
            if (x) {
                this.notificationService.success("Triggered the Watchlist Import");
            }
        });
    }

    public openWatchlistUserLog(): void {
        this.dialog.open(PlexWatchlistComponent, { width: "700px", panelClass: 'modal-panel' });
    }

    public ngOnDestroy() {
        this.subscriptions.next();
        this.subscriptions.complete();
    }
}
