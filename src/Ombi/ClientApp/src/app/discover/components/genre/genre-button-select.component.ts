import { Component, OnInit } from "@angular/core";
import { SearchV2Service } from "../../../services";
import { AuthService } from "../../../auth/auth.service";
import { IMovieDbKeyword } from "../../../interfaces";
import { MatButtonToggleChange } from "@angular/material/button-toggle";
import { CarouselListComponent } from "../carousel-list/carousel-list.component";
import { RequestType } from "../../../interfaces";
import { AdvancedSearchDialogDataService } from "app/shared/advanced-search-dialog/advanced-search-dialog-data.service";
import { Router } from "@angular/router";

@Component({
    selector: "genre-button-select",
    templateUrl: "./genre-button-select.component.html",
    styleUrls: ["./genre-button-select.component.scss"],
})
export class GenreButtonSelectComponent implements OnInit {
    public genreList: IMovieDbKeyword[] = [];
    public selectedGenre: IMovieDbKeyword;
    public mediaType: string = "movie";

    isLoading: boolean = false;

    constructor(private searchService: SearchV2Service, 
        private advancedSearchService: AdvancedSearchDialogDataService,
        private router: Router) { }

    public ngOnInit(): void {
        this.searchService.getGenres(this.mediaType).subscribe(results => {
            this.genreList = results;
        });
    }

    public async toggleChanged(event: MatButtonToggleChange) {
        this.isLoading = true;

        const genres: number[] = [event.value];
        const data = await this.searchService.advancedSearch({
            watchProviders: [],
            genreIds: genres,
            keywordIds: [],
            type: this.mediaType,
        }, 0, 30);

        this.advancedSearchService.setData(data, RequestType.movie);
        this.advancedSearchService.setOptions([], genres, [], null, RequestType.movie, 30);
        this.router.navigate([`discover/advanced/search`]);
    }

    
}
