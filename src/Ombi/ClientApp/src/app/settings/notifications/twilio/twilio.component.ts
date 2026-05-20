import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatIconModule } from "@angular/material/icon";
import { MatTabsModule } from "@angular/material/tabs";
import { TranslateModule } from "@ngx-translate/core";

import { WhatsAppComponent } from "./whatsapp.component";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        MatIconModule,
        MatTabsModule,
        TranslateModule,
        WhatsAppComponent,
    ],
    templateUrl: "./twilio.component.html",
    styleUrls: ["./twilio.component.scss"],
})
export class TwilioComponent {
}
