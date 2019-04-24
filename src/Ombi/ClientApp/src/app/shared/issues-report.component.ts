import { Component, EventEmitter, Input, Output } from "@angular/core";

import { IIssueCategory, IIssues, IssueStatus, RequestType } from "../interfaces";
import { IssuesService, NotificationService } from "../services";

@Component({
    selector: "issue-report",
    templateUrl: "issues-report.component.html",

})
export class IssuesReportComponent {
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

    get getTitle(): string {
        return this.title;
    }

    public issue: IIssues;

    constructor(private issueService: IssuesService,
                private notification: NotificationService) {
        this.issue = {
            subject: "",
            description: "",
            issueCategory: { value: "", id: 0 },
            status: IssueStatus.Pending,
            resolvedDate: undefined,
            id: undefined,
            issueCategoryId: 0,
            comments: [],
            requestId: undefined,
            requestType: RequestType.movie,
            title: "",
            providerId: "",
            userReported: undefined,
        };
    }

    public submit() {
        this.submitted = true;
        const issue = this.issue;
        issue.requestId = this.id;
        issue.issueCategory = this.issueCategory;
        issue.issueCategoryId = this.issueCategory.id;
        issue.title = this.title;
        issue.providerId = this.providerId;
        if (this.movie) {
            issue.requestType = RequestType.movie;
        } else {
            issue.requestType = RequestType.tvShow;
        }
        this.issueService.createIssue(issue).subscribe(x => {
            if (x) {
                this.notification.success("Issue Created");
            }
        });

    }

    public hide(): void {
        this.submitted = false;
        this.visible = !this.visible;
        this.visibleChange.emit(this.visible);
    }
}
