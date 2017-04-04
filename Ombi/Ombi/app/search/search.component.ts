import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './search.component.html'
})
export class SearchComponent implements OnInit{
    ngOnInit(): void {
        this.searchText = "";
    }

    searchText : string;
}