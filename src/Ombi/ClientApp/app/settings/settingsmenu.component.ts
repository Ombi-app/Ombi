import { Component } from "@angular/core";
@Component({
    selector: "settings-menu",
    templateUrl: "./settingsmenu.component.html",
})
export class SettingsMenuComponent {
    public ignore(event: any): void {
        event.preventDefault();
    }

    public ngOnInit() {
        const element = document.getElementById("settings");
        if (element != null) {
            element.classList.add("active");
        }
    }

    public ngOnDestroy() {
        const element = document.getElementById("settings");
        if (element != null) {
            element.classList.remove("active");
        }
    }

 }
