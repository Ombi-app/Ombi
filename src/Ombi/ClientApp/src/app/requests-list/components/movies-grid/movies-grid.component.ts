import { ChangeDetectorRef, Component } from "@angular/core";
import { IMovieRequests, IRequestEngineResult, IRequestsViewModel } from "../../../interfaces";
import { NotificationService, RequestService } from "../../../services";
import { Observable, combineLatest, forkJoin } from 'rxjs';

import { AuthService } from "../../../auth/auth.service";
import { FeaturesFacade } from "../../../state/features/features.facade";
import { MatPaginatorModule } from "@angular/material/paginator";
import { MatTableDataSource } from "@angular/material/table";
import { RequestFilterType } from "../../models/RequestFilterType";
import { RequestServiceV2 } from "../../../services/requestV2.service";
import { SelectionModel } from "@angular/cdk/collections";
import { StorageService } from "../../../shared/storage/storage-service";
import { TranslateService } from "@ngx-translate/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { GridSpinnerComponent } from "../grid-spinner/grid-spinner.component";
import { TranslateModule } from "@ngx-translate/core";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatOptionModule } from "@angular/material/core";
import { MatSelectModule } from "@angular/material/select";
import { MatMenuModule } from "@angular/material/menu";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatTooltipModule } from "@angular/material/tooltip";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { OmbiDatePipe } from "../../../pipes/OmbiDatePipe";
import { TranslateStatusPipe } from "../../../pipes/TranslateStatus";
import { BaseGridComponent } from "../base-grid/base-grid.component";

@Component({
    standalone: true,
    templateUrl: "./movies-grid.component.html",
    selector: "movies-grid",
    styleUrls: ["../_shared-card-grid.scss", "./movies-grid.component.scss"],
    imports: [
        CommonModule,
        RouterModule,
        GridSpinnerComponent,
        TranslateModule,
        MatFormFieldModule,
        MatOptionModule,
        MatPaginatorModule,
        MatSelectModule,
        MatMenuModule,
        MatButtonModule,
        MatIconModule,
        MatTooltipModule,
        MatCheckboxModule,
        OmbiDatePipe,
        TranslateStatusPipe
    ]
})
export class MoviesGridComponent extends BaseGridComponent<IMovieRequests> {
    public dataSource: MatTableDataSource<IMovieRequests> = new MatTableDataSource<IMovieRequests>();
    public is4kEnabled = false;
    public isPlayedSyncEnabled = false;
    public selection = new SelectionModel<IMovieRequests>(true, []);

    protected storageKeySort = "Movie_DefaultRequestListSort";
    protected storageKeySortOrder = "Movie_DefaultRequestListSortOrder";
    protected storageKeyGridCount = "Movie_DefaultGridCount";
    protected storageKeyCurrentFilter = "Movie_DefaultFilter";

    constructor(
        private readonly requestService: RequestServiceV2,
        ref: ChangeDetectorRef,
        auth: AuthService,
        storageService: StorageService,
        private readonly requestServiceV1: RequestService,
        private readonly notification: NotificationService,
        private readonly translateService: TranslateService,
        private readonly featureFacade: FeaturesFacade
    ) {
        super(auth, ref, storageService);
    }

    protected override initFeatures() {
        this.is4kEnabled = this.featureFacade.is4kEnabled();
        this.isPlayedSyncEnabled = this.featureFacade.isPlayedSyncEnabled();
    }

    protected override setData(data: IMovieRequests[]) {
        this.selection.clear();
        this.dataSource = new MatTableDataSource(data);
    }

    public loadData(): Observable<IRequestsViewModel<IMovieRequests>> {
        const count = this.gridCount;
        const offset = this.paginator.pageIndex * count;
        switch(this.currentFilter) {
            case RequestFilterType.All:
                return this.requestService.getMovieRequests(count, offset, this.sortActive, this.sortDirection);
            case RequestFilterType.Pending:
                return this.requestService.getMoviePendingRequests(count, offset, this.sortActive, this.sortDirection);
            case RequestFilterType.Available:
                return this.requestService.getMovieAvailableRequests(count, offset, this.sortActive, this.sortDirection);
            case RequestFilterType.Processing:
                return this.requestService.getMovieProcessingRequests(count, offset, this.sortActive, this.sortDirection);
            case RequestFilterType.Denied:
                return this.requestService.getMovieDeniedRequests(count, offset, this.sortActive, this.sortDirection);
            default:
                return this.requestService.getMovieRequests(count, offset, this.sortActive, this.sortDirection);
        }
    }

    protected removeFromDataSource(id: number) {
        const removed = this.dataSource.data.find(req => req.id === id);
        if (removed) {
            this.selection.deselect(removed);
        }
        this.dataSource.data = this.dataSource.data.filter(req => req.id !== id);
    }

    public openOptions(request: IMovieRequests) {
        this.emitOptions(request, {
            has4kRequest: request.has4KRequest,
            hasRegularRequest: this.checkDate(request.requestedDate)
        });
    }

    private checkDate(date: Date|string): boolean {
        if (typeof date === 'string') return new Date(date).getFullYear() > 1;
        return date.getFullYear() > 1;
    }

    public isAllSelected() {
        return this.selection.selected.length === this.dataSource.data.length;
    }

    public masterToggle() {
        this.isAllSelected() ?
            this.selection.clear() :
            this.dataSource.data.forEach(row => this.selection.select(row));
    }

    public async bulkDelete() {
        if (this.selection.isEmpty()) return;
        const tasks = new Array<Observable<IRequestEngineResult>>();
        this.selection.selected.forEach(s => tasks.push(this.requestServiceV1.removeMovieRequestAsync(s.id)));
        combineLatest(tasks).subscribe((result: IRequestEngineResult[]) => {
            const failed = result.filter(x => !x.result);
            if (failed.length > 0) {
                this.notification.error(`Some requests failed to delete: ${failed[0].errorMessage}`);
                this.selection.clear();
                this.refresh();
                return;
            }
            this.notification.success(this.translateService.instant('Requests.RequestPanel.Deleted'));
            this.selection.clear();
            this.refresh();
        });
    }

    public bulkApprove = () => this.bulkAction(false, true);
    public bulkApprove4K = () => this.bulkAction(true, true);
    public bulkDeny = () => this.bulkAction(false, false);
    public bulkDeny4K = () => this.bulkAction(true, false);

    private bulkAction(is4k: boolean, approve: boolean) {
        const eligible = this.selection.selected.filter(s =>
            is4k ? s.has4KRequest : this.checkDate(s.requestedDate)
        );
        if (eligible.length === 0) return;
        const tasks = eligible.map(s =>
            approve
                ? this.requestServiceV1.approveMovie({ id: s.id, is4K: is4k })
                : this.requestServiceV1.denyMovie({ id: s.id, is4K: is4k, reason: '' })
        );
        this.isLoadingResults = true;
        forkJoin(tasks).subscribe((result: IRequestEngineResult[]) => {
            this.isLoadingResults = false;
            const failed = result.filter(x => !x.result);
            if (failed.length > 0) {
                const action = approve ? 'approve' : 'deny';
                this.notification.error(`Some requests failed to ${action}: ${failed[0].errorMessage}`);
                this.selection.clear();
                this.refresh();
                return;
            }
            const key = approve ? 'Requests.RequestPanel.Approved' : 'Requests.RequestPanel.Denied';
            this.notification.success(this.translateService.instant(key));
            this.selection.clear();
            this.refresh();
        });
    }

    public getRequestDate(request: IMovieRequests): Date {
        return this.checkDate(request.requestedDate)
            ? request.requestedDate
            : request.requestedDate4k;
    }
}
