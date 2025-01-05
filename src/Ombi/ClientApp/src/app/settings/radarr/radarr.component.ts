import { Component, OnDestroy, OnInit, QueryList, ViewChildren } from "@angular/core";
import { UntypedFormBuilder, UntypedFormGroup } from "@angular/forms";
import { RadarrFacade } from "app/state/radarr";

import { IMinimumAvailability, IRadarrCombined, IRadarrProfile, IRadarrRootFolder } from "../../interfaces";
import { NotificationService } from "../../services";
import { FeaturesFacade } from "../../state/features/features.facade";
import { RadarrFormComponent } from "./components/radarr-form.component";
import { Observable, ReplaySubject, Subject, combineLatest, map, switchMap, takeUntil, tap } from "rxjs";

@Component({
    templateUrl: "./radarr.component.html",
    styleUrls: ["./radarr.component.scss"]
})
export class RadarrComponent implements OnInit, OnDestroy {

    public qualities: IRadarrProfile[];
    public rootFolders: IRadarrRootFolder[];
    public minimumAvailabilityOptions: IMinimumAvailability[];
    public profilesRunning: boolean;
    public rootFoldersRunning: boolean;
    public is4kEnabled: boolean = false;

    public readonly form$: Observable<UntypedFormGroup>;

    private readonly form4k$: ReplaySubject<QueryList<RadarrFormComponent>>;
    private readonly normalForm$: ReplaySubject<QueryList<RadarrFormComponent>>;
    private readonly destroyed$: Subject<void>;

    constructor(
        private readonly radarrFacade: RadarrFacade,
        private readonly notificationService: NotificationService,
        private readonly featureFacade: FeaturesFacade,
        readonly fb: UntypedFormBuilder
    ) {
        this.form4k$ = new ReplaySubject();
        this.normalForm$ = new ReplaySubject();
        this.destroyed$ = new Subject();

        this.form$ = radarrFacade.state$()
            .pipe(
                map(x => fb.group({
                    radarr: fb.group({
                        enabled: [x.settings.radarr.enabled],
                        apiKey: [x.settings.radarr.apiKey],
                        defaultQualityProfile: [+x.settings.radarr.defaultQualityProfile],
                        defaultRootPath: [x.settings.radarr.defaultRootPath],
                        tag: [x.settings.radarr.tag],
                        sendUserTags: [x.settings.radarr.sendUserTags],
                        ssl: [x.settings.radarr.ssl],
                        subDir: [x.settings.radarr.subDir],
                        ip: [x.settings.radarr.ip],
                        port: [x.settings.radarr.port],
                        addOnly: [x.settings.radarr.addOnly],
                        minimumAvailability: [x.settings.radarr.minimumAvailability],
                        scanForAvailability: [x.settings.radarr.scanForAvailability]
                    }),
                    radarr4K: fb.group({
                        enabled: [x.settings.radarr4K.enabled],
                        apiKey: [x.settings.radarr4K.apiKey],
                        defaultQualityProfile: [+x.settings.radarr4K.defaultQualityProfile],
                        defaultRootPath: [x.settings.radarr4K.defaultRootPath],
                        tag: [x.settings.radarr4K.tag],
                        sendUserTags: [x.settings.radarr4K.sendUserTags],
                        ssl: [x.settings.radarr4K.ssl],
                        subDir: [x.settings.radarr4K.subDir],
                        ip: [x.settings.radarr4K.ip],
                        port: [x.settings.radarr4K.port],
                        addOnly: [x.settings.radarr4K.addOnly],
                        minimumAvailability: [x.settings.radarr4K.minimumAvailability],
                        scanForAvailability: [x.settings.radarr4K.scanForAvailability]
                    }),
                }))
            )
    }

    @ViewChildren('4kForm')
    protected set form4k(component: QueryList<RadarrFormComponent>) {
        this.form4k$.next(component);
    }

    @ViewChildren('normalForm')
    protected set normalForm(component: QueryList<RadarrFormComponent>) {
        this.normalForm$.next(component);
    }

    public ngOnInit() {
        this.is4kEnabled = this.featureFacade.is4kEnabled();

        combineLatest([this.form$, this.normalForm$])
            .pipe(
                switchMap(([, normalForm]) => normalForm.changes),
                tap(comp => comp.first.toggleValidators()),
                takeUntil(this.destroyed$)
            ).subscribe();

        if (this.is4kEnabled) {
            combineLatest([this.form$, this.form4k$])
                .pipe(
                    switchMap(([, form4k]) => form4k.changes),
                    tap(comp => comp.first.toggleValidators()),
                    takeUntil(this.destroyed$)
                ).subscribe();
        }
    }

    public ngOnDestroy(): void {
        this.destroyed$.next();
        this.destroyed$.complete();
    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        const radarrForm = form.controls.radarr as UntypedFormGroup;
        const radarr4KForm = form.controls.radarr4K as UntypedFormGroup;

        if (radarrForm.controls.enabled.value && (radarrForm.controls.defaultQualityProfile.value === -1
            || radarrForm.controls.defaultRootPath.value === "Please Select")) {
            this.notificationService.error("Please check your entered values for Radarr");
            return;
        }
        if (radarr4KForm.controls.enabled.value && (radarr4KForm.controls.defaultQualityProfile.value === -1
            || radarr4KForm.controls.defaultRootPath.value === "Please Select")) {
            this.notificationService.error("Please check your entered values for Radarr 4K");
            return;
        }

        if (radarr4KForm.controls.tag.value === -1) {
            radarr4KForm.controls.tag.setValue(null);
        }
        if (radarrForm.controls.tag.value === -1) {
            radarr4KForm.controls.tag.setValue(null);
        }

        const settings = <IRadarrCombined> form.value;
        this.radarrFacade.updateSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Radarr settings");
            } else {
                this.notificationService.success("There was an error when saving the Radarr settings");
            }
        });

    }
}
