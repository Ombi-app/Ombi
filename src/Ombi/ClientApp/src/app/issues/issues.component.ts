import { Component, computed, inject, signal, ChangeDetectionStrategy } from "@angular/core";
import { PageEvent } from '@angular/material/paginator';
import { IssuesService } from "../services";
import { IssuesV2Service } from "../services/issuesv2.service";
import { IIssueCount, IIssues, IIssuesSummary, IssueStatus } from "../interfaces";
import { CommonModule } from "@angular/common";
import { MatPaginatorModule } from "@angular/material/paginator";
import { MatTabsModule } from "@angular/material/tabs";
import { MatCardModule } from "@angular/material/card";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { IssuesTableComponent } from "./issuestable.component";

@Component({
    standalone: true,
    templateUrl: "issues.component.html",
    styleUrls: ['issues.component.scss'],
    imports: [
        CommonModule,
        MatPaginatorModule,
        MatTabsModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatTooltipModule,
        TranslateModule,
        IssuesTableComponent
    ]
})
export class IssuesComponent {
    // Services using inject() function
    private issuev2Service = inject(IssuesV2Service);
    private issueService = inject(IssuesService);

    // State using signals
    private pendingSkip = signal(0);
    private inProgressSkip = signal(0);
    private resolvedSkip = signal(0);
    
    private takeAmount = 50;

    // Public state signals
    public pendingIssues = signal<IIssuesSummary[]>([]);
    public inProgressIssues = signal<IIssuesSummary[]>([]);
    public resolvedIssues = signal<IIssuesSummary[]>([]);
    public count = signal<IIssueCount | null>(null);

    // Computed properties for derived state
    public hasPendingIssues = computed(() => this.pendingIssues().length > 0);
    public hasInProgressIssues = computed(() => this.inProgressIssues().length > 0);
    public hasResolvedIssues = computed(() => this.resolvedIssues().length > 0);
    
    public totalIssues = computed(() => {
        const count = this.count();
        if (!count) return 0;
        return count.pending + count.inProgress + count.resolved;
    });

    constructor() {
        // Initialize data
        this.loadInitialData();
    }

    private loadInitialData(): void {
        this.getPending();
        this.getInProg();
        this.getResolved();
        this.issueService.getIssuesCount().subscribe(count => this.count.set(count));
    }

    public changePagePending(event: PageEvent): void {
        this.pendingSkip.set(event.pageSize * event.pageIndex);
        this.getPending();
    }

    public changePageInProg(event: PageEvent): void {
        this.inProgressSkip.set(event.pageSize * event.pageIndex);
        this.getInProg();
    }

    public changePageResolved(event: PageEvent): void {
        this.resolvedSkip.set(event.pageSize * event.pageIndex);
        this.getResolved();
    }

    private getPending(): void {
        this.issuev2Service.getIssues(this.pendingSkip(), this.takeAmount, IssueStatus.Pending)
            .subscribe(issues => this.pendingIssues.set(issues));
    }

    private getInProg(): void {
        this.issuev2Service.getIssues(this.inProgressSkip(), this.takeAmount, IssueStatus.InProgress)
            .subscribe(issues => this.inProgressIssues.set(issues));
    }

    private getResolved(): void {
        this.issuev2Service.getIssues(this.resolvedSkip(), this.takeAmount, IssueStatus.Resolved)
            .subscribe(issues => this.resolvedIssues.set(issues));
    }
}
