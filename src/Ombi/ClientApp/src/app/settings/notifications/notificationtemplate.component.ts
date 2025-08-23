import { Component, Input } from "@angular/core";

import { INotificationTemplates, NotificationType } from "../../interfaces";

@Component({
        standalone: false,
    selector:"notification-templates",
    templateUrl: "./notificationtemplate.component.html",
    styleUrls: ["./notificationtemplate.component.scss"],
})
export class NotificationTemplate {
    @Input() public templates: INotificationTemplates[];
    @Input() public showSubject = true; // True by default
    public NotificationType = NotificationType;
}
