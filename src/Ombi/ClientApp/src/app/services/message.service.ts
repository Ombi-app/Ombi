import { Injectable } from "@angular/core";
import { MatSnackBar, MatSnackBarConfig } from "@angular/material/snack-bar";
import { TranslateService } from "@ngx-translate/core";
import { IRequestEngineResult } from "../interfaces/IRequestEngineResult";

@Injectable()
export class MessageService {
    constructor(private snackBar: MatSnackBar, private translate: TranslateService) {
        this.config = {
            duration: 4000,
        }
    }
    private config: MatSnackBarConfig;


    public send(message: string, action?: string) {
        if (action) {
            this.snackBar.open(message, action.toUpperCase(), this.config)
        } else {
            this.snackBar.open(message, "OK", this.config)
        }
    }
    public sendRequestEngineResultError(result: IRequestEngineResult, action: string = "Ok") {
        console.log(result.errorCode);
        const textKey = 'Requests.ErrorCodes.' + result.errorCode;
        const text = this.translate.instant(textKey);
        if (text !== textKey) {
            this.send(text, action);
        } else {
            this.send(result.errorMessage ? result.errorMessage : result.message, action);
        }
    }
}
