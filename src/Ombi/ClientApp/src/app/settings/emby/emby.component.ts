import { Component, OnInit } from "@angular/core";
import {
  JobService,
  NotificationService,
  SettingsService,
} from "../../services";
import { IEmbyServer, IEmbySettings } from "../../interfaces";
import { MatSlideToggleChange } from "@angular/material/slide-toggle";
import { MatDialog } from "@angular/material/dialog";
import {
  EmbyServerDialog,
  EmbyServerDialogData,
} from "./emby-server-dialog/emby-server-dialog.component";

@Component({
  templateUrl: "./emby.component.html",
  styleUrls: ["./emby.component.scss"],
})
export class EmbyComponent implements OnInit {
  public savedSettings: IEmbySettings;

  constructor(
    private settingsService: SettingsService,
    private notificationService: NotificationService,
    private jobService: JobService,
    private dialog: MatDialog
  ) {}

  public ngOnInit() {
    this.settingsService.getEmby().subscribe({
      next: (result) => {
        if (result.servers == null) result.servers = [];
        this.savedSettings = result;
      },
      error: () => {
        this.notificationService.error("Failed to retrieve Emby settings.");
      },
    });
  }

  public toggleEnableFlag(event: MatSlideToggleChange) {
    const newSettings: IEmbySettings = {
      ...structuredClone(this.savedSettings),
      enable: event.checked,
    };
    const errorMessage = event.checked
      ? "There was an error enabling Emby settings. Check that all servers are configured correctly."
      : "There was an error disabling Emby settings.";
    this.settingsService.saveEmby(newSettings).subscribe({
      next: (result) => {
        if (result) {
          this.savedSettings.enable = event.checked;
          this.notificationService.success(
            `Successfully ${
              event.checked ? "enabled" : "disabled"
            } Emby settings.`
          );
        } else {
          event.source.checked = !event.checked;
          this.notificationService.error(errorMessage);
        }
      },
      error: () => {
        event.source.checked = !event.checked;
        this.notificationService.error(errorMessage);
      },
    });
  }

  public newServer() {
    const newServer: IEmbyServer = {
      name: "",
      id: Math.floor(Math.random() * 99999 + 1),
      apiKey: "",
      administratorId: "",
      enableEpisodeSearching: false,
      ip: "",
      port: undefined,
      ssl: false,
      subDir: "",
      serverId: "",
      serverHostname: "",
      embySelectedLibraries: [],
    };
    const data: EmbyServerDialogData = {
      server: newServer,
      isNewServer: true,
      savedSettings: this.savedSettings,
    };
    const dialog = this.dialog.open(EmbyServerDialog, {
      width: "700px",
      data: data,
      panelClass: "modal-panel",
    });
    dialog.afterClosed().subscribe((x) => {
      return console.log(x);
    });
  }

  public editServer(server: IEmbyServer) {
    const data: EmbyServerDialogData = {
      server: server,
      isNewServer: false,
      savedSettings: this.savedSettings,
    };
    const dialog = this.dialog.open(EmbyServerDialog, {
      width: "700px",
      data: data,
      panelClass: "modal-panel",
    });
    dialog.afterClosed().subscribe((x) => {
      console.log(server);
      return console.log(x);
    });
  }

  public runIncrementalSync(): void {
    const errorMessage = "There was an error triggering the incremental sync.";
    this.jobService.runEmbyRecentlyAddedCacher().subscribe({
      next: (result) => {
        if (result) {
          this.notificationService.success("Triggered the incremental sync.");
        } else {
          this.notificationService.error(errorMessage);
        }
      },
      error: () => {
        this.notificationService.error(errorMessage);
      },
    });
  }

  public runFullSync(): void {
    const errorMessage = "There was an error triggering the full sync.";
    this.jobService.runEmbyCacher().subscribe({
      next: (result) => {
        if (result) {
          this.notificationService.success("Triggered the full sync.");
        } else {
          this.notificationService.error(errorMessage);
        }
      },
      error: () => {
        this.notificationService.error(errorMessage);
      },
    });
  }

  public runCacheWipeWithFullSync(): void {
    const errorMessage =
      "There was an error triggering the cache wipe with a full sync.";
    this.jobService.clearMediaserverData().subscribe({
      next: (result) => {
        if (result) {
          this.notificationService.success(
            "Triggered the cache wipe with a full sync."
          );
        } else {
          this.notificationService.error(errorMessage);
        }
      },
      error: () => {
        this.notificationService.error(errorMessage);
      },
    });
  }
}
