import { Component } from "@angular/core";
import { MatBottomSheet } from "@angular/material";
import { RequestOptionsComponent } from "./options/request-options.component";

@Component({
    templateUrl: "./requests-list.component.html",
    styleUrls: ["./requests-list.component.scss"]
})
export class RequestsListComponent {

    constructor(private bottomSheet: MatBottomSheet) { }
    
    public onOpenOptions(event: {request: any, filter: any}) {
        const ref = this.bottomSheet.open(RequestOptionsComponent, { data: { id: event.request.id, type: event.request.requestType } });

        ref.afterDismissed().subscribe((result) => {
            if (!result) {
                return;
            }
            event.filter();
        });
    }
}
