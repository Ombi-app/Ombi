import { Component, EventEmitter, Input, OnChanges, Output } from "@angular/core";

import { IIssueCategory, IIssues, IssueStatus, RequestType } from "../interfaces";
import { IssuesService, NotificationService } from "../services";

@Component({
    selector: "issue-report",
    templateUrl: "issues-report.component.html",

})
export class IssuesReportComponent implements OnChanges {
    @Input() public visible: boolean;
    @Input() public id: number; // RequestId
    @Input() public title: string;
    @Input() public issueCategory: IIssueCategory;
    @Input() public movie: boolean;
    @Input() public providerId: string;
    @Input() public background: string;
    @Input() public posterPath: string;

    @Output() public visibleChange = new EventEmitter<boolean>();

    public submitted: boolean = false;
    public issue: IIssues;

    constructor(private issueService: IssuesService,
                private notification: NotificationService) {
    }

    public ngOnChanges() {
        if (this.visible && this.title) {
            this.issue = {
                subject: "",
                description: "",
                issueCategory: this.issueCategory,
                issueCategoryId: this.issueCategory.id,
                status: IssueStatus.Pending,
                resolvedDate: undefined,
                id: undefined,
                comments: [],
                requestId: this.id,
                requestType: this.movie ? RequestType.movie : RequestType.tvShow,
                title: this.title,
                providerId: this.providerId,
                userReported: undefined,
            };
        }
    }

    public submit() {
        this.submitted = true;
        this.issueService.createIssue(this.issue).subscribe(x => {
            if (x) {
                this.notification.success("Issue Created");
                this.hide();
            }
        });
    }

    public hide(): void {
        this.submitted = false;
        this.visibleChange.emit(false);
    }
}
