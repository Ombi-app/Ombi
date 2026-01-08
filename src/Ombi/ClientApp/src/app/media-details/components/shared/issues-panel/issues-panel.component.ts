import { Component, OnInit, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { TranslateModule } from "@ngx-translate/core";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatTooltipModule } from "@angular/material/tooltip";
import { IssuesService, NotificationService, SettingsService } from "../../../../services";
import { RequestType, IIssues, IssueStatus, IIssueSettings } from "../../../../interfaces";
import { TranslateService } from "@ngx-translate/core";
import { MatExpansionModule } from "@angular/material/expansion";
import { DetailsGroupComponent } from "app/issues/components/details-group/details-group.component";

@Component({
    standalone: true,
    selector: "issues-panel",
    templateUrl: "./issues-panel.component.html",
    styleUrls: ["./issues-panel.component.scss"],
    imports: [
        CommonModule,
        TranslateModule,
        MatButtonModule,
        MatIconModule,
        MatTooltipModule,
        MatExpansionModule,
        DetailsGroupComponent,
    ]
})
export class IssuesPanelComponent implements OnInit {

    @Input() public providerId: string;
    @Input() public isAdmin: boolean;

    public issuesCount: number;
    public issues: IIssues[];
    public IssueStatus = IssueStatus;
    public isOutstanding: boolean;
    public loadingFlag: boolean;
    public settings: IIssueSettings;

    constructor(private issuesService: IssuesService, private notificationService: NotificationService,
                private translateService: TranslateService, private settingsService: SettingsService) {
    }

    public async ngOnInit() {
        this.issues = await this.issuesService.getIssuesByProviderId(this.providerId);
        this.issuesCount = this.issues.length;
        this.calculateOutstanding();
        this.settings = await this.settingsService.getIssueSettings().toPromise();
    }

    public resolve(issue: IIssues) {
        this.issuesService.updateStatus({issueId: issue.id, status: IssueStatus.Resolved}).subscribe(x => {
            this.notificationService.success(this.translateService.instant("Issues.MarkedAsResolved"));
            issue.status = IssueStatus.Resolved;
            this.calculateOutstanding();
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
        this.issues = this.issues.filter((el) => { return el.id !== issue.id; }); 
        this.issuesCount = this.issues.length;
        this.calculateOutstanding();
    }

    private calculateOutstanding() {
        this.isOutstanding = this.issues.some((i) => {
            return i.status !== IssueStatus.Resolved;
        });
    }
}
