import { Component, Inject } from '@angular/core';
import { MAT_BOTTOM_SHEET_DATA, MatBottomSheetRef } from '@angular/material/bottom-sheet';
import { RequestService } from '../../../services';
import { RequestType } from '../../../interfaces';
import { UpdateType } from '../../models/UpdateType';

@Component({
  selector: 'request-options',
  templateUrl: './request-options.component.html',
})
export class RequestOptionsComponent {

  public RequestType = RequestType;

  constructor(@Inject(MAT_BOTTOM_SHEET_DATA) public data: any,
    private requestService: RequestService, private bottomSheetRef: MatBottomSheetRef<RequestOptionsComponent>) { }

  public async delete() {
    if (this.data.type === RequestType.movie) {
      await this.requestService.removeMovieRequestAsync(this.data.id);
    }
    if (this.data.type === RequestType.tvShow) {
      await this.requestService.deleteChild(this.data.id).toPromise();
    }
    if (this.data.type === RequestType.album) {
      await this.requestService.removeAlbumRequest(this.data.id).toPromise();
    }

    this.bottomSheetRef.dismiss({type: UpdateType.Delete});
    return;
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