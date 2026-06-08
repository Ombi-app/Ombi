import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { SettingsMenuComponent } from "./settingsmenu.component";
import { OutletAttachDirective } from "../shared/outlet-attach.directive";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        SettingsMenuComponent,
        OutletAttachDirective
    ],
    template: `
        <settings-menu></settings-menu>
        <router-outlet></router-outlet>
    `,
    styleUrls: ["./settings.component.scss"]
})
export class SettingsComponent {
}
