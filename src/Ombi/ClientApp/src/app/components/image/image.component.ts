import { OmbiCommonModules } from "../modules";
import {
  ChangeDetectionStrategy,
  Component,
  Inject,
  Input,
  ViewEncapsulation,
} from "@angular/core";
import { RequestType } from "../../interfaces";
import { APP_BASE_HREF } from "@angular/common";

@Component({
  standalone: true,
  selector: "ombi-image",
  imports: [...OmbiCommonModules],
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: "./image.component.html",
})
export class ImageComponent {
  private _src: string;
  
  @Input() 
  public get src(): string {
    return this._src;
  }
  
  public set src(value: string) {
    if (value === undefined || value === null) {
      console.warn('ImageComponent: src setter received undefined/null value', {
        value: value,
        type: this.type,
        alt: this.alt
      });
    }
    this._src = value;
  }
  
  @Input() public type: RequestType;

  // Attributes from the parent
  @Input() public class: string;
  @Input() public id: string;
  @Input() public alt: string;
  @Input() public style: string;

  private baseUrl: string = "";

  private defaultTv = "/images/default_tv_poster.png";
  private defaultMovie = "/images/default_movie_poster.png";
  private defaultMusic = "/images/default-music-placeholder.png";

  private maxRetries = 1;
  private retriesPerformed = 0;

  constructor(@Inject(APP_BASE_HREF) private href: string) {
    if (this.href.length > 1) {
      this.baseUrl = this.href;
    }
  }

  ngOnInit() {
    if (!this.src) {
      console.warn('ImageComponent: src is undefined or null, using placeholder', {
        src: this.src,
        type: this.type,
        alt: this.alt,
        stack: new Error().stack
      });
      // Prevent unnecessary error handling when src is not specified.
      this.src = this.getPlaceholderImage();
    }
  }

  public onError(event: any) {
    event.target.src = this.getPlaceholderImage();

    if (!this.src || this.retriesPerformed === this.maxRetries) {
      return;
    }

    // Retry the original image.
    this.retriesPerformed++;
    const timeout = setTimeout(() => {
      clearTimeout(timeout);
      event.target.src = this.src;
    }, Math.floor(Math.random() * (7000 - 1000 + 1)) + 1000);
  }

  public getImageSrc(): string {
    if (!this.src) {
      return this.getPlaceholderImage();
    }
    return this.src;
  }

  public getPlaceholderImage() {
    switch (this.type) {
      case RequestType.movie:
        return this.baseUrl + this.defaultMovie;
      case RequestType.tvShow:
        return this.baseUrl + this.defaultTv;
      case RequestType.album:
        return this.baseUrl + this.defaultMusic;
    }
  }
}