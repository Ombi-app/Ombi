import { Component, Input, Output, EventEmitter } from "@angular/core";
import { FilterType, IFilter, OrderType } from "../../interfaces";
import { Subject } from "rxjs";
import { debounceTime, distinctUntilChanged } from "rxjs/operators";

export interface IRequestSearchModel {
    searchText: string,
    filter: IFilter,
    orderType: OrderType,
}

var uniqueRadioButtonId: number = 0;

@Component({
    selector: "requests-search-bar",
    templateUrl: "./requests-search-bar.component.html"
})
export class RequestsSearchBar {
    @Input() searchOptions: IRequestSearchModel;
    @Input() showFilter?: boolean;
    @Input() showSort?: boolean;
    @Output() searchEvent = new EventEmitter<IRequestSearchModel>();

    isFilterDisplayed: boolean;
    sortOptions: Array<{ orderType: OrderType, name: string, isActive: boolean }>;
    availabilityFilterOptions: Array<{ groupName: string, id: string, filterType: FilterType, text: string }>;
    requestStatusFilterOptions: Array<{ groupName: string, id: string, filterType: FilterType, text: string }>;

    private searchChanged = new Subject<string>();

    constructor() {
        uniqueRadioButtonId++;
        this.sortOptions = this.getSortOptions();
        this.availabilityFilterOptions = this.getAvailabilityFilterOptions();
        this.requestStatusFilterOptions = this.getRequestStatusFilterOptions();

        this.searchChanged.pipe(
            debounceTime(600), // Wait Xms after the last event before emitting last event
            distinctUntilChanged(), // only emit if value is different from previous value
        ).subscribe((searchText: any) => {
            this.searchOptions.searchText = searchText as string;
            this.searchEvent.emit(this.searchOptions);
        });
    }

    public ngOnInit() {
        this.markSelectedSortOptionAsActive();
        if (this.showFilter === null || this.showFilter === undefined) {
            this.showFilter = true;
        }
        if (this.showSort === null || this.showSort === undefined) {
            this.showSort = true;
        }
    }

    public searchOnKeyUp(searchText: any) {
        this.searchChanged.next(searchText.target.value);
    }

    public clearFilterClick() {
        this.isFilterDisplayed = false;
        this.searchOptions.filter.availabilityFilter = FilterType.None;
        this.searchOptions.filter.statusFilter = FilterType.None;
        this.searchEvent.emit(this.searchOptions);
    }

    public filterOptionsChange() {
        this.searchEvent.emit(this.searchOptions);
    }

    public setOrderClick(value: OrderType) {
        this.searchOptions.orderType = value;
        this.markSelectedSortOptionAsActive();
        this.searchEvent.emit(this.searchOptions);
    }

    private markSelectedSortOptionAsActive() {
        this.sortOptions = this.sortOptions.map(option => {
            option.isActive = option.orderType === this.searchOptions.orderType;
            return option;
        });
    }

    private getAvailabilityFilterOptions() {
        const groupName = `Availability_${uniqueRadioButtonId}`;
        return [
            {
                groupName: groupName,
                id: `Available_${uniqueRadioButtonId}`,
                filterType: FilterType.Available,
                text: "Common.Available"
            },
            {
                groupName: groupName,
                id: `NotAvailable_${uniqueRadioButtonId}`,
                filterType: FilterType.NotAvailable,
                text: "Common.NotAvailable"
            }
        ];
    }

    private getRequestStatusFilterOptions() {
        const groupName = `Status_${uniqueRadioButtonId}`;
        return [
            {
                groupName: groupName,
                id: `Approved_${uniqueRadioButtonId}`,
                filterType: FilterType.Approved,
                text: "Filter.Approved"
            },
            {
                groupName: groupName,
                id: `Processing_${uniqueRadioButtonId}`,
                filterType: FilterType.Processing,
                text: "Common.ProcessingRequest"
            },
            {
                groupName: groupName,
                id: `PendingApproval_${uniqueRadioButtonId}`,
                filterType: FilterType.PendingApproval,
                text: "Filter.PendingApproval"
            }
        ];
    }

    private getSortOptions() {
        return [
            {
                isActive: false,
                name: "Requests.SortRequestDateAsc",
                orderType: OrderType.RequestedDateAsc
            },
            {
                isActive: false,
                name: "Requests.SortRequestDateDesc",
                orderType: OrderType.RequestedDateDesc
            },
            {
                isActive: false,
                name: "Requests.SortTitleAsc",
                orderType: OrderType.TitleAsc
            },
            {
                isActive: false,
                name: "Requests.SortTitleDesc",
                orderType: OrderType.TitleDesc
            },
            {
                isActive: false,
                name: "Requests.SortStatusAsc",
                orderType: OrderType.StatusAsc
            },
            {
                isActive: false,
                name: "Requests.SortStatusDesc",
                orderType: OrderType.StatusDesc
            }
        ];
    }
}
