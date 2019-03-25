import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Component, OnInit, Inject } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { AuthService } from "../auth/auth.service";
import { ImageService, IssuesService, NotificationService, SettingsService } from "../services";

import { DomSanitizer } from "@angular/platform-browser";
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
    public backgroundPath: any;
    public posterPath: any;
    public defaultPoster: string;

    private issueId: number;

    constructor(private issueService: IssuesService,
                private route: ActivatedRoute,
                private authService: AuthService,
                private settingsService: SettingsService,
                private notificationService: NotificationService,
                private imageService: ImageService,
                private sanitizer: DomSanitizer,
                @Inject(APP_BASE_HREF) href:string) {
            this.route.params
            .subscribe((params: any) => {
                  this.issueId = parseInt(params.id);
                });

            this.isAdmin = this.authService.hasRole("Admin") || this.authService.hasRole("PowerUser");
            this.settingsService.getIssueSettings().subscribe(x => this.settings = x);

            const base = href;
            if (base) {
                this.defaultPoster = "../../.." + base + "/images/";
            } else {
                this.defaultPoster = "../../../images/";
            }
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
                status: x.status,
                resolvedDate: x.resolvedDate,
                title: x.title,
                requestType: x.requestType,
                requestId: x.requestId,
                providerId: x.providerId,
                userReported: x.userReported,
            };
            this.setBackground(x);
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

    public deleteComment(id: number) {
        this.issueService.deleteComment(id).subscribe(x => {
            this.loadComments();
            this.notificationService.success("Comment Deleted");
        });
    }

    private loadComments() {
        this.issueService.getComments(this.issueId).subscribe(x => this.comments = x);
    }

    private setBackground(issue: any) {
        if (issue.requestType === 1) {
            this.imageService.getMovieBackground(issue.providerId).subscribe(x => {
                this.backgroundPath = this.sanitizer.bypassSecurityTrustStyle
                    ("url(" + x + ")");
            });
            this.imageService.getMoviePoster(issue.providerId).subscribe(x => {
                if (x.length === 0) {
                    this.posterPath = this.defaultPoster + "default_movie_poster.png";
                } else {
                    this.posterPath = x.toString();
                }
            });

        } else {
            this.imageService.getTvBackground(Number(issue.providerId)).subscribe(x => {
                if (x) {
                    this.backgroundPath = this.sanitizer.bypassSecurityTrustStyle
                        ("url(" + x + ")");
                }
            });
            this.imageService.getTvPoster(Number(issue.providerId)).subscribe(x => {
                if (x.length === 0) {
                    this.posterPath = this.defaultPoster + "default_tv_poster.png";
                } else {
                    this.posterPath = x.toString();
                }
            });
        }

    }
}
