import { Component, OnInit } from "@angular/core";
import { SearchService } from "../../services";
import { ISearchTvResult } from "../../interfaces";

@Component({
    selector: "popular-tv",
    templateUrl: "./popular-tv.component.html",
})
export class PopularTvComponent implements OnInit {

    public tvShows: ISearchTvResult[];

    public defaultPoster: string;

    constructor(private searchService: SearchService) {

    }

    public ngOnInit() {
        this.defaultPoster = "../../../images/default_tv_poster.png";
        this.searchService.popularTv().subscribe(x => {this.tvShows = x; this.getExtraInfo();});
    }

    public getExtraInfo() {
        this.tvShows.forEach((val, index) => {
            this.searchService.getShowInformation(val.id)
                .subscribe(x => {
                    if (x) {
                        this.setDefaults(x);
                        this.updateItem(val, x);
                    } else {
                        const index = this.tvShows.indexOf(val, 0);
                        if (index > -1) {
                            this.tvShows.splice(index, 1);
                        }
                    }
                });
        });
    }

    private setDefaults(x: ISearchTvResult) {
        if (x.banner === null) {
            x.banner = this.defaultPoster;
        }

        if (x.imdbId === null) {
            x.imdbId = "https://www.tvmaze.com/shows/" + x.seriesId;
        } else {
            x.imdbId = "http://www.imdb.com/title/" + x.imdbId + "/";
        }
    }

    private updateItem(key: ISearchTvResult, updated: ISearchTvResult) {
        const index = this.tvShows.indexOf(key, 0);
        if (index > -1) {
            // Update certain properties, otherwise we will loose some data
            this.tvShows[index].title = updated.title;
            this.tvShows[index].banner = updated.banner;
            this.tvShows[index].imdbId = updated.imdbId;
            this.tvShows[index].seasonRequests = updated.seasonRequests;
            this.tvShows[index].seriesId = updated.seriesId;
            this.tvShows[index].fullyAvailable = updated.fullyAvailable;
            this.tvShows[index].background = updated.banner;
        }
    }
}
