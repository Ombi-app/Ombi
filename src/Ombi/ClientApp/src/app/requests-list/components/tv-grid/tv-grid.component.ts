import { ChangeDetectorRef, Component } from "@angular/core";
import { IChildRequests, IRequestsViewModel } from "../../../interfaces";
import { Observable } from 'rxjs';

import { AuthService } from "../../../auth/auth.service";
import { FeaturesFacade } from "../../../state/features/features.facade";
import { IdentityService } from "../../../services/identity.service";
import { MatPaginatorModule } from "@angular/material/paginator";
import { RequestFilterType } from "../../models/RequestFilterType";
import { RequestServiceV2 } from "../../../services/requestV2.service";
import { StorageService } from "../../../shared/storage/storage-service";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { MatButtonModule } from "@angular/material/button";
import { MatMenuModule } from "@angular/material/menu";
import { TranslateModule } from "@ngx-translate/core";
import { OmbiDatePipe } from "../../../pipes/OmbiDatePipe";
import { TranslateStatusPipe } from "../../../pipes/TranslateStatus";
import { GridSpinnerComponent } from "../grid-spinner/grid-spinner.component";
import { BaseGridComponent } from "../base-grid/base-grid.component";

@Component({
    standalone: true,
    templateUrl: "./tv-grid.component.html",
    selector: "tv-grid",
    styleUrls: ["../_shared-card-grid.scss", "./tv-grid.component.scss"],
    imports: [
        CommonModule,
        RouterModule,
        MatPaginatorModule,
        MatButtonModule,
        MatMenuModule,
        TranslateModule,
        OmbiDatePipe,
        TranslateStatusPipe,
        GridSpinnerComponent
    ]
})
export class TvGridComponent extends BaseGridComponent<IChildRequests> {
    public dataSource: IChildRequests[] = [];
    public isPlayedSyncEnabled = false;

    protected storageKeySort = "Tv_DefaultRequestListSort";
    protected storageKeySortOrder = "Tv_DefaultRequestListSortOrder";
    protected storageKeyGridCount = "Tv_DefaultGridCount";
    protected storageKeyCurrentFilter = "Tv_DefaultFilter";
    protected storageKeyViewMode = "Tv_DefaultViewMode";

    public override sortOptions = [
        { value: "requestedDate", label: "Requests.RequestDate" },
        { value: "title", label: "Requests.RequestsTitle" },
    ];

    constructor(
        private readonly requestService: RequestServiceV2,
        auth: AuthService,
        ref: ChangeDetectorRef,
        storageService: StorageService,
        identityService: IdentityService,
        private readonly featureFacade: FeaturesFacade
    ) {
        super(auth, ref, storageService, identityService);
    }

    protected override initFeatures() {
        this.isPlayedSyncEnabled = this.featureFacade.isPlayedSyncEnabled();
    }

    protected loadData(): Observable<IRequestsViewModel<IChildRequests>> {
        const count = this.gridCount;
        const offset = this.paginator.pageIndex * count;
        const requestedBy = this.selectedUserId || undefined;
        switch(this.currentFilter) {
            case RequestFilterType.Pending:
                return this.requestService.getPendingTvRequests(count, offset, this.sortActive, this.sortDirection, requestedBy);
            case RequestFilterType.Available:
                return this.requestService.getAvailableTvRequests(count, offset, this.sortActive, this.sortDirection, requestedBy);
            case RequestFilterType.Processing:
                return this.requestService.getProcessingTvRequests(count, offset, this.sortActive, this.sortDirection, requestedBy);
            case RequestFilterType.Denied:
                return this.requestService.getDeniedTvRequests(count, offset, this.sortActive, this.sortDirection, requestedBy);
            case RequestFilterType.All:
            default:
                return this.requestService.getTvRequests(count, offset, this.sortActive, this.sortDirection, requestedBy);
        }
    }

    protected override setData(data: IChildRequests[]) {
        this.dataSource = data;
    }

    protected removeFromDataSource(id: number) {
        this.dataSource = this.dataSource.filter(req => req.id !== id);
    }

    public openOptions(request: IChildRequests) {
        this.emitOptions(request, { has4kRequest: false, hasRegularRequest: true });
    }
}
