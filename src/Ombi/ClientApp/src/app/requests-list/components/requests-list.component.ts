import { Component, ViewChild } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatBottomSheetModule } from "@angular/material/bottom-sheet";
import { MatTabsModule } from "@angular/material/tabs";
import { TranslateModule } from "@ngx-translate/core";
import { MatBottomSheet } from "@angular/material/bottom-sheet";
import { RequestOptionsComponent } from "./options/request-options.component";
import { UpdateType } from "../models/UpdateType";
import { LidarrService } from "app/services";
import { take } from "rxjs";
import { MoviesGridComponent } from "./movies-grid/movies-grid.component";
import { TvGridComponent } from "./tv-grid/tv-grid.component";
import { AlbumsGridComponent } from "./albums-grid/albums-grid.component";
import { GridSpinnerComponent } from "./grid-spinner/grid-spinner.component";

@Component({
    standalone: true,
    templateUrl: "./requests-list.component.html",
    styleUrls: ["./requests-list.component.scss"],
    imports: [
        CommonModule,
        MatBottomSheetModule,
        MatTabsModule,
        TranslateModule,
        RequestOptionsComponent,
        MoviesGridComponent,
        TvGridComponent,
        AlbumsGridComponent,
        GridSpinnerComponent
    ]
})
export class RequestsListComponent {

    constructor(private bottomSheet: MatBottomSheet, private lidarrService: LidarrService) { }

    public readonly musicEnabled$ = this.lidarrService.enabled().pipe(take(1));

    public onOpenOptions(event: { request: any, filter: any, onChange: any, manageOwnRequests: boolean, isAdmin: boolean, has4kRequest: boolean, hasRegularRequest: boolean }) {
        const ref = this.bottomSheet.open(RequestOptionsComponent, { data: { id: event.request.id, type: event.request.requestType, canApprove: event.request.canApprove, manageOwnRequests: event.manageOwnRequests, isAdmin: event.isAdmin, has4kRequest: event.has4kRequest, hasRegularRequest: event.hasRegularRequest } });

        ref.afterDismissed().subscribe((result) => {
            if(!result) {
                return;
            }
            if (result.type == UpdateType.Delete) {
                event.filter();
                return;
            }
            if (result.type == UpdateType.Approve) {
                // Need to do this here, as the status is calculated on the server
                event.request.requestStatus = 'Common.ProcessingRequest';
                event.onChange();
                return;
            }
            if (result.type == UpdateType.Availability) {
                // Need to do this here, as the status is calculated on the server
                event.request.requestStatus = 'Common.Available';
                event.onChange();
                return;
            }
            if (result.type == UpdateType.Deny) {
                event.request.requestStatus = 'Common.Denied';
                event.onChange();
                return;
            }
        });
    }
}
