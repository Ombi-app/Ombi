import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { SettingsMenuComponent } from "./settingsmenu.component";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        SettingsMenuComponent
    ],
    template: `
        <div class="settings-layout">
            <settings-menu></settings-menu>
            <div class="settings-content">
                <router-outlet></router-outlet>
            </div>
        </div>
    `,
    styleUrls: ["./settings.component.scss"]
})
export class SettingsComponent {
}
