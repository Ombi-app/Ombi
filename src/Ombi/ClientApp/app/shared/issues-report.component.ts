import { Component, EventEmitter, Input, Output  } from "@angular/core";

import { IIssueCategory, IIssues, IMovieIssues, IssueStatus, ITvIssues } from "./../interfaces";
import { IssuesService, NotificationService } from "./../services";

@Component({
    selector: "issues-report",
    templateUrl: "issues-report.component.html",
})
export class IssuesReportComponent {
    @Input() public visible: boolean;
    @Input() public id: number;
    @Input() public title: string;
    @Input() public issueCategory: IIssueCategory;
    @Input() public movie: boolean;

    @Output() public close = new EventEmitter();

    public issue: IIssues;

    constructor(private issueService: IssuesService,
                private notification: NotificationService) { 
        this.issue = {
            subject: "",
            description: "",
            issueCategoryId: -1,
            status: IssueStatus.Pending,
            resolvedDate: undefined,
            id: undefined,
        };
    }

    public submit() {
        if(this.movie) {
            const movieIssue = <IMovieIssues>this.issue;
            movieIssue.movieId = this.id;
            movieIssue.issueCategoryId = this.issueCategory.id;
            this.issueService.createMovieIssue(movieIssue).subscribe(x => {
                if(x) {
                    this.notification.success("Issue Created");
                }});
        } else {
            
            const tvIssue = <ITvIssues>this.issue;
            tvIssue.tvId = this.id;
            tvIssue.issueCategoryId = this.issueCategory.id;
            
            this.issueService.createTvIssue(tvIssue).subscribe(x => {
                if(x) {
                    this.notification.success("Issue Created");
                    this.visible = false;
                }
            });
        }
    }

    public hide(): void {
        this.visible = false;
        this.close.emit(true);
    }
}
