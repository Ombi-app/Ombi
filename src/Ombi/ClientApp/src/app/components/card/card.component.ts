import { Component, Input } from "@angular/core";

import { DiscoverType } from "../../discover/components/carousel-list/carousel-list.component";
import { EventEmitter } from '@angular/core';
import { IDiscoverCardResult } from "../../discover/interfaces";
import { ISearchTvResultV2 } from "../../interfaces/ISearchTvResultV2";
import { Output } from "@angular/core";
import { RequestType } from "../../interfaces";
import { TranslateService } from "@ngx-translate/core";

@Component({
    selector: "ombi-card",
    templateUrl: "./card.component.html",
    styleUrls: ["./card.component.scss"],
})
export default class CardComponent {

    @Input() public discoverType: DiscoverType;
    @Input() public result: IDiscoverCardResult;
    @Input() public isAdmin: boolean;
    @Input() public is4kEnabled: boolean = false;
    @Output() public requested: EventEmitter<boolean> = new EventEmitter();
    public RequestType = RequestType;
    public hide: boolean;
    public allow4KButton: boolean = false;

    public requestable: boolean;

    // This data is needed to open the dialog
    private tvSearchResult: ISearchTvResultV2;

    constructor(private translate: TranslateService) { }


    public generateDetailsLink(): string {
        switch (this.result.type) {
            case RequestType.movie:
                return `/details/movie/${this.result.id}`;
            case RequestType.tvShow:
                return `/details/tv/${this.result.id}`;
            case RequestType.album: //Actually artist
                return `/details/artist/${this.result.id}`;
        }
    }

    public getStatusClass(): string {
        if (this.result.available) {
            return "available";
        }
        if (this.tvSearchResult?.partlyAvailable) {
            return "partly-available";
        }
        if (this.result.approved) {
            return "approved";
        }
        if (this.result.denied) {
            return "denied";
        }
        if (this.result.requested) {
            return "requested";
        }
        return "";
    }

    public getAvailabilityStatus(): string {
        if (this.result.available) {
            return this.translate.instant("Common.Available");
        }
        if (this.tvSearchResult?.partlyAvailable) {
            return this.translate.instant("Common.PartlyAvailable");
        }
        if (this.result.approved) {
            return this.translate.instant("Common.Approved");
        }
        if (this.result.denied) {
            return this.translate.instant("Common.Denied");
        }
        if (this.result.requested) {
            return this.translate.instant("Common.Pending");
        }
        return "";
    }

    public request(is4k: boolean) {
        this.requested.emit(is4k);
    }
}
