import {COMMA, ENTER} from "@angular/cdk/keycodes";
import { Component, OnInit, ElementRef, ViewChild } from "@angular/core";
import { MatChipInputEvent } from "@angular/material/chips";
import { MatAutocompleteSelectedEvent, MatAutocomplete } from "@angular/material/autocomplete";
import { empty, of, Observable } from "rxjs";

import { ITheMovieDbSettings, IMovieDbKeyword } from "../../interfaces";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";
import { TheMovieDbService } from "../../services";
import { FormControl, FormBuilder, FormGroup } from "@angular/forms";
import { startWith, map, debounceTime, tap, switchMap, finalize } from "rxjs/operators";

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
    public excludedKeywords: IKeywordTag[];
    public tagForm: FormGroup;
    public filteredTags: IMovieDbKeyword[];
    @ViewChild('fruitInput') public fruitInput: ElementRef<HTMLInputElement>;
    @ViewChild('auto') public matAutocomplete: MatAutocomplete;

    private readonly separatorKeysCodes: number[] = [ENTER, COMMA];

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
            this.excludedKeywords = settings.excludedKeywordIds
                ? settings.excludedKeywordIds.map(id => ({
                    id,
                    name: "",
                    initial: true,
                }))
                : [];
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

    //     this.tagForm.controls.input.valueChanges
    //   .pipe(
    //     debounceTime(500),
    //     switchMap(value => this.tmdbService.getKeywords(value))
    //   )
    //   .subscribe((data: IMovieDbKeyword[]) => {
    //     this.filteredTags = data;
    //   });
    }

    public async selected(event: MatAutocompleteSelectedEvent) {
        const keywordId = await this.tmdbService.getKeyword(+event.option.value).toPromise();
        this.excludedKeywords.push({ id: keywordId.id, name: keywordId.name, initial: false});
        this.fruitInput.nativeElement.value = '';
        this.tagForm.controls.input.setValue(null);
      }

    public autocompleteKeyword = (text: string) => this.tmdbService.getKeywords(text);

    public onAddingKeyword = (tag: string | IKeywordTag) => {
        if (typeof tag === "string") {
            const id = Number(tag);
            return isNaN(id) ? empty() : this.tmdbService.getKeyword(id);
        } else {
            return of(tag);
        }
    }

    public onKeywordSelect = (keyword: IKeywordTag) => {
        if (keyword.initial) {
            this.tmdbService.getKeyword(keyword.id)
                .subscribe(k => {
                    keyword.name = k.name;
                    keyword.initial = false;
                });
        }
    }

    public async add(event: MatChipInputEvent) {
        const input = event.input;
        const value = event.value;

        // Add our fruit
        if ((value || '').trim()) {
            const keyword = await this.tmdbService.getKeywords(value).toPromise();
            this.excludedKeywords.push({ id: keyword[0].id, name: keyword[0].name, initial: false });
        }

        // Reset the input value
        if (input) {
          input.value = '';
        }
        this.tagForm.controls.input.setValue(null);
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
