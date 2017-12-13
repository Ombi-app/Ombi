import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { IssuesService } from "../services";

import { IIssueDetails, IIssuesChat, INewIssueComments, IssueStatus } from "../interfaces";

@Component({
    templateUrl: "issueDetails.component.html",
    styleUrls: ["./issueDetails.component.scss"],
})
export class IssueDetailsComponent implements OnInit {

    public issue: IIssueDetails;
    public comments: IIssuesChat[];
    public newComment: INewIssueComments = {
        comment: "",
        movieIssueId: undefined,
        tvIssueId: undefined,
    };

    public IssueStatus = IssueStatus;
    
    private issueId: number;
    private type: string; // 1 = Movie, 2 = TV

    constructor(private issueService: IssuesService,
                private route: ActivatedRoute) { 
        this.route.params
        .subscribe((params: any) => {
            this.issueId = parseInt(params.id); 
            this.type = params.type;           
            });
        }
                
    public ngOnInit() { 
        if(this.type === "1") {
            this.issueService.getMovieIssue(this.issueId).subscribe(x => {
                this.issue = {
                    comments: x.comments,
                    id: x.id,
                    issueCategory: x.issueCategory,
                    issueCategoryId: x.issueCategoryId,
                    subject: x.subject,
                    description: x.description,
                    status:x.status,
                    resolvedDate:x.resolvedDate,
                    movie: x.movie,
                    child: undefined,
                };
            });
        } else {
            this.issueService.getTvIssue(this.issueId).subscribe(x => {
                this.issue = {
                    comments: x.comments,
                    id: x.id,
                    issueCategory: x.issueCategory,
                    issueCategoryId: x.issueCategoryId,
                    subject: x.subject,
                    description: x.description,
                    status:x.status,
                    resolvedDate:x.resolvedDate,
                    movie: undefined,
                    child: x.child,
                };
            });
        }
        this.loadComments();
    }

    public addComment() {
        if(this.type === "1") {
            this.newComment.movieIssueId = this.issueId;
        } else {
            this.newComment.tvIssueId = this.issueId;
        }
        this.issueService.addComment(this.newComment).subscribe(x => {
            this.loadComments();
        });
    }

    private loadComments() {
        if(this.type === "1") {
            this.issueService.getMovieComments(this.issueId).subscribe(x => this.comments = x);
        } else {
            this.issueService.getTvComments(this.issueId).subscribe(x => this.comments = x);
        }
    }
}
