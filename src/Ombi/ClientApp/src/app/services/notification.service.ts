import { Injectable } from "@angular/core";
import { MatSnackBar, MatSnackBarConfig } from "@angular/material/snack-bar";

@Injectable()
export class NotificationService {
    constructor(private snackbar: MatSnackBar) { }

    private config: MatSnackBarConfig<any> = {
        duration:3000,

    }

    public success(body: string) {
        this.snackbar.open(body, "OK", this.config);
    }

    public info(title: string, body: string) {
        this.snackbar.open(body, "OK", this.config);
    }

    public warning(title: string, body: string) {
        this.snackbar.open(body, "OK", this.config);
    }

    public error(body: string) {
        this.snackbar.open(body, "OK", this.config);
    }
}
