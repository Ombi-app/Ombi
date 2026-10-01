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
import { TranslateModule, TranslateService } from "@ngx-translate/core";
import { finalize } from "rxjs/operators";
import { IFailedRequestsViewModel, RequestType } from "../../interfaces";
import { MessageService, RequestRetryService } from "../../services";
import { MatTableModule } from "@angular/material/table";
import { HumanizePipe } from "../../pipes/standalone-pipes";

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
        MatTableModule,
        HumanizePipe
    ],
    providers: [
        RequestRetryService
    ],
    templateUrl: "./failedrequests.component.html",
    styleUrls: ["./failedrequests.component.scss"],
})
export class FailedRequestsComponent implements OnInit {

    public columnsToDisplay = ["title", "type", "retryCount", "errorDescription", "reprocessBtn", "deleteBtn"];
    public vm: IFailedRequestsViewModel[] = [];
    public RequestType = RequestType;
    public reprocessing = new Set<number>();

    constructor(
        private retry: RequestRetryService,
        private messageService: MessageService,
        private translateService: TranslateService
    ) { }

    public ngOnInit() {
        this.retry.getFailedRequests().subscribe(x => this.vm = x);
    }

    public remove(failed: IFailedRequestsViewModel) {
        this.retry.deleteFailedRequest(failed.failedId).subscribe(x => {
            if(x) {
                this.removeFromList(failed);
            }
        });
    }

    public reprocess(failed: IFailedRequestsViewModel) {
        if (this.reprocessing.has(failed.failedId)) {
            return;
        }

        this.reprocessing.add(failed.failedId);
        this.retry.retryFailedRequest(failed.failedId).pipe(
            finalize(() => this.reprocessing.delete(failed.failedId))
        ).subscribe({
            next: result => {
                if (result.result) {
                    this.removeFromList(failed);
                    this.messageService.send(this.translateService.instant("Requests.SuccessfullyReprocessed"));
                } else {
                    this.messageService.sendRequestEngineResultError(result);
                }
            },
            error: () => {
                this.messageService.send(this.translateService.instant("ErrorPages.SomethingWentWrong"));
            }
        });
    }

    public isReprocessing(failedId: number): boolean {
        return this.reprocessing.has(failedId);
    }

    private removeFromList(failed: IFailedRequestsViewModel) {
        const index = this.vm.indexOf(failed);
        if (index >= 0) {
            this.vm.splice(index, 1);
        }
    }
}
