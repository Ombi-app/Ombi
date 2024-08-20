import { Component, OnInit, computed, signal } from "@angular/core";
import { SearchV2Service } from "../../../services";
import { MatButtonToggleChange } from "@angular/material/button-toggle";
import { RequestType } from "../../../interfaces";
import { AdvancedSearchDialogDataService } from "app/shared/advanced-search-dialog/advanced-search-dialog-data.service";
import { Router } from "@angular/router";
import { map, Observable } from "rxjs";
import { toSignal } from '@angular/core/rxjs-interop';

interface IGenreSelect {
    name: string;
    id: number;
    type: "movie"|"tv";
}
@Component({
    selector: "genre-button-select",
    templateUrl: "./genre-button-select.component.html",
    styleUrls: ["./genre-button-select.component.scss"],
})
export class GenreButtonSelectComponent implements OnInit {

    public movieGenreList = signal<IGenreSelect[]>(null);
    public tvGenreList = signal<IGenreSelect[]>(null);


    isLoading: boolean = false;

    constructor(private searchService: SearchV2Service,
        private advancedSearchService: AdvancedSearchDialogDataService,
        private router: Router) { }

    public ngOnInit(): void {
        this.searchService.getGenres("movie").pipe(map(x => x.slice(0, 10).map(y => ({ name: y.name, id: y.id, type: "movie" } as IGenreSelect))))
            .subscribe(x => {
                this.movieGenreList.set(x);
            });
        this.searchService.getGenres("tv").pipe(map(x => x.slice(0, 10).map(y => ({ name: y.name, id: y.id, type: "tv" } as IGenreSelect))))
            .subscribe(x => {
                this.tvGenreList.set(x);
            });
    }

    public async toggleChanged(event: MatButtonToggleChange, type: "movie"|"tv") {
        this.isLoading = true;

        const genres: number[] = [event.value];
        const data = await this.searchService.advancedSearch({
            watchProviders: [],
            genreIds: genres,
            keywordIds: [],
            type: type,
        }, 0, 30);

        this.advancedSearchService.setData(data, type == "movie" ? RequestType.movie : RequestType.tvShow);
        this.advancedSearchService.setOptions([], genres, [], null, type == "movie" ? RequestType.movie : RequestType.tvShow, 30);
        this.router.navigate([`discover/advanced/search`]);
    }
}
