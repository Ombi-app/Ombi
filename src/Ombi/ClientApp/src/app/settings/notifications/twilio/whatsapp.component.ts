import { Component, Input } from "@angular/core";
import { ReactiveFormsModule, UntypedFormGroup } from "@angular/forms";
import { TesterService, NotificationService } from "../../../services";
import { INotificationTemplates, NotificationType } from "../../../interfaces";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { NotificationTemplate } from "../notificationtemplate.component";
import { MatTabsModule } from "@angular/material/tabs";



@Component({
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatTabsModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        NotificationTemplate
    ],
    templateUrl: "./whatsapp.component.html",
    selector: "app-whatsapp"
})
export class WhatsAppComponent {

    public NotificationType = NotificationType;
    @Input() public templates: INotificationTemplates[];
    @Input() public form: UntypedFormGroup;

    constructor(private testerService: TesterService,
                private notificationService: NotificationService) { }


    public test(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.testerService.whatsAppTest(form.get("whatsAppSettings").value).subscribe(x => {
            if (x) {
                this.notificationService.success( "Successfully sent a WhatsApp message, please check the appropriate channel");
            } else {
                this.notificationService.error("There was an error when sending the WhatsApp message. Please check your settings");
            }
        });

    }
}
