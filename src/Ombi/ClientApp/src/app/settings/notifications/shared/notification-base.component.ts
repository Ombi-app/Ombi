import { Directive, OnDestroy, OnInit } from "@angular/core";
import { UntypedFormBuilder, UntypedFormGroup } from "@angular/forms";
import { Observable, Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";

import { INotificationSettings, INotificationTemplates, NotificationType } from "../../../interfaces";
import { NotificationService, SettingsService, TesterService } from "../../../services";

type NotificationPayload<T> = T & { notificationTemplates?: INotificationTemplates[] };

@Directive()
export abstract class NotificationBaseComponent<T extends INotificationSettings>
    implements OnInit, OnDestroy {

    public readonly NotificationType = NotificationType;
    public templates: INotificationTemplates[] = [];
    public form!: UntypedFormGroup;
    public loading = true;

    private readonly destroy$ = new Subject<void>();

    protected constructor(
        protected readonly settingsService: SettingsService,
        protected readonly notificationService: NotificationService,
        protected readonly fb: UntypedFormBuilder,
        protected readonly testerService: TesterService,
    ) { }

    protected abstract readonly providerName: string;
    protected abstract loadSettings(): Observable<T>;
    protected abstract saveSettings(settings: T): Observable<boolean>;
    protected abstract testSettings(settings: T): Observable<boolean>;
    protected abstract buildForm(settings: T): { [key: string]: unknown };

    protected validate(_settings: T): string | null { return null; }
    protected onFormReady(_form: UntypedFormGroup, _settings: T): void { /* no-op */ }
    protected attachTemplates: boolean = true;

    protected get savedMessage(): string {
        return `Successfully saved the ${this.providerName} settings`;
    }
    protected get saveErrorMessage(): string {
        return `There was an error when saving the ${this.providerName} settings`;
    }
    protected get testSuccessMessage(): string {
        return `Successfully sent a ${this.providerName} message`;
    }
    protected get testErrorMessage(): string {
        return `There was an error when sending the ${this.providerName} message. Please check your settings`;
    }

    public ngOnInit(): void {
        this.loadSettings()
            .pipe(takeUntil(this.destroy$))
            .subscribe(settings => {
                const payload = settings as NotificationPayload<T>;
                this.templates = payload?.notificationTemplates ?? [];
                this.form = this.fb.group(this.buildForm(settings));
                this.onFormReady(this.form, settings);
                this.loading = false;
            });
    }

    public ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    public onSubmit(form: UntypedFormGroup = this.form): void {
        if (!form || form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = this.materialise(form);
        const validationError = this.validate(settings);
        if (validationError) {
            this.notificationService.error(validationError);
            return;
        }

        this.saveSettings(settings)
            .pipe(takeUntil(this.destroy$))
            .subscribe(ok => {
                if (ok) {
                    this.notificationService.success(this.savedMessage);
                } else {
                    this.notificationService.success(this.saveErrorMessage);
                }
            });
    }

    public test(form: UntypedFormGroup = this.form): void {
        if (!form || form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = form.value as T;
        const validationError = this.validate(settings);
        if (validationError) {
            this.notificationService.error(validationError);
            return;
        }

        this.testSettings(settings)
            .pipe(takeUntil(this.destroy$))
            .subscribe(ok => {
                if (ok) {
                    this.notificationService.success(this.testSuccessMessage);
                } else {
                    this.notificationService.error(this.testErrorMessage);
                }
            });
    }

    private materialise(form: UntypedFormGroup): T {
        const settings = form.value as NotificationPayload<T>;
        if (this.attachTemplates) {
            settings.notificationTemplates = this.templates;
        }
        return settings as T;
    }
}
