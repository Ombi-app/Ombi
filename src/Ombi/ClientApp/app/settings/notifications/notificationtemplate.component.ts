import { Component, Input } from '@angular/core';

import { INotificationTemplates, NotificationType } from '../../interfaces/INotifcationSettings';

@Component({
    selector:'notification-templates',
    templateUrl: './notificationtemplate.component.html',
})
export class NotificationTemplate {
    @Input() templates: INotificationTemplates[];  
    NotificationType = NotificationType;
}