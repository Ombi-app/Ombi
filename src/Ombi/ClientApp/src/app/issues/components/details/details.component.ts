import { ActivatedRoute, ActivatedRouteSnapshot, Router } from "@angular/router";
import { Component, Inject, OnInit, ViewEncapsulation } from "@angular/core";
import { IIssueSettings, IIssues, IIssuesSummary, IssueStatus, RequestType } from "../../../interfaces";
import { IssuesService, NotificationService, SettingsService } from "../../../services";
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';

import { AuthService } from "../../../auth/auth.service";
import { IssueChatComponent } from "../issue-chat/issue-chat.component";
import { IssuesV2Service } from "../../../services/issuesv2.service";
import { TranslateService } from "@ngx-translate/core";

export interface IssuesDetailsGroupData {
    issues: IIssues[];
    title: string;
}

@Component({
    selector: "issues-details",
    templateUrl: "details.component.html",
    styleUrls: ["details.component.scss"],
    encapsulation: ViewEncapsulation.None
})
export class IssuesDetailsComponent implements OnInit {

    public details: IIssuesSummary;
    public isAdmin: boolean;
    public IssueStatus = IssueStatus;
    public settings: IIssueSettings;
    public get hasRequest(): boolean {
        return this.details.issues.some(x => x.requestId);
    }

    private providerId: string;

    constructor(private authService: AuthService, private settingsService: SettingsService,
                private issueServiceV2: IssuesV2Service, private route: ActivatedRoute, private router: Router,
                private issuesService: IssuesService, private translateService: TranslateService, private notificationService: NotificationService,
                private dialog: MatDialog) {
                    this.route.params.subscribe(async (params: any) => {
                        if (typeof params.providerId === 'string' || params.providerId instanceof String) {
                            this.providerId = params.providerId;
                        }
                    });
                }

    public ngOnInit() {
        this.isAdmin = this.authService.hasRole("Admin") || this.authService.hasRole("PowerUser");
        this.settingsService.getIssueSettings().subscribe(x => this.settings = x);
        this.issueServiceV2.getIssuesByProviderId(this.providerId).subscribe(x => this.details = x);
    }

    public resolve(issue: IIssues) {
        this.issuesService.updateStatus({issueId: issue.id, status: IssueStatus.Resolved}).subscribe(x => {
            this.notificationService.success(this.translateService.instant("Issues.MarkedAsResolved"));
            issue.status = IssueStatus.Resolved;
        });
    }

    public inProgress(issue: IIssues) {
        this.issuesService.updateStatus({issueId: issue.id, status: IssueStatus.InProgress}).subscribe(x => {
            this.notificationService.success(this.translateService.instant("Issues.MarkedAsInProgress"));
            issue.status = IssueStatus.InProgress;
        });
    }

    public async delete(issue: IIssues) {
        await this.issuesService.deleteIssue(issue.id);
        this.notificationService.success(this.translateService.instant("Issues.DeletedIssue"));
        this.details.issues =  this.details.issues.filter((el) => { return el.id !== issue.id; }); 
    }

    public openChat(issue: IIssues) {
        this.dialog.open(IssueChatComponent, { width: "100vh", data: { issueId: issue.id },  panelClass: 'modal-panel' })
    }

    public navToMedia() {
        const firstIssue = this.details.issues[0];
        switch(firstIssue.requestType) {
            case RequestType.movie:
                this.router.navigate(['/details/movie/', this.providerId]);
                return;


            case RequestType.artist:
                this.router.navigate(['/details/artist/', firstIssue.providerId]);
                return;

            case RequestType.tvShow:
                this.router.navigate(['/details/tv/', this.providerId]);
                return;
        }
    }

}
