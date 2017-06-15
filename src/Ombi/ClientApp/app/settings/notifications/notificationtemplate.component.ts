import { Component, Input } from '@angular/core';

import { INotificationTemplates, NotificationType } from '../../interfaces/INotifcationSettings';

@Component({
    selector:'notification-templates',
    templateUrl: './notificationtemplate.component.html',
})
export class NotificationTemplate {
    @Input() templates: INotificationTemplates[];  
    NotificationType = NotificationType;

    helpText = `
{RequestedUser} : The User who requested the content <br/>
{RequestedDate} : The Date the media was requested <br/>
{Title} : The title of the request e.g. Lion King <br/>
{Type} : The request type e.g. Movie/Tv Show <br/>
{LongDate} : 15 June 2017 <br/>
{ShortDate} : 15/06/2017 <br/>
{LongTime} : 16:02:34 <br/>
{ShortTime} : 16:02 <br/>

`
}