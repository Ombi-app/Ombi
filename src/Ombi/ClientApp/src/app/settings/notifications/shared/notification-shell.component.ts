import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, UntypedFormGroup } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatDividerModule } from "@angular/material/divider";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";

import { INotificationTemplates } from "../../../interfaces";
import { NotificationTemplate } from "../notificationtemplate.component";

@Component({
    selector: "ombi-notification-shell",
    standalone: true,
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCardModule,
        MatDividerModule,
        MatIconModule,
        MatProgressBarModule,
        MatProgressSpinnerModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        NotificationTemplate,
    ],
    templateUrl: "./notification-shell.component.html",
    styleUrls: ["./notification-shell.component.scss"],
})
export class NotificationShellComponent {
    @Input({ required: true }) providerName!: string;
    @Input() icon: string = "notifications";
    @Input() description?: string;
    @Input() docsUrl?: string;
    @Input() docsLabel: string = "Setup guide";

    @Input() form: UntypedFormGroup | null = null;
    @Input() loading: boolean = false;

    @Input() templates: INotificationTemplates[] | null = null;
    @Input() showTemplates: boolean = true;
    @Input() showSubject: boolean = false;

    @Input() showEnabledToggle: boolean = true;
    @Input() enabledControlName: string = "enabled";

    @Input() submitLabel: string = "Save changes";
    @Input() testLabel: string = "Send test";
    @Input() showTest: boolean = true;
    @Input() requireDirtyToSave: boolean = true;

    @Output() readonly submitted = new EventEmitter<UntypedFormGroup>();
    @Output() readonly tested = new EventEmitter<UntypedFormGroup>();

    public onSubmit(): void {
        if (this.form) {
            this.submitted.emit(this.form);
        }
    }

    public onTest(): void {
        if (this.form) {
            this.tested.emit(this.form);
        }
    }

    public get isEnabled(): boolean {
        return !!this.form?.get(this.enabledControlName)?.value;
    }
}
