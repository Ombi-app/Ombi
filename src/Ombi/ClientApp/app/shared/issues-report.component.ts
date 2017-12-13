import { Component, EventEmitter, Input, Output  } from "@angular/core";

import { IIssueCategory, IIssues, IMovieIssues, IssueStatus, ITvIssues } from "./../interfaces";
import { IssuesService, NotificationService } from "./../services";

@Component({
    selector: "issue-report",
    templateUrl: "issues-report.component.html",
    
})
export class IssuesReportComponent {
    @Input() public visible: boolean;
    @Input() public id: number;
    @Input() public title: string;
    @Input() public issueCategory: IIssueCategory;
    @Input() public movie: boolean;

    @Output() public visibleChange = new EventEmitter<boolean>();

    get getTitle(): string {
        return this.title;
    }

    public issue: IIssues;

    constructor(private issueService: IssuesService,
                private notification: NotificationService) { 
        this.issue = {
            subject: "",
            description: "",
            issueCategory: { value: "", id:0},
            status: IssueStatus.Pending,
            resolvedDate: undefined,
            id: undefined,
            issueCategoryId: 0,
            comments: [],
        };
    }

    public submit() {
        if(this.movie) {
            const movieIssue = <IMovieIssues>this.issue;
            movieIssue.movieId = this.id;
            movieIssue.issueCategory = this.issueCategory;
            movieIssue.issueCategoryId = this.issueCategory.id;
            this.issueService.createMovieIssue(movieIssue).subscribe(x => {
                if(x) {
                    this.notification.success("Issue Created");
                }});
        } else {
            
            const tvIssue = <ITvIssues>this.issue;
            tvIssue.tvId = this.id;
            tvIssue.issueCategory = this.issueCategory;
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
        this.visible = !this.visible; 
        this.visibleChange.emit(this.visible);
    }
}
