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
        const textKey = 'Requests.ErrorCodes.' + result.errorCode;
        var text = this.translate.instant(textKey);
        if (text === textKey) { // Error code on backend may not exist in frontend
            if (result.errorMessage || result.message) {
                text = result.errorMessage ? result.errorMessage : result.message;
            } else {
                text = this.translate.instant('ErrorPages.SomethingWentWrong');
            }
        }

        this.send(text, action);
    }
}
