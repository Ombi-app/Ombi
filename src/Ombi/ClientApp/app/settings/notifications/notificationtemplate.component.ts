import { Component, Input } from "@angular/core";

import { INotificationTemplates, NotificationType } from "../../interfaces";

@Component({
    selector:"notification-templates",
    templateUrl: "./notificationtemplate.component.html",
})
export class NotificationTemplate {
    @Input() public templates: INotificationTemplates[];
    public NotificationType = NotificationType;

    public helpText = `
{RequestedUser} : The User who requested the content <br/>
{RequestedDate} : The Date the media was requested <br/>
{Title} : The title of the request e.g. Lion King <br/>
{Type} : The request type e.g. Movie/Tv Show <br/>
{Overview} : Overview of the requested item <br/>
{Year} : The release year of the request<br/>
{EpisodesList} : A comma seperated list of Episodes requested<br/>
{SeasonsList} : A comma seperated list of seasons requested<br/>
{PosterImage} : The requested poster image link<br/>
{ApplicationName} : The Application Name from the Customization Settings
{LongDate} : 15 June 2017 <br/>
{ShortDate} : 15/06/2017 <br/>
{LongTime} : 16:02:34 <br/>
{ShortTime} : 16:02 <br/>

`;
}
