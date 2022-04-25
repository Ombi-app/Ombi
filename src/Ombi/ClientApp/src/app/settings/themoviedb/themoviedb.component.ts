import {COMMA, ENTER} from "@angular/cdk/keycodes";
import { Component, ElementRef, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { ILanguage, IMovieDbKeyword, ITheMovieDbSettings } from "../../interfaces";
import { debounceTime, switchMap } from "rxjs/operators";

import { MatAutocomplete } from "@angular/material/autocomplete";
import { NotificationService, SearchV2Service } from "../../services";
import { SettingsService } from "../../services";
import { TheMovieDbService } from "../../services";

interface IKeywordTag {
    id: number;
    name: string;
    initial: boolean;
}

@Component({
    templateUrl: "./themoviedb.component.html",
    styleUrls: ["./themoviedb.component.scss"]
})
export class TheMovieDbComponent implements OnInit {

    public settings: ITheMovieDbSettings;
    public originalLanguages: ILanguage[];
    public excludedKeywords: IKeywordTag[];
    public excludedMovieGenres: IKeywordTag[];
    public excludedTvGenres: IKeywordTag[];
    public tagForm: FormGroup;
    public languages: ILanguage[];
    public filteredTags: IMovieDbKeyword[];
    public filteredMovieGenres: IMovieDbKeyword[];
    public filteredTvGenres: IMovieDbKeyword[];


    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private tmdbService: TheMovieDbService,
                private searchService: SearchV2Service,
                private fb: FormBuilder) { }

    public ngOnInit() {
        this.settingsService.getTheMovieDbSettings().subscribe(settings => {
            this.settings = settings;

            this.tagForm = this.fb.group({
              input: null,
              originalLanguages: [this.settings.originalLanguages],
              excludedMovieGenres: null,
              excludedTvGenres: null,
            });

            this.tagForm
            .get("input")
            .valueChanges.pipe(
              debounceTime(600),
              switchMap((value: string) => {
                if (value) {
                  return this.tmdbService.getKeywords(value);
                }
                return [];
              })
            )
            .subscribe((r) => (this.filteredTags = r));


            // Map Keyword ids -> keyword name
            this.excludedKeywords = settings.excludedKeywordIds
                ? settings.excludedKeywordIds.map(id => ({
                    id,
                    name: "",
                    initial: true,
                }))
                : [];

            this.excludedKeywords.forEach(key => {
                this.tmdbService.getKeyword(key.id).subscribe(keyResult => {
                    var keyToUpdate = this.excludedKeywords.filter((val) => {
                        return val.id == key.id;
                    })[0];
                    keyToUpdate.name = keyResult.name;
                });
            });

            // Map Movie Genre ids -> genre name
            this.excludedMovieGenres = settings.excludedMovieGenreIds
                ? settings.excludedMovieGenreIds.map(id => ({
                    id,
                    name: "",
                    initial: true,
                }))
                : [];

            this.searchService.getGenres("movie").subscribe(results => {
                this.filteredMovieGenres = results;

                this.excludedMovieGenres.forEach(genre => {
                    results.forEach(result => {
                        if (genre.id == result.id) {
                            genre.name = result.name;
                        }
                    });
                });
            });

            this.searchService.getLanguages().subscribe((results) => {
              this.languages = results.sort((a: ILanguage, b: ILanguage) => (a.english_name > b.english_name) ? 1 : -1);;
            });

            // Map Tv Genre ids -> genre name
            this.excludedTvGenres = settings.excludedTvGenreIds
                ? settings.excludedTvGenreIds.map(id => ({
                    id,
                    name: "",
                    initial: true,
                }))
                : [];

            this.searchService.getGenres("tv").subscribe(results => {
                this.filteredTvGenres = results;

                this.excludedTvGenres.forEach(genre => {
                    results.forEach(result => {
                        if (genre.id == result.id) {
                            genre.name = result.name;
                        }
                    });
                });
            });
        });

    }

    public remove(tag: IKeywordTag, tag_type: string): void {
        var exclusion_list;

        switch (tag_type) {
            case "keyword":
                exclusion_list = this.excludedKeywords;
                break;
            case "movieGenre":
                exclusion_list = this.excludedMovieGenres;
                break;
            case "tvGenre":
                exclusion_list = this.excludedTvGenres;
                break;
            default:
                return;
        }

        const index = exclusion_list.indexOf(tag);

        if (index >= 0) {
          exclusion_list.splice(index, 1);
        }
      }

    public save() {

        var selectedMovieGenres: number[] = this.tagForm.controls.excludedMovieGenres.value ?? [];
        var selectedTvGenres: number[] = this.tagForm.controls.excludedTvGenres.value ?? [];

        var movieIds: number[] = this.excludedMovieGenres.map(k => k.id);
        var tvIds: number[] = this.excludedTvGenres.map(k => k.id)

        // Concat and dedup already excluded genres + newly selected ones
        selectedMovieGenres = movieIds.concat(selectedMovieGenres.filter(item => movieIds.indexOf(item) < 0));
        selectedTvGenres = tvIds.concat(selectedTvGenres.filter(item => tvIds.indexOf(item) < 0));

        this.settings.excludedKeywordIds = this.excludedKeywords.map(k => k.id);
        this.settings.excludedMovieGenreIds = selectedMovieGenres;
        this.settings.excludedTvGenreIds = selectedTvGenres;

        this.settingsService.saveTheMovieDbSettings(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved The Movie Database settings. Restart the server to refresh the cache.");
            } else {
                this.notificationService.success("There was an error when saving The Movie Database settings");
            }
        });
    }

    public async optionSelected(item: IMovieDbKeyword) {

        if ((item.name || '').trim()) {
            this.excludedKeywords.push({ id: item.id, name: item.name, initial: false });
        }

        this.tagForm.controls.input.setValue(null);
    }
}
