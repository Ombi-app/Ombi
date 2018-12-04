import { Component, OnInit } from "@angular/core";
import { IFailedRequestsViewModel, RequestType } from "../../interfaces";
import { RequestRetryService } from "../../services";

@Component({
    templateUrl: "./failedrequest.component.html",
})
export class FailedRequestsComponent implements OnInit {

    public vm: IFailedRequestsViewModel[];
    public RequestType = RequestType;

    constructor(private retry: RequestRetryService) { }

    public ngOnInit() {
        this.retry.getFailedRequests().subscribe(x => this.vm = x);
    }

    public remove(failedId: number) {
        this.retry.deleteFailedRequest(failedId).subscribe();
    }
}
