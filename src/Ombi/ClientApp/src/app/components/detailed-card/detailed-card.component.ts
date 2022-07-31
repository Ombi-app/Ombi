import { OmbiCommonModules } from "../modules";
import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewEncapsulation } from "@angular/core";
import { IRecentlyRequested, RequestType } from "../../interfaces";
import { ImageComponent } from "../image/image.component";
import { ImageService } from "app/services";
import { Subject, takeUntil } from "rxjs";
import { PipeModule } from "app/pipes/pipe.module";

@Component({
    standalone: true,
    selector: 'ombi-detailed-card',
    imports: [...OmbiCommonModules, ImageComponent, PipeModule],
    providers: [ImageService],
    changeDetection: ChangeDetectionStrategy.OnPush,
    templateUrl: './detailed-card.component.html',
    styleUrls: ['./detailed-card.component.scss']
  })
  export class DetailedCardComponent implements OnInit, OnDestroy {
      @Input() public request: IRecentlyRequested;
      @Input() public is4kEnabled: boolean = false;

      @Output() public onRequest: EventEmitter<boolean> = new EventEmitter<boolean>();

      public RequestType = RequestType;
      public loading: false;

      private $imageSub = new Subject<void>();

    constructor(private imageService: ImageService) { }

      ngOnInit(): void {
        if (!this.request.posterPath) {
          this.imageService.getMoviePoster(this.request.mediaId).pipe(takeUntil(this.$imageSub)).subscribe(x => this.request.posterPath = x);
        }
      }


      public submitRequest(is4k: boolean) {
        this.onRequest.emit(is4k);
      }


      public ngOnDestroy() {
        this.$imageSub.next();
        this.$imageSub.complete();
    }

  }