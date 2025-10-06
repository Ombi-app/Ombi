import { Component, OnDestroy, OnInit } from "@angular/core";
import { MatTableDataSource, MatTableModule } from "@angular/material/table";
import { Subscription, interval, startWith, switchMap, take } from "rxjs";
import { IPlexWatchlistUsers, WatchlistSyncStatus } from "../../../../interfaces";
import { PlexService } from "../../../../services";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatCardModule } from "@angular/material/card";
import { CommonModule } from "@angular/common";
import { MatDialogModule } from "@angular/material/dialog";
import { MatProgressBarModule } from "@angular/material/progress-bar";

@Component({
        standalone: true,
    templateUrl: "./plex-watchlist.component.html",
    styleUrls: ["./plex-watchlist.component.scss"],
    imports: [
        CommonModule,
        MatIconModule,
        MatCardModule,
        MatButtonModule,
        MatDialogModule,
        MatTableModule,
        MatProgressBarModule
    ]
})
export class PlexWatchlistComponent implements OnInit, OnDestroy{

    public dataSource: MatTableDataSource<IPlexWatchlistUsers> = new MatTableDataSource();
    public displayedColumns: string[] = ['userName','syncStatus'];
    public isReloading = false;

    public WatchlistSyncStatus = WatchlistSyncStatus;

    private readonly pollIntervalMs = 1000;
    private readonly maxPollAttempts = 15;
    private pollSubscription?: Subscription;
    private lastSnapshot: string[] = [];

    constructor(private plexService: PlexService) { }

    public ngOnInit() {
        this.loadWatchlistUsers();
    }

    public ngOnDestroy(): void {
        this.stopPolling();
    }

    public forceRecheck(): void {
        if (this.isReloading) {
            return;
        }

        this.isReloading = true;
        this.lastSnapshot = this.createSnapshot(this.dataSource.data);
        this.plexService.revalidateWatchlistUsers().pipe(take(1)).subscribe({
            next: () => {
                this.beginPollingForUpdates();
            },
            error: () => {
                this.finishReload();
            }
        });
    }

    private loadWatchlistUsers(): void {
        this.plexService.getWatchlistUsers().pipe(take(1)).subscribe((x: IPlexWatchlistUsers[]) => {
            this.dataSource = new MatTableDataSource(x);
            this.lastSnapshot = this.createSnapshot(x);
        });
    }

    private beginPollingForUpdates(): void {
        this.stopPolling();

        let attempts = 0;
        this.pollSubscription = interval(this.pollIntervalMs)
            .pipe(
                startWith(0),
                switchMap(() => this.plexService.getWatchlistUsers().pipe(take(1)))
            )
            .subscribe({
                next: (users: IPlexWatchlistUsers[]) => {
                    this.dataSource = new MatTableDataSource(users);
                    const currentSnapshot = this.createSnapshot(users);
                    attempts += 1;

                    if (this.hasStateChanged(this.lastSnapshot, currentSnapshot) || attempts >= this.maxPollAttempts) {
                        this.lastSnapshot = currentSnapshot;
                        this.finishReload();
                    }
                },
                error: () => {
                    this.finishReload();
                }
            });
    }

    private finishReload(): void {
        this.stopPolling();
        this.isReloading = false;
    }

    private stopPolling(): void {
        this.pollSubscription?.unsubscribe();
        this.pollSubscription = undefined;
    }

    private createSnapshot(users: IPlexWatchlistUsers[]): string[] {
        return users
            .map(user => `${user.userId}:${user.syncStatus}`)
            .sort((a, b) => a.localeCompare(b));
    }

    private hasStateChanged(previous: string[], current: string[]): boolean {
        if (previous.length !== current.length) {
            return true;
        }

        return previous.some((value, index) => value !== current[index]);
    }
}
