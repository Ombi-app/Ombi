import { Component, EventEmitter, Input, Output } from "@angular/core";
import { MatDialog, MatDialogModule } from "@angular/material/dialog";
import { CommonModule } from "@angular/common";
import { MatTableModule } from "@angular/material/table";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatTooltipModule } from "@angular/material/tooltip";
import { MatPaginatorModule } from "@angular/material/paginator";
import { TranslateModule } from "@ngx-translate/core";

import { IIssuesSummary, IPagenator, IssueStatus } from "../interfaces";
import { RouterLink } from "@angular/router";

@Component({
    standalone: true,
    selector: "issues-table",
    templateUrl: "issuestable.component.html",
    styleUrls: ['issuestable.component.scss'],
    imports: [
        CommonModule,
        MatDialogModule,
        MatTableModule,
        MatButtonModule,
        MatIconModule,
        MatTooltipModule,
        MatPaginatorModule,
        TranslateModule,
        RouterLink
    ]
})
export class IssuesTableComponent  {

    constructor(public dialog: MatDialog) { }

    @Input() public issues: IIssuesSummary[];
    @Input() public totalRecords: number;

    @Output() public changePage = new EventEmitter<IPagenator>();

    public displayedColumns = ["title", "count",  "actions"]
    public IssueStatus = IssueStatus; 
    public resultsLength: number;
    public gridCount: string = "15";

    public order: string = "id";
    public reverse = false;

    public rowCount = 10;

    public setOrder(value: string, el: any) {
        el = el.toElement || el.relatedTarget || el.target || el.srcElement;

        if (el.nodeName === "A") {
            el = el.parentElement;
        }

        const parent = el.parentElement;
        const previousFilter = parent.querySelector(".active");

        if (this.order === value) {
            this.reverse = !this.reverse;
        } else {
            if (previousFilter) {
                previousFilter.className = "";
            }
            el.className = "active";
        }

        this.order = value;
      }

      public paginate(event: IPagenator) {
        this.changePage.emit(event);
    }
}
