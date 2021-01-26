import { Component, Input, Output, EventEmitter } from "@angular/core";
@Component({
    selector: "social-icons",
    templateUrl: "./social-icons.component.html",
    styleUrls: ["./social-icons.component.scss"]
})
export class SocialIconsComponent {
    @Input() homepage: string;
    @Input() theMoviedbId: number;
    @Input() hasTrailer: boolean;
    @Input() imdbId: string;
    @Input() tvdbId: string;
    @Input() twitter: string;
    @Input() facebook: string;
    @Input() instagram: string;
    @Input() available: boolean;
    @Input() plexUrl: string;
    @Input() embyUrl: string;
    @Input() jellyfinUrl: string;
    @Input() doNotAppend: boolean;

    @Input() isAdmin: boolean;
    @Input() canRequestOnBehalf: boolean;
    @Input() canShowAdvanced: boolean;

    @Output() openTrailer: EventEmitter<any> = new EventEmitter();
    @Output() onRequestBehalf: EventEmitter<any> = new EventEmitter();
    @Output() onAdvancedOptions: EventEmitter<any> = new EventEmitter();


    public openDialog() {
        this.openTrailer.emit();
    }

    public openRequestOnBehalf() {
        this.onRequestBehalf.emit();
    }

    public openAdvancedOptions() {
        this.onAdvancedOptions.emit();
    }
}
