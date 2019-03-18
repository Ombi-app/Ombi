import { Component, ViewEncapsulation, Input, OnInit } from "@angular/core";
import { IChildRequests } from "../../../../../interfaces";

@Component({
    templateUrl: "./tv-requests-panel.component.html",
    styleUrls: ["./tv-requests-panel.component.scss"],
    selector: "tv-requests-panel"
})
export class TvRequestsPanelComponent implements OnInit {
    @Input() public tvRequest: IChildRequests[];
    
    public displayedColumns: string[] = ['number', 'title', 'airDate', 'status'];
    public ngOnInit(): void {
        //
    }
}
