import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { FormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatDialog, MatDialogModule } from "@angular/material/dialog";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { EMPTY, Observable } from "rxjs";
import { catchError } from "rxjs/operators";

import { IPlexDeviceResponse, IPlexServer, IPlexServerViewModel, IPlexSettings } from "../../interfaces";
import { JobService, NotificationService, PlexService, SettingsService } from "../../services";
import { PlexServerDialogComponent } from "./components/plex-server-dialog/plex-server-dialog.component";
import { PlexWatchlistComponent } from "./components/watchlist/plex-watchlist.component";
import { PlexServerDialogData, PlexSyncType } from "./components/models";

@Component({
    standalone: true,
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormsModule,
        MatButtonModule,
        MatDialogModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
    ],
    templateUrl: "./plex.component.html",
    styleUrls: ["./plex.component.scss"],
})
export class PlexComponent implements OnInit {
    private readonly settingsService = inject(SettingsService);
    private readonly notificationService = inject(NotificationService);
    private readonly plexService = inject(PlexService);
    private readonly jobService = inject(JobService);
    private readonly dialog = inject(MatDialog);
    private readonly destroyRef = inject(DestroyRef);

    public readonly settings = signal<IPlexSettings | undefined>(undefined);
    public readonly loadedServers = signal<IPlexServerViewModel | undefined>(undefined);

    public username = "";
    public password = "";

    public readonly PlexSyncType = PlexSyncType;

    private nextServerId = 0;

    public ngOnInit(): void {
        this.settingsService.getPlex()
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe((x) => {
                if (!x.servers) {
                    x.servers = [];
                }
                this.nextServerId = x.servers.reduce((max, s) => Math.max(max, s.id ?? 0), 0);
                this.settings.set(x);
            });
    }

    private generateServerId(): number {
        return ++this.nextServerId;
    }

    private notifySettingsChanged(): void {
        this.settings.update((s) => (s ? { ...s } : s));
    }

    public requestServers(): void {
        this.plexService.getServers(this.username, this.password).pipe(
            takeUntilDestroyed(this.destroyRef),
            catchError(() => {
                this.notificationService.error("There was an issue. Please make sure your username and password are correct");
                return EMPTY;
            }),
        ).subscribe((x) => {
            if (x.success) {
                this.loadedServers.set(x);
                this.notificationService.success("Found the servers! Please select one!");
            } else {
                this.notificationService.warning("Error When Requesting Plex Servers", "Please make sure your username and password are correct");
            }
        });
    }

    public selectServer(selectedDevice: IPlexDeviceResponse): void {
        const settings = this.settings();
        if (!settings) {
            return;
        }

        const server = <IPlexServer>{
            name: "New" + settings.servers.length + "*",
            id: this.generateServerId(),
        };

        const splitServers = selectedDevice.localAddresses.split(",");
        server.ip = splitServers.length > 1 ? splitServers[splitServers.length - 1] : selectedDevice.localAddresses;
        server.name = selectedDevice.name;
        server.machineIdentifier = selectedDevice.machineIdentifier;
        server.plexAuthToken = selectedDevice.accessToken;
        server.port = parseInt(selectedDevice.port);
        server.ssl = selectedDevice.scheme !== "http";
        server.serverHostname = "";

        this.notificationService.success(`Selected ${server.name}!`);
        this.newServer(server);
    }

    public save(successMessage = "Successfully saved Plex settings"): void {
        const settings = this.settings();
        if (!settings) {
            return;
        }

        settings.servers = settings.servers.filter((x) => x.name !== "");
        this.notifySettingsChanged();

        const invalid = settings.servers.some(
            (server) => server.serverHostname && server.serverHostname.length > 0 && !server.serverHostname.startsWith("http"),
        );

        if (invalid) {
            this.notificationService.error("Please ensure that your External Hostname is a full URL including the Scheme (http/https)");
            return;
        }

        this.settingsService.savePlex(settings)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe((x) => {
                if (x) {
                    this.notificationService.success(successMessage);
                } else {
                    this.notificationService.error("There was an error when saving the Plex settings");
                }
            });
    }

    public runSync(type: PlexSyncType): void {
        switch (type) {
            case PlexSyncType.Full:
                this.runJob(this.jobService.runPlexCacher(), "Triggered the Plex Full Sync");
                return;
            case PlexSyncType.RecentlyAdded:
                this.runJob(this.jobService.runPlexRecentlyAddedCacher(), "Triggered the Plex Recently Added Sync");
                return;
            case PlexSyncType.ClearAndReSync:
                this.runJob(this.jobService.clearMediaserverData(), "Triggered the Clear MediaServer Resync");
                return;
            case PlexSyncType.WatchlistImport:
                this.runJob(this.jobService.runPlexWatchlistImport(), "Triggered the Watchlist Import");
                return;
        }
    }

    public edit(server: IPlexServer): void {
        const data: PlexServerDialogData = { server };
        const dialogRef = this.dialog.open(PlexServerDialogComponent, {
            width: "700px",
            data,
            panelClass: "modal-panel",
        });

        dialogRef.afterClosed()
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe((x) => {
                if (!x) {
                    return;
                }
                if (x.deleted) {
                    this.removeServer(server);
                    return;
                }
                if (x.server) {
                    const settings = this.settings();
                    if (!settings) {
                        return;
                    }
                    const idx = settings.servers.findIndex((s) => s.id === x.server.id);
                    if (idx >= 0) {
                        settings.servers[idx] = x.server;
                        this.notifySettingsChanged();
                        this.save("Server updated");
                    } else {
                        settings.servers.push(x.server);
                        this.notifySettingsChanged();
                        this.save("Server added");
                    }
                }
            });
    }

    public newServer(server?: IPlexServer): void {
        const settings = this.settings();
        if (!settings) {
            return;
        }

        const initial = server ?? <IPlexServer>{
            name: "New" + settings.servers.length + "*",
            id: this.generateServerId(),
        };

        const dialogRef = this.dialog.open(PlexServerDialogComponent, {
            width: "700px",
            data: { server: initial },
            panelClass: "modal-panel",
        });

        dialogRef.afterClosed()
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe((x) => {
                if (!x || x.closed) {
                    return;
                }
                if (x.server) {
                    const current = this.settings();
                    if (!current) {
                        return;
                    }
                    current.servers.push(x.server);
                    this.notifySettingsChanged();
                    this.save("Server added");
                }
            });
    }

    public openWatchlistUserLog(): void {
        this.dialog.open(PlexWatchlistComponent, { width: "700px", panelClass: "modal-panel" });
    }

    private removeServer(server: IPlexServer): void {
        const settings = this.settings();
        if (!settings) {
            return;
        }
        const index = settings.servers.indexOf(server, 0);
        if (index > -1) {
            settings.servers.splice(index, 1);
            this.notifySettingsChanged();
        }
        this.save("Server removed");
    }

    private runJob(job$: Observable<boolean>, message: string): void {
        job$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((x) => {
            if (x) {
                this.notificationService.success(message);
            }
        });
    }
}
