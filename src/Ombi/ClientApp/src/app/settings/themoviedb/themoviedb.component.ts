import { Component, OnInit } from "@angular/core";
import { empty, of } from "rxjs";

import { ITheMovieDbSettings } from "../../interfaces";
import { NotificationService } from "../../services";
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
    public excludedKeywords: IKeywordTag[];

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private tmdbService: TheMovieDbService) { }

    public ngOnInit() {
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
}
