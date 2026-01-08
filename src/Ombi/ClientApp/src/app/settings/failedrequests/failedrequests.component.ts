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
import { IFailedRequestsViewModel, RequestType } from "../../interfaces";
import { SettingsMenuComponent } from "../settingsmenu.component";
import { WikiComponent } from "../wiki.component";
import { RequestRetryService } from "../../services";
import { MatTableModule } from "@angular/material/table";

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
        TranslateModule,
        MatTableModule
    ],
    providers: [
        RequestRetryService
    ],
    templateUrl: "./failedrequests.component.html",
    styleUrls: ["./failedrequests.component.scss"],
})
export class FailedRequestsComponent implements OnInit {

    public columnsToDisplay = ["title", "type", "retryCount", "errorDescription", "deleteBtn"];
    public vm: IFailedRequestsViewModel[];
    public RequestType = RequestType;

    constructor(private retry: RequestRetryService) { }

    public ngOnInit() {
        this.retry.getFailedRequests().subscribe(x => this.vm = x);
    }

    public remove(failed: IFailedRequestsViewModel) {
        this.retry.deleteFailedRequest(failed.failedId).subscribe(x => {
            if(x) {
                const index = this.vm.indexOf(failed);
                this.vm.splice(index,1);
            }
        });
    }
}
