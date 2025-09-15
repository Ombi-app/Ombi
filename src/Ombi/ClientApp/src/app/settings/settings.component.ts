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
        <settings-menu></settings-menu>
        <router-outlet></router-outlet>
    `,
    styleUrls: ["./settings.component.scss"]
})
export class SettingsComponent {
}
