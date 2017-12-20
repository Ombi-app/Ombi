import { Component, Input } from "@angular/core";

import { IIssues, IssueStatus } from "./../interfaces";

@Component({
    selector: "issues-table",
    templateUrl: "issuestable.component.html",
})
export class IssuesTableComponent  {

    @Input() public issues: IIssues[];

    public IssueStatus = IssueStatus;

    public order: string = "id";
    public reverse = false;

    public setOrder(value: string) {
        if (this.order === value) {
          this.reverse = !this.reverse;
        }
    
        this.order = value;
      }
}
