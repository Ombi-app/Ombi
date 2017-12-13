import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { IssuesService } from "../services";

import { IIssues, IIssuesChat, INewIssueComments, IssueStatus } from "../interfaces";

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
    
    private issueId: number;

    constructor(private issueService: IssuesService,
                private route: ActivatedRoute) { 
        this.route.params
        .subscribe((params: any) => {
            this.issueId = parseInt(params.id);    
            });
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

    private loadComments() {
        this.issueService.getComments(this.issueId).subscribe(x => this.comments = x);
    }
}
