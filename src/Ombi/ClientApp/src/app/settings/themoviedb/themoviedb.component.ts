import {COMMA, ENTER} from "@angular/cdk/keycodes";
import { Component, OnInit, ElementRef, ViewChild } from "@angular/core";
import { MatAutocomplete } from "@angular/material/autocomplete";

import { ITheMovieDbSettings, IMovieDbKeyword, IMovieDbGenre } from "../../interfaces";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";
import { TheMovieDbService } from "../../services";
import { FormBuilder, FormGroup } from "@angular/forms";
import { debounceTime, switchMap } from "rxjs/operators";

interface IKeywordTag {
    id: number;
    name: string;
    initial: boolean;
}

interface IGenres {
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
    public excludedKeywords: IKeywordTag[];
    public excludedMovieGenres: IGenres[];
    public excludedTvGenres: IGenres[];
    public tagForm: FormGroup;
    public filteredTags: IMovieDbKeyword[];
    public filteredMovieGenres: IMovieDbGenre[];
    public filteredTvGenres: IMovieDbGenre[];

    @ViewChild('fruitInput') public fruitInput: ElementRef<HTMLInputElement>;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private tmdbService: TheMovieDbService,
                private fb: FormBuilder) { }

    public ngOnInit() {
        this.tagForm = this.fb.group({
            input: null,
          });
        this.settingsService.getTheMovieDbSettings().subscribe(settings => {
            this.settings = settings;

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
                    this.excludedKeywords.filter((val, idx) => {
                        val.name = keyResult.name;
                    })
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

            this.tmdbService.getGenres("movie").subscribe(results => {
                results.forEach(genre => {
                    this.excludedMovieGenres.forEach(key => {
                        this.excludedMovieGenres.filter((val, idx) => {
                            val.name = genre.name
                        })
                    })
                });
            });
            
            // Map Tv Genre ids -> genre name
            this.excludedTvGenres = settings.excludedTvGenreIds
                ? settings.excludedTvGenreIds.map(id => ({
                    id,
                    name: "",
                    initial: true,
                }))
                : [];

            this.tmdbService.getGenres("tv").subscribe(results => {
                results.forEach(genre => {
                    this.excludedTvGenres.forEach(key => {
                        this.excludedTvGenres.filter((val, idx) => {
                            val.name = genre.name
                        })
                    })
                });
            });
        });

        this.tagForm
        .get("input")
        .valueChanges.pipe(
          debounceTime(600),
          switchMap((value: string) => {
            if (value) {
              return this.tmdbService.getKeywords(value);
            }
          })
        )
        .subscribe((r) => (this.filteredTags = r));
    }

    public remove(tag: IKeywordTag): void {
        const index = this.excludedKeywords.indexOf(tag);

        if (index >= 0) {
          this.excludedKeywords.splice(index, 1);
        }
      }

    public save() {
        this.settings.excludedKeywordIds = this.excludedKeywords.map(k => k.id);
        this.settingsService.saveTheMovieDbSettings(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved The Movie Database settings");
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
