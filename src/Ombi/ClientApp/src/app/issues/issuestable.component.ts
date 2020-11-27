import { Component, EventEmitter, Input, Output } from "@angular/core";

import { IIssues, IPagenator, IssueStatus } from "../interfaces";

@Component({
    selector: "issues-table",
    templateUrl: "issuestable.component.html",
})
export class IssuesTableComponent  {

    @Input() public issues: IIssues[];
    @Input() public totalRecords: number;

    @Output() public changePage = new EventEmitter<IPagenator>();

    public displayedColumns = ["title", "category", "subject", "status", "reportedBy", "actions"]
    public IssueStatus = IssueStatus;

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
        //event.first = Index of the first record (current index)
        //event.rows = Number of rows to display in new page
        //event.page = Index of the new page
        //event.pageCount = Total number of pages

        this.changePage.emit(event);
    }

}
