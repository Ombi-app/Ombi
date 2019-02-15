import { Injectable } from "@angular/core";
import { MatSnackBar, MatSnackBarConfig } from "@angular/material";

@Injectable()
export class MessageService {
    constructor(private snackBar: MatSnackBar) { 
        this.config = {
            duration: 4000,
        }
    }
    private config: MatSnackBarConfig;


    public send(message: string, action?: string) {
        this.snackBar.open(message, action.toUpperCase(), this.config)
    }
}
