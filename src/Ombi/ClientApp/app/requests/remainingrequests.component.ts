import { IRemainingRequests } from "../interfaces/IRemainingRequests";
import { RequestService } from "../services";

import { Component, Input, OnInit } from "@angular/core";
import { Observable } from "rxjs";

@Component({
    selector: "remaining-requests",
    templateUrl: "./remainingrequests.component.html",
})

export class RemainingRequestsComponent implements OnInit  {
    public remaining: IRemainingRequests;
    @Input() public movie: boolean;
    public daysUntil: number;
    public hoursUntil: number;
    public minutesUntil: number;
    @Input() public quotaRefreshEvents: Observable<void>;

    constructor(private requestService: RequestService) {
    }

    public ngOnInit() {
        const self = this;

        this.update();

        this.quotaRefreshEvents.subscribe(() => {
            this.update();
        });

        setInterval(() => {
            self.update();
        }, 60000);
    }

    public update(): void {
        const callback = (remaining => {
            this.remaining = remaining;
            this.calculateTime();
        });

        if (this.movie) {
            this.requestService.getRemainingMovieRequests().subscribe(callback);
        } else {
            this.requestService.getRemainingTvRequests().subscribe(callback);
        }
    }

    private calculateTime(): void {
        this.daysUntil = Math.ceil(this.daysUntilNextRequest());
        this.hoursUntil = Math.ceil(this.hoursUntilNextRequest());
        this.minutesUntil = Math.ceil(this.minutesUntilNextRequest());
    }

    private daysUntilNextRequest(): number {
        return (new Date(this.remaining.nextRequest).getTime() - new Date().getTime()) / 1000 / 60 / 60 / 24;
    }

    private hoursUntilNextRequest(): number {
        return (new Date(this.remaining.nextRequest).getTime() - new Date().getTime()) / 1000 / 60 / 60;
    }

    private minutesUntilNextRequest(): number {
        return (new Date(this.remaining.nextRequest).getTime() - new Date().getTime()) / 1000 / 60;
    }
}
