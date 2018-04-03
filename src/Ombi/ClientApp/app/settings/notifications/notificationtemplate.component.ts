import { Component, Input } from "@angular/core";

import { INotificationTemplates, NotificationType } from "../../interfaces";

@Component({
    selector:"notification-templates",
    templateUrl: "./notificationtemplate.component.html",
})
export class NotificationTemplate {
    @Input() public templates: INotificationTemplates[];
    @Input() public showSubject = true; // True by default
    @Input() public showAdvancedOptions = false;
    public NotificationType = NotificationType;
}
