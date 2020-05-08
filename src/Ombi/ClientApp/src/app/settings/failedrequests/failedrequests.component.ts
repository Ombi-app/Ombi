import { Component, OnInit } from "@angular/core";
import { IFailedRequestsViewModel, RequestType } from "../../interfaces";
import { RequestRetryService } from "../../services";

@Component({
    templateUrl: "./failedrequests.component.html",
    styleUrls: ["./failedrequests.component.scss"],
})
export class FailedRequestsComponent implements OnInit {

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
