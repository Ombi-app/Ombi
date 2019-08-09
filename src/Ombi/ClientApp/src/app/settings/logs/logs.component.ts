
import { Component, OnInit } from "@angular/core";
import { SystemService } from "../../services";

@Component({
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
