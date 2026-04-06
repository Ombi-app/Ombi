import { Component, OnInit, OnDestroy } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        RouterModule
    ],
    selector: "settings-menu",
    templateUrl: "./settingsmenu.component.html",
    styleUrls: ["./settingsmenu.component.scss"]
})
export class SettingsMenuComponent implements OnInit, OnDestroy {

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
