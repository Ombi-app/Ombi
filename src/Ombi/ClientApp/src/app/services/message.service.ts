import { Injectable } from "@angular/core";
import { MatSnackBar, MatSnackBarConfig } from "@angular/material/snack-bar";

@Injectable()
export class MessageService {
    constructor(private snackBar: MatSnackBar) {
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
}
