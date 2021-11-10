import { Component, Inject } from '@angular/core';
import { MAT_BOTTOM_SHEET_DATA, MatBottomSheetRef } from '@angular/material/bottom-sheet';
import { MessageService, RequestService } from '../../../services';
import { IRequestEngineResult, RequestType } from '../../../interfaces';
import { UpdateType } from '../../models/UpdateType';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';

@Component({
  selector: 'request-options',
  templateUrl: './request-options.component.html',
})
export class RequestOptionsComponent {

  public RequestType = RequestType;

  constructor(@Inject(MAT_BOTTOM_SHEET_DATA) public data: any,
    private requestService: RequestService, 
    private messageService: MessageService,
    private bottomSheetRef: MatBottomSheetRef<RequestOptionsComponent>,
    private translate: TranslateService) { }

  public async delete() {
    var  request: Observable<IRequestEngineResult>;
    if (this.data.type === RequestType.movie) {
      request = this.requestService.removeMovieRequestAsync(this.data.id);
    }
    if (this.data.type === RequestType.tvShow) {
      request = this.requestService.deleteChild(this.data.id);
    }
    if (this.data.type === RequestType.album) {
      request = this.requestService.removeAlbumRequest(this.data.id);
    }
    request.subscribe(result => {
      if (result.result) {
          this.messageService.send(this.translate.instant("Requests.SuccessfullyDeleted"));
          this.bottomSheetRef.dismiss({type: UpdateType.Delete});
          return;
      } else {
          this.messageService.sendRequestEngineResultError(result);
      }
    });
  }

  public async approve() {
    if (this.data.type === RequestType.movie) {
      await this.requestService.approveMovie({id: this.data.id}).toPromise();
    }
    if (this.data.type === RequestType.tvShow) {
      await this.requestService.approveChild({id: this.data.id}).toPromise();
    }
    if (this.data.type === RequestType.album) {
      await this.requestService.approveAlbum({id: this.data.id}).toPromise();
    }

    this.bottomSheetRef.dismiss({type: UpdateType.Approve});
    return;
  }

  public async changeAvailability() {
    if (this.data.type === RequestType.movie) {
      await this.requestService.markMovieAvailable({id: this.data.id}).toPromise();
    }
    if (this.data.type === RequestType.album) {
      await this.requestService.markAlbumAvailable({id: this.data.id}).toPromise();
    }

    this.bottomSheetRef.dismiss({type: UpdateType.Availability});
    return;
  }
}