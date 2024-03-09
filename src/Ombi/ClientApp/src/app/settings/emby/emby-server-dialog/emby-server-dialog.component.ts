import { Component, Inject, ViewChild } from "@angular/core";
import { NgForm } from "@angular/forms";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import {
  IEmbyLibrariesSettings,
  IEmbyServer,
  IEmbySettings,
} from "app/interfaces";
import {
  EmbyService,
  NotificationService,
  SettingsService,
  TesterService,
} from "app/services";
import { isEqual } from "lodash";

export interface EmbyServerDialogData {
  server: IEmbyServer;
  isNewServer: boolean;
  savedSettings: IEmbySettings;
}

@Component({
  selector: "emby-server-dialog-component",
  templateUrl: "emby-server-dialog.component.html",
  styleUrls: ["emby-server-dialog.component.scss"],
})
export class EmbyServerDialog {
  @ViewChild("embyServerForm") embyServerForm: NgForm;
  public server: IEmbyServer;
  public isServerNameMissing: boolean;
  public isChangeDetected: boolean;
  public serverDiscoveryRequired: boolean;
  private validatedFields: {
    ip: string;
    port: number;
    ssl: boolean;
    apiKey: string;
    subDir: string;
  };

  constructor(
    private dialogRef: MatDialogRef<EmbyServerDialog>,
    @Inject(MAT_DIALOG_DATA) public data: EmbyServerDialogData,
    private notificationService: NotificationService,
    private settingsService: SettingsService,
    private testerService: TesterService,
    private embyService: EmbyService
  ) {
    this.server = structuredClone(data.server);
    this.isChangeDetected = false;
    this.serverDiscoveryRequired = data.isNewServer;
    this.isServerNameMissing = !this.server.name;
    this.validatedFields = {
      ip: this.server.ip,
      port: this.server.port,
      ssl: this.server.ssl,
      apiKey: this.server.apiKey,
      subDir: this.server.subDir,
    };
  }

  public processChangeEvent() {
    if (
      this.validatedFields.ip !== this.server.ip ||
      this.validatedFields.port?.toString() !== this.server.port?.toString() ||
      this.validatedFields.ssl !== this.server.ssl ||
      this.validatedFields.apiKey !== this.server.apiKey ||
      this.validatedFields.subDir !== this.server.subDir ||
      !this.embyServerForm.valid
    ) {
      this.serverDiscoveryRequired = true;
    } else {
      this.serverDiscoveryRequired = false;
    }

    this.isServerNameMissing = !this.server.name;
    this.isChangeDetected = !isEqual(this.data.server, this.server);
  }

  public cancel() {
    this.dialogRef.close();
  }

  public delete() {
    const settings: IEmbySettings = structuredClone(this.data.savedSettings);
    const index = settings.servers.findIndex((i) => i.id === this.server.id);
    if (index == -1) return;

    settings.servers.splice(index, 1);
    const errorMessage = "There was an error removing the server.";
    this.settingsService.saveEmby(settings).subscribe({
      next: (result) => {
        if (result) {
          this.data.savedSettings.servers.splice(index, 1);
          this.dialogRef.close();
        } else {
          this.notificationService.error(errorMessage);
        }
      },
      error: () => {
        this.notificationService.error(errorMessage);
      },
    });
  }

  public save() {
    const settings: IEmbySettings = structuredClone(this.data.savedSettings);
    if (this.data.isNewServer) {
      settings.servers.push(this.server);
    } else {
      const index = settings.servers.findIndex((i) => i.id === this.server.id);
      if (index !== -1) settings.servers[index] = this.server;
    }

    const errorMessage = "There was an error saving the server.";
    this.settingsService.saveEmby(settings).subscribe({
      next: (result) => {
        if (result) {
          if (this.data.isNewServer) {
            this.data.savedSettings.servers.push(this.server);
          } else {
            const index = this.data.savedSettings.servers.findIndex(
              (i) => i.id === this.server.id
            );
            if (index !== -1)
              this.data.savedSettings.servers[index] = this.server;
          }
          this.dialogRef.close();
        } else {
          this.notificationService.error(errorMessage);
        }
      },
      error: () => {
        this.notificationService.error(errorMessage);
      },
    });
  }

  public discoverServer() {
    this.embyServerForm.form.markAllAsTouched();
    if (!this.embyServerForm.valid) return;

    const errorMessage = `Failed to connect to the server. Make sure configuration is correct.`;
    this.testerService.embyTest(this.server).subscribe({
      next: (result) => {
        if (!result) {
          this.notificationService.error(errorMessage);
          return;
        }

        this.retrieveServerNameAndId(this.server);
      },
      error: () => {
        this.notificationService.error(errorMessage);
      },
    });
  }

  private retrieveServerNameAndId(server: IEmbyServer) {
    const errorMessage =
      "Failed to discover server. Make sure configuration is correct.";
    this.embyService.getPublicInfo(server).subscribe({
      next: (result) => {
        if (!result) {
          this.notificationService.error(errorMessage);
          return;
        }

        if (!server.name) {
          server.name = result.serverName;
          this.isServerNameMissing = false;
        }
        server.serverId = result.id;
        this.loadLibraries(server);
      },
      error: () => {
        this.notificationService.error(errorMessage);
      },
    });
  }

  private loadLibraries(server: IEmbyServer) {
    this.embyService.getLibraries(server).subscribe({
      next: (result) => {
        server.embySelectedLibraries = result.items.map((item) => {
          const index = server.embySelectedLibraries.findIndex(
            (x) => x.key == item.id
          );
          const enabled =
            index === -1 ? false : server.embySelectedLibraries[index].enabled;
          const lib: IEmbyLibrariesSettings = {
            key: item.id,
            title: item.name,
            enabled: enabled,
            collectionType: item.collectionType,
          };
          return lib;
        });

        this.serverDiscoveryRequired = false;
        this.validatedFields = {
          ip: this.server.ip,
          port: this.server.port,
          ssl: this.server.ssl,
          apiKey: this.server.apiKey,
          subDir: this.server.subDir,
        };
        this.notificationService.success("Successfully discovered the server.");
      },
      error: () => {
        const errorMessage = "There was an error retrieving libraries.";
        this.notificationService.error(errorMessage);
      },
    });
  }
}
