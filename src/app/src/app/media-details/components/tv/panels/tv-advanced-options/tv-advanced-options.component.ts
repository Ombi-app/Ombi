import { Component, Inject, OnInit } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import {
  IAdvancedData,
  ILanguageProfiles,
  ISonarrProfile,
  ISonarrRootFolder,
  ISonarrSettings,
} from "../../../../../interfaces";
import { SettingsService, SonarrService } from "../../../../../services";

@Component({
  templateUrl: "./tv-advanced-options.component.html",
  selector: "tv-advanced-options",
})
export class TvAdvancedOptionsComponent implements OnInit {
  public sonarrProfiles: ISonarrProfile[];
  public sonarrRootFolders: ISonarrRootFolder[];
  public sonarrLanguageProfiles: ILanguageProfiles[];
  public sonarrEnabled: boolean;

  constructor(
    public dialogRef: MatDialogRef<TvAdvancedOptionsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: IAdvancedData,
    private sonarrService: SonarrService,
    private settingsService: SettingsService
  ) {}

  public async ngOnInit() {
    this.settingsService.getSonarr().subscribe((settings: ISonarrSettings) => {
      if (!settings.enabled) {
          this.sonarrEnabled = false;
        return;
      }

      this.sonarrEnabled = true;
      this.sonarrService.getQualityProfilesWithoutSettings().subscribe((c) => {
        this.sonarrProfiles = c;
        this.data.profiles = c;
        this.setQualityOverrides();
      });
      this.sonarrService.getRootFoldersWithoutSettings().subscribe((c) => {
        this.sonarrRootFolders = c;
        this.data.rootFolders = c;
        this.setRootFolderOverrides();
      });

        this.sonarrService
          .getV3LanguageProfiles(settings)
          .subscribe((profiles: ILanguageProfiles[]) => {
            this.sonarrLanguageProfiles = profiles;
            this.data.languages = profiles;
            this.setLanguageOverride();
          });
    });
  }

  private setQualityOverrides(): void {
    if (this.sonarrProfiles) {
      const profile = this.sonarrProfiles.filter((p) => {
        return p.id === this.data.tvRequest.qualityOverride;
      });
      if (profile.length > 0) {
        this.data.tvRequest.qualityOverrideTitle = profile[0].name;
      }
    }
  }

  private setRootFolderOverrides(): void {
    if (this.sonarrRootFolders) {
      const path = this.sonarrRootFolders.filter((folder) => {
        return folder.id === this.data.tvRequest.rootFolder;
      });
      if (path.length > 0) {
        this.data.tvRequest.rootPathOverrideTitle = path[0].path;
      }
    }
  }

  private setLanguageOverride(): void {
    if (this.sonarrLanguageProfiles) {
      const profile = this.sonarrLanguageProfiles.filter((p) => {
        return p.id === this.data.tvRequest.languageProfile;
      });
      if (profile.length > 0) {
        this.data.tvRequest.languageOverrideTitle = profile[0].name;
      }
    }
  }
}
