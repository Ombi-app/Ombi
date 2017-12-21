import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { AuthService } from "../auth/auth.service";
import { IssuesService, NotificationService, SettingsService } from "../services";

import { IIssues, IIssuesChat, IIssueSettings, INewIssueComments, IssueStatus } from "../interfaces";

@Component({
    templateUrl: "issueDetails.component.html",
    styleUrls: ["./issueDetails.component.scss"],
})
export class IssueDetailsComponent implements OnInit {

    public issue: IIssues;
    public comments: IIssuesChat[];
    public newComment: INewIssueComments = {
        comment: "",
        issueId: 0,
    };

    public IssueStatus = IssueStatus;
    public isAdmin: boolean;
    public settings: IIssueSettings;
    
    private issueId: number;

    constructor(private issueService: IssuesService,
                private route: ActivatedRoute,
                private authService: AuthService,
                private settingsService: SettingsService,
                private notificationService: NotificationService) { 
            this.route.params
            .subscribe((params: any) => {
                  this.issueId = parseInt(params.id);    
                });

            this.isAdmin = this.authService.hasRole("Admin") || this.authService.hasRole("PowerUser");
            this.settingsService.getIssueSettings().subscribe(x => this.settings = x);
        }
                
    public ngOnInit() { 
        this.issueService.getIssue(this.issueId).subscribe(x => {
            this.issue = {
                comments: x.comments,
                id: x.id,
                issueCategory: x.issueCategory,
                issueCategoryId: x.issueCategoryId,
                subject: x.subject,
                description: x.description,
                status:x.status,
                resolvedDate:x.resolvedDate,
                title: x.title,
                requestType: x.requestType,
                requestId: x.requestId,
                providerId: x.providerId,
            };
        });
        
        this.loadComments();
    }

    public addComment() {
        this.newComment.issueId = this.issueId;
        this.issueService.addComment(this.newComment).subscribe(x => {
            this.loadComments();
        });
    }

    public inProgress() {
        this.issueService.updateStatus({issueId: this.issueId, status: IssueStatus.InProgress}).subscribe(x => {
            this.notificationService.success("Marked issue as In Progress");
            this.issue.status = IssueStatus.InProgress;
        });
    }

    public resolve() {
        this.issueService.updateStatus({issueId: this.issueId, status: IssueStatus.Resolved}).subscribe(x => {
            this.notificationService.success("Marked issue as Resolved");
            this.issue.status = IssueStatus.Resolved;
        });
    }

    private loadComments() {
        this.issueService.getComments(this.issueId).subscribe(x => this.comments = x);
    }
}
