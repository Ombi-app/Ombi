import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { MatExpansionModule } from "@angular/material/expansion";
import { TranslateModule } from "@ngx-translate/core";

import { INotificationTemplates, NotificationType } from "../../interfaces";
import { WikiComponent } from "../wiki.component";
import { HumanizePipe } from "../../pipes/standalone-pipes";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatExpansionModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        WikiComponent,
        HumanizePipe
    ],
    selector:"notification-templates",
    templateUrl: "./notificationtemplate.component.html",
    styleUrls: ["./notificationtemplate.component.scss"],
})
export class NotificationTemplate {
    @Input() public templates: INotificationTemplates[];
    @Input() public showSubject = true; // True by default
    public NotificationType = NotificationType;
}
