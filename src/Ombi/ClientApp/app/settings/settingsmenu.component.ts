import { Component } from "@angular/core";
@Component({
    selector: "settings-menu",
    templateUrl: "./settingsmenu.component.html",
})
export class SettingsMenuComponent {
    public ignore(event: any): void {
        event.preventDefault();
    }
 }
