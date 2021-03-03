import { Component, Input } from "@angular/core";
import { MatDialog } from '@angular/material/dialog';
import { TranslateService } from "@ngx-translate/core";
import { IIssues, IIssueSettings, IssueStatus } from "../../../interfaces";
import { IssuesService, NotificationService } from "../../../services";
import { IssueChatComponent } from "../issue-chat/issue-chat.component";

@Component({
    selector: "issues-details-group",
    templateUrl: "details-group.component.html",
    styleUrls: ["details-group.component.scss"],
})
export class DetailsGroupComponent {

    @Input() public issue: IIssues;
    @Input() public isAdmin: boolean;
    @Input() public settings: IIssueSettings;

    public deleted: boolean;
    public IssueStatus = IssueStatus;
    public get hasRequest(): boolean {
        if (this.issue.requestId) {
            return true;
        }
        return false;
    }

 constructor(
    private translateService: TranslateService, private issuesService: IssuesService,
    private notificationService: NotificationService, private dialog: MatDialog) { }

    public async delete(issue: IIssues) {
        await this.issuesService.deleteIssue(issue.id);
        this.notificationService.success(this.translateService.instant("Issues.DeletedIssue"));
        this.deleted = true;
    }

    public openChat(issue: IIssues) {
        this.dialog.open(IssueChatComponent, { width: "100vh", data: { issueId: issue.id },  panelClass: 'modal-panel' })
    }

    public resolve(issue: IIssues) {
        this.issuesService.updateStatus({issueId: issue.id, status: IssueStatus.Resolved}).subscribe(() => {
            this.notificationService.success(this.translateService.instant("Issues.MarkedAsResolved"));
            issue.status = IssueStatus.Resolved;
        });
    }

    public inProgress(issue: IIssues) {
        this.issuesService.updateStatus({issueId: issue.id, status: IssueStatus.InProgress}).subscribe(() => {
            this.notificationService.success(this.translateService.instant("Issues.MarkedAsInProgress"));
            issue.status = IssueStatus.InProgress;
        });
    }
}
