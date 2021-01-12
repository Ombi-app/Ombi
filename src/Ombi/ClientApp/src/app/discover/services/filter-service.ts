import { EventEmitter, Injectable, Output } from "@angular/core";
import { SearchFilter } from "../../my-nav/SearchFilter";

@Injectable()
export class FilterService {

    @Output() public onFilterChange = new EventEmitter<SearchFilter>();
    public filter: SearchFilter;

    public changeFilter(filter: SearchFilter) {
        this.filter = filter;
        this.onFilterChange.emit(this.filter);
    }
}
