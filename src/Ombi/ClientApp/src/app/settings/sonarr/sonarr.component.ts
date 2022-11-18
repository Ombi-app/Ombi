import { Component, OnInit } from "@angular/core";
import { UntypedFormBuilder, FormControl, UntypedFormGroup, Validators } from "@angular/forms";
import { finalize, map } from "rxjs";

import { ILanguageProfiles, ISonarrProfile, ISonarrRootFolder, ITag } from "../../interfaces";

import { ISonarrSettings } from "../../interfaces";
import { SonarrService } from "../../services";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./sonarr.component.html",
    styleUrls: ["./sonarr.component.scss"]
})
export class SonarrComponent implements OnInit {

    public qualities: ISonarrProfile[];
    public qualitiesAnime: ISonarrProfile[];
    public rootFolders: ISonarrRootFolder[];
    public rootFoldersAnime: ISonarrRootFolder[];
    public languageProfiles: ILanguageProfiles[];
    public languageProfilesAnime: ILanguageProfiles[];
    
    public tags: ITag[];
    public animeTags: ITag[];

    public selectedRootFolder: ISonarrRootFolder;
    public selectedQuality: ISonarrProfile;
    public selectedLanguageProfiles: ILanguageProfiles;
    public profilesRunning: boolean;
    public rootFoldersRunning: boolean;
    public tagsRunning: boolean;
    public langRunning: boolean;
    public form: UntypedFormGroup;
    public advanced = false;
    public sonarrVersion: string;
    formErrors: any;

    constructor(private settingsService: SettingsService,
                private sonarrService: SonarrService,
                private notificationService: NotificationService,
                private testerService: TesterService,
                private fb: UntypedFormBuilder){}

    onFormValuesChanged()
        {
            for ( const field in this.formErrors )
            {
                if ( !this.formErrors.hasOwnProperty(field) )
                {
                    continue;
                }
                // Clear previous errors
                this.formErrors[field] = {};
                // Get the control
                const control = this.form.get(field);
                if ( control && control.dirty && !control.valid && control.value === "Please Select")
                    {
                        this.formErrors[field] = control.errors;
                    }
            }
        }

    public ngOnInit() {
        this.settingsService.getSonarr()
            .subscribe(x => {
                this.form = this.fb.group({
                    enabled: [x.enabled],
                    apiKey: [x.apiKey, [Validators.required]],
                    qualityProfile: [x.qualityProfile, [Validators.required, validateProfile]],
                    rootPath: [x.rootPath, [Validators.required, validateProfile]],
                    qualityProfileAnime: [x.qualityProfileAnime],
                    rootPathAnime: [x.rootPathAnime],
                    ssl: [x.ssl],
                    subDir: [x.subDir],
                    ip: [x.ip, [Validators.required]],
                    port: [x.port, [Validators.required]],
                    addOnly: [x.addOnly],
                    seasonFolders: [x.seasonFolders],
                    languageProfile: [x.languageProfile],
                    languageProfileAnime: [x.languageProfileAnime],
                    scanForAvailability: [x.scanForAvailability],
                    sendUserTags: [x.sendUserTags],
                    tag: [x.tag],
                    animeTag: [x.animeTag]
                });

                this.rootFolders = [];
                this.qualities = [];
                this.languageProfiles = [];
                this.tags = [];
                this.animeTags = [];

                if (x.enabled && this.form.valid) {
                    this.testerService.sonarrTest(x).subscribe(result => {
                        this.sonarrVersion = result.version[0];
                        if (this.sonarrVersion === '3') {
                            this.form.controls.languageProfile.addValidators([Validators.required, validateProfile]);
                        }
                    });
                }

                if (x.qualityProfile) {
                    this.getProfiles(this.form);
                }
                if (x.rootPath) {
                    this.getRootFolders(this.form);
                }
                if (x.languageProfile) {
                    this.getLanguageProfiles(this.form);
                }
                if (x.tag || x.animeTag) {
                    this.getTags(this.form);
                }

                this.formErrors ={
                    apiKey: {},
                    qualityProfile: {},
                    rootPath: {},
                    ip: {},
                    port: {},
                };
                this.onFormValuesChanged();
            });

        this.rootFolders.push({ path: "Please Select", id: -1 });
        this.qualities.push({ name: "Please Select", id: -1 });
        this.languageProfiles.push({ name: "Please Select", id: -1 });
        this.animeTags.push({label: "None", id: -1});
        this.tags.push({label: "None", id: -1});
    }

    public getProfiles(form: UntypedFormGroup) {
        this.profilesRunning = true;
        this.sonarrService.getQualityProfiles(form.value)
            .subscribe(x => {
                this.qualities = x;
                this.qualitiesAnime = x;
                this.qualities.unshift({ name: "Please Select", id: -1 });
                this.profilesRunning = false;
                this.notificationService.success("Successfully retrieved the Quality Profiles");
            });
    }

    public getRootFolders(form: UntypedFormGroup) {
        this.rootFoldersRunning = true;
        this.sonarrService.getRootFolders(form.value)
            .subscribe(x => {
                this.rootFolders = x;
                this.rootFolders.unshift({ path: "Please Select", id: -1 });
                this.rootFoldersAnime = x;

                this.rootFoldersRunning = false;
                this.notificationService.success("Successfully retrieved the Root Folders");
            });
    }

    public getLanguageProfiles(form: UntypedFormGroup) {
        this.langRunning = true;
        this.sonarrService.getV3LanguageProfiles(form.value)
            .subscribe(x => {
                this.languageProfiles = x;
                this.languageProfilesAnime = x;

                this.langRunning = false;
                this.notificationService.success("Successfully retrieved the Language Profiles");
            });
    }

    public getTags(form: UntypedFormGroup) {
        this.tagsRunning = true;
        this.sonarrService.getTags(form.value).pipe(
            finalize(() => {
                this.tagsRunning = false;
                this.animeTags.unshift({ label: "None", id: -1 });
                this.tags.unshift({ label: "None", id: -1 });
                this.notificationService.success("Successfully retrieved the Tags");
            }),
            map(result => {
                this.tags = result;
                this.tags.forEach(val => this.animeTags.push(Object.assign({}, val)));
            })
        ).subscribe()
    }

    public test(form: UntypedFormGroup) {
        const settings = <ISonarrSettings> form.value;
        this.testerService.sonarrTest(settings).subscribe(result => {
            if (result.isValid) {
                this.sonarrVersion = result.version[0];
                this.notificationService.success("Successfully connected to Sonarr!");
            } else if (result.expectedSubDir) {
                this.notificationService.error("Your Sonarr Base URL must be set to " + result.expectedSubDir);
            } else {
                this.notificationService.error("We could not connect to Sonarr!");
            }
        });
    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        if (form.controls.defaultQualityProfile) {
            if (form.controls.defaultQualityProfile.value === "-1") {
                this.notificationService.error("Please check your entered values");
            }
        }
        if (form.controls.defaultRootPath) {
            if (form.controls.defaultRootPath.value === "Please Select") {
                this.notificationService.error("Please check your entered values");
            }
        }
        if (form.controls.languageProfile) {
            if (form.controls.languageProfile.value === "Please Select") {
                this.notificationService.error("Please check your entered values");
            }
        }
        if (form.controls.animeTag.value == -1) {
            form.controls.animeTag.setValue(null);
        }
        if (form.controls.tag.value == -1) {
            form.controls.tag.setValue(null);
        }

        this.settingsService.saveSonarr(form.value)
            .subscribe(x => {
                if (x) {
                    this.notificationService.success("Successfully saved Sonarr settings");
                } else {
                    this.notificationService.error("There was an error when saving the Sonarr settings");
                }
            });
    }
}

function validateProfile(qualityProfile): { [key: string]:boolean } | null {

    if (qualityProfile.value !== undefined && (isNaN(qualityProfile.value) || qualityProfile.value == -1)) {
        return { 'profileValidation': true };
    }
    return null;
}
