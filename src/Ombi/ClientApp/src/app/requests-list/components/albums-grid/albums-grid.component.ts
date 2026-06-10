import { ChangeDetectorRef, Component } from "@angular/core";
import { IAlbumRequest, IRequestsViewModel } from "../../../interfaces";
import { Observable } from 'rxjs';

import { AuthService } from "../../../auth/auth.service";
import { IdentityService } from "../../../services/identity.service";
import { MatPaginatorModule } from "@angular/material/paginator";
import { RequestFilterType } from "../../models/RequestFilterType";
import { RequestServiceV2 } from "../../../services/requestV2.service";
import { StorageService } from "../../../shared/storage/storage-service";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatSelectModule } from "@angular/material/select";
import { TranslateModule } from "@ngx-translate/core";
import { OmbiDatePipe } from "../../../pipes/OmbiDatePipe";
import { GridSpinnerComponent } from "../grid-spinner/grid-spinner.component";
import { BaseGridComponent } from "../base-grid/base-grid.component";

@Component({
    standalone: true,
    templateUrl: "./albums-grid.component.html",
    selector: "albums-grid",
    styleUrls: ["../_shared-card-grid.scss", "./albums-grid.component.scss"],
    imports: [
        CommonModule,
        RouterModule,
        MatPaginatorModule,
        MatButtonModule,
        MatFormFieldModule,
        MatSelectModule,
        TranslateModule,
        OmbiDatePipe,
        GridSpinnerComponent
    ]
})
export class AlbumsGridComponent extends BaseGridComponent<IAlbumRequest> {
    public dataSource: IAlbumRequest[] = [];

    protected storageKeySort = "Albums_DefaultRequestListSort";
    protected storageKeySortOrder = "Albums_DefaultRequestListSortOrder";
    protected storageKeyGridCount = "Albums_DefaultGridCount";
    protected storageKeyCurrentFilter = "Albums_DefaultFilter";
    protected storageKeyViewMode = "Albums_DefaultViewMode";

    public override sortOptions = [
        { value: "requestedDate", label: "Requests.RequestDate" },
        { value: "releaseDate", label: "Requests.ReleaseDateSort" },
        { value: "title", label: "Requests.RequestsTitle" },
        { value: "artistName", label: "Requests.ArtistName" },
    ];

    constructor(
        private readonly requestService: RequestServiceV2,
        ref: ChangeDetectorRef,
        auth: AuthService,
        storageService: StorageService,
        identityService: IdentityService
    ) {
        super(auth, ref, storageService, identityService);
    }

    public loadData(): Observable<IRequestsViewModel<IAlbumRequest>> {
        const count = this.gridCount;
        const offset = this.paginator.pageIndex * count;
        const requestedBy = this.selectedUserId || undefined;
        switch(this.currentFilter) {
            case RequestFilterType.Pending:
                return this.requestService.getAlbumPendingRequests(count, offset, this.sortActive, this.sortDirection, requestedBy);
            case RequestFilterType.Available:
                return this.requestService.getAlbumAvailableRequests(count, offset, this.sortActive, this.sortDirection, requestedBy);
            case RequestFilterType.Processing:
                return this.requestService.getAlbumProcessingRequests(count, offset, this.sortActive, this.sortDirection, requestedBy);
            case RequestFilterType.Denied:
                return this.requestService.getAlbumDeniedRequests(count, offset, this.sortActive, this.sortDirection, requestedBy);
            case RequestFilterType.All:
            default:
                return this.requestService.getAlbumRequests(count, offset, this.sortActive, this.sortDirection, requestedBy);
        }
    }

    protected override setData(data: IAlbumRequest[]) {
        this.dataSource = data;
    }

    protected removeFromDataSource(id: number) {
        this.dataSource = this.dataSource.filter(req => req.id !== id);
    }

    public openOptions(request: IAlbumRequest) {
        this.emitOptions(request, { has4kRequest: false, hasRegularRequest: true });
    }
}
