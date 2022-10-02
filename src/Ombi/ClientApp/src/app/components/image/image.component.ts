import { OmbiCommonModules } from "../modules";
import { ChangeDetectionStrategy, Component, ElementRef, Inject, Input, ViewEncapsulation } from "@angular/core";
import { RequestType } from "../../interfaces";
import { APP_BASE_HREF } from "@angular/common";

@Component({
    standalone: true,
    selector: 'ombi-image',
    imports: [...OmbiCommonModules],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    templateUrl: './image.component.html',
  })
  export class ImageComponent {

    @Input() public src: string;
    @Input() public type: RequestType;

    // Attributes from the parent
    @Input() public class: string;
    @Input() public id: string;
    @Input() public alt: string;
    @Input() public style: string;

    public baseUrl: string = "";

    public defaultTv = "/images/default_tv_poster.png";
    private defaultMovie = "/images/default_movie_poster.png";
    private defaultMusic = "i/mages/default-music-placeholder.png";

    private alreadyErrored = false;

    constructor (@Inject(APP_BASE_HREF) public href: string) {
        if (this.href.length > 1) {
            this.baseUrl = this.href;
        }
    }

    public onError(event: any) {
        if (this.alreadyErrored) {
            return;
        }
        // set to a placeholder
        switch(this.type) {
            case RequestType.movie:
                event.target.src = this.baseUrl + this.defaultMovie;
                break;
            case RequestType.tvShow:
                event.target.src = this.baseUrl + this.defaultTv;
                break;
            case RequestType.album:
                event.target.src = this.baseUrl + this.defaultMusic;
                break;
        }

        this.alreadyErrored = true;
        // Retry the original image
        const timeout = setTimeout(() => {
            clearTimeout(timeout);
            event.target.src = this.src;
        }, Math.floor(Math.random() * (7000 - 1000 + 1)) + 1000);
    }
  }