import { Component, OnInit } from "@angular/core";
import { MatTableDataSource } from "@angular/material/table";
import { take } from "rxjs";
import { IPlexWatchlistUsers, WatchlistSyncStatus } from "../../../../interfaces";
import { PlexService } from "../../../../services";

@Component({
    templateUrl: "./plex-watchlist.component.html",
    styleUrls: ["./plex-watchlist.component.scss"]
})
export class PlexWatchlistComponent implements OnInit{

    public dataSource: MatTableDataSource<IPlexWatchlistUsers> = new MatTableDataSource();
    public displayedColumns: string[] = ['userName','syncStatus'];

    public WatchlistSyncStatus = WatchlistSyncStatus;

    constructor(private plexService: PlexService) { }

    public ngOnInit() {
        this.plexService.getWatchlistUsers().pipe(take(1)).subscribe((x: IPlexWatchlistUsers[]) => {
            this.dataSource = new MatTableDataSource(x);
        });
    }
}
