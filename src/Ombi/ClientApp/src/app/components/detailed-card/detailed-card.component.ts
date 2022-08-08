import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnDestroy, OnInit, Output } from "@angular/core";
import { IRecentlyRequested, RequestType } from "../../interfaces";
import { ImageService } from "app/services";
import { Subject, takeUntil } from "rxjs";

@Component({
    standalone: false,
    selector: 'ombi-detailed-card',
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

      public getStatus(request: IRecentlyRequested) {
        if (request.available) {
          return "Common.Available";
        }
        if (request.approved) {
          return "Common.Approved";
        } else {
          return "Common.Pending";
        }
      }

      public getClass(request: IRecentlyRequested) {
        if (request.available) {
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