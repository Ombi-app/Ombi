import { Component, Inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from "@angular/material/dialog";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatButtonModule } from "@angular/material/button";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";

import {
  PlexService,
  NotificationService,
  TesterService,
} from "../../../../services";
import { take } from "rxjs";
import { IPlexLibrariesSettings } from "../../../../interfaces";
import { PlexServerDialogData } from "../models";

@Component({
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSlideToggleModule
  ],
  selector: "plex-server-dialog-component",
  templateUrl: "plex-server-dialog.component.html",
  styleUrls: ["plex-server-dialog.component.scss"],
})
export class PlexServerDialogComponent {

    public password: string;
    public username: string;

  constructor(
    private dialogRef: MatDialogRef<PlexServerDialogData>,
    @Inject(MAT_DIALOG_DATA) public data: PlexServerDialogData,
    private notificationService: NotificationService,
    private testerService: TesterService,
    private plexService: PlexService
  ) {
  }


  public cancel() {
    this.dialogRef.close({closed: true});
  }

  public testPlex() {
    this.testerService.plexTest(this.data.server).pipe(take(1))
        .subscribe(x => {
        if (x) {
            this.notificationService.success(`Successfully connected to the Plex server ${this.data.server.name}!`);
        } else {
            this.notificationService.error(`We could not connect to the Plex server  ${this.data.server.name}!`);
        }
    });
}

  public delete() {
    this.dialogRef.close({deleted: true});
  }

  public save() {
    this.dialogRef.close({server: this.data.server});
  }

  public loadLibraries() {
        if (this.data.server.ip == null) {
            this.notificationService.error("Plex is not yet configured correctly");
            return;
        }
        this.plexService.getLibraries(this.data.server).subscribe(x => {
            this.data.server.plexSelectedLibraries = [];
            if (x.successful) {
                x.data.mediaContainer.directory.forEach((item) => {
                    const lib: IPlexLibrariesSettings = {
                        key: item.key,
                        title: item.title,
                        enabled: false,
                    };
                    this.data.server.plexSelectedLibraries.push(lib);
                });
            } else {
                this.notificationService.error(x.message);
            }
        });
    }

}
