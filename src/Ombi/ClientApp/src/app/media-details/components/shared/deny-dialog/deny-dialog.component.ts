import { Component, Inject } from "@angular/core";
import { IDenyDialogData } from "../interfaces/interfaces";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { RequestService, MessageService } from "../../../../services";
import { TranslateService } from "@ngx-translate/core";
import { RequestType, IRequestEngineResult } from "../../../../interfaces";

@Component({
    selector: "deny-dialog",
    templateUrl: "./deny-dialog.component.html",
})
export class DenyDialogComponent {
    constructor(
        public dialogRef: MatDialogRef<DenyDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: IDenyDialogData,
        private requestService: RequestService,
        public messageService: MessageService,
        private translate: TranslateService) {}

        public denyReason: string;

        public async deny() {
            let result: IRequestEngineResult;
            if(this.data.requestType == RequestType.movie) {
                result = await this.requestService.denyMovie({id: this.data.requestId, reason: this.denyReason }).toPromise();
            }
            if(this.data.requestType == RequestType.tvShow) {
                result = await this.requestService.denyChild({id: this.data.requestId, reason: this.denyReason }).toPromise();
            }
            if(this.data.requestType == RequestType.artist) {
                result = await this.requestService.denyAlbum({id: this.data.requestId, reason: this.denyReason }).toPromise();
            }

            if (result.result) {
                this.messageService.send(this.translate.instant("Requests.DeniedRequest"), "Ok");
                this.data.denied = true;
            } else {
                this.messageService.sendRequestEngineResultError(result);
                this.data.denied = false;
            }

            this.dialogRef.close({denied: this.data.denied, reason: this.denyReason});
        }

        onNoClick(): void {
            this.dialogRef.close();
            this.data.denied = false;
          }
}
