import { Component, OnInit } from "@angular/core";
import { MatTableDataSource, MatTableModule } from "@angular/material/table";
import { take } from "rxjs";
import { IPlexWatchlistUsers, WatchlistSyncStatus } from "../../../../interfaces";
import { PlexService } from "../../../../services";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatCardModule } from "@angular/material/card";
import { CommonModule } from "@angular/common";
import { MatDialogModule } from "@angular/material/dialog";

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
        MatTableModule
    ]
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
