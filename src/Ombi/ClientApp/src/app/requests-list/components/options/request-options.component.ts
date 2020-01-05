import {Component, Inject} from '@angular/core';
import {MAT_BOTTOM_SHEET_DATA, MatBottomSheetRef} from '@angular/material/bottom-sheet';
import { RequestService } from '../../../services';
import { RequestType } from '../../../interfaces';

@Component({
  selector: 'request-options',
  templateUrl: './request-options.component.html',
})
export class RequestOptionsComponent {
  constructor(@Inject(MAT_BOTTOM_SHEET_DATA) public data: any,
            private requestService: RequestService, private bottomSheetRef: MatBottomSheetRef<RequestOptionsComponent>) { }

  public async delete() {
    if (this.data.type === RequestType.movie) {
        await this.requestService.removeMovieRequestAsync(this.data.id);
    }
    if(this.data.type === RequestType.tvShow) {
        await this.requestService.deleteChild(this.data.id).toPromise();
    }

    this.bottomSheetRef.dismiss(true);
    return;
  }
}