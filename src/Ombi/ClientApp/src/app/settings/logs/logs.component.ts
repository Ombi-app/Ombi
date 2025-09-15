
import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { SystemService } from "../../services";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule
    ],
    templateUrl: "./logs.component.html",
    styleUrls:["./logs.component.scss"]
})
export class LogsComponent implements OnInit {

    public logs: string[];
    public logDetail: string;

    constructor(private readonly systemService: SystemService) { }

    public async ngOnInit() {
        this.logs = await this.systemService.getAvailableLogs().toPromise();
    }

    public async loadLog(logName: string) {
        this.logDetail = await this.systemService.getLog(logName).toPromise();
    }
}
