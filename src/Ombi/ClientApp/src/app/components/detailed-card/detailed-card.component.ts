import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnDestroy, OnInit, Output } from "@angular/core";
import { IRecentlyRequested, RequestType } from "../../interfaces";
import { ImageService } from "app/services";
import { Subject, takeUntil } from "rxjs";
import { DomSanitizer, SafeStyle } from "@angular/platform-browser";

@Component({
    standalone: false,
    selector: 'ombi-detailed-card',
    templateUrl: './detailed-card.component.html',
    styleUrls: ['./detailed-card.component.scss']
  })
  export class DetailedCardComponent implements OnInit, OnDestroy {
      @Input() public request: IRecentlyRequested;
      @Input() public isAdmin: boolean = false;
      @Output() public onClick: EventEmitter<void> = new EventEmitter<void>();
      @Output() public onApprove: EventEmitter<void> = new EventEmitter<void>();

      public RequestType = RequestType;
      public loading: false;

      private $imageSub = new Subject<void>();

      public background: SafeStyle;

    constructor(private imageService: ImageService, private sanitizer: DomSanitizer) { }

      ngOnInit(): void {
        if (!this.request.posterPath) {
          switch (this.request.type) {
            case RequestType.movie:
              this.imageService.getMoviePoster(this.request.mediaId).pipe(takeUntil(this.$imageSub)).subscribe(x => this.request.posterPath = x);
              this.imageService.getMovieBackground(this.request.mediaId).pipe(takeUntil(this.$imageSub)).subscribe(x => {
                this.background = this.sanitizer.bypassSecurityTrustStyle("linear-gradient(rgba(0,0,0,.5), rgba(0,0,0,.5)), url(" + x + ")");
              });
              break;
            case RequestType.tvShow:
              this.imageService.getTmdbTvPoster(Number(this.request.mediaId)).pipe(takeUntil(this.$imageSub)).subscribe(x => this.request.posterPath = `https://image.tmdb.org/t/p/w300${x}`);
              this.imageService.getTmdbTvBackground(Number(this.request.mediaId)).pipe(takeUntil(this.$imageSub)).subscribe(x => {
                this.background = this.sanitizer.bypassSecurityTrustStyle("linear-gradient(rgba(0,0,0,.5), rgba(0,0,0,.5)), url(https://image.tmdb.org/t/p/w300" + x + ")");
              });
              break;
          }
        }
      }

      public getStatus(request: IRecentlyRequested) {
        if (request.available) {
          return "Common.Available";
        }
        if (request.tvPartiallyAvailable) {
          return "Common.PartiallyAvailable";
        }
        if (request.approved) {
          return "Common.Approved";
        } else {
          return "Common.Pending";
        }
      }

      public click() {
        this.onClick.emit();
      }

      public approve() {
        this.onApprove.emit();
      }

      public getClass(request: IRecentlyRequested) {
        if (request.available || request.tvPartiallyAvailable) {
          return "success";
        }
        if (request.approved) {
          return "primary";
        } else {
          return "info";
        }
      }

      public ngOnDestroy() {
        this.$imageSub.next();
        this.$imageSub.complete();
    }

  }