import { Component, Input } from "@angular/core";
import { FormGroup } from "@angular/forms";
import { TesterService, NotificationService } from "../../../services";
import { INotificationTemplates, NotificationType } from "../../../interfaces";



@Component({
    templateUrl: "./whatsapp.component.html",
    selector: "app-whatsapp"
})
export class WhatsAppComponent {

    public NotificationType = NotificationType;
    @Input() public templates: INotificationTemplates[];
    @Input() public form: FormGroup;

    constructor(private testerService: TesterService,
                private notificationService: NotificationService) { }


    public test(form: FormGroup) {
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
