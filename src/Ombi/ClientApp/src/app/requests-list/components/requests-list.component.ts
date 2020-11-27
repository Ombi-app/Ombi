import { Component, ViewChild } from "@angular/core";
import { MatBottomSheet } from "@angular/material/bottom-sheet";
import { RequestOptionsComponent } from "./options/request-options.component";
import { UpdateType } from "../models/UpdateType";
import { MoviesGridComponent } from "./movies-grid/movies-grid.component";

@Component({
    templateUrl: "./requests-list.component.html",
    styleUrls: ["./requests-list.component.scss"]
})
export class RequestsListComponent {

    constructor(private bottomSheet: MatBottomSheet) { }

    public onOpenOptions(event: { request: any, filter: any, onChange: any }) {
        const ref = this.bottomSheet.open(RequestOptionsComponent, { data: { id: event.request.id, type: event.request.requestType, canApprove: event.request.canApprove } });

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
        });
    }
}
