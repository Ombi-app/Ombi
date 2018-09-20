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
    @Input() public tv: boolean;
    @Input() public music: boolean;
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
        } 
        if(this.tv) {
            this.requestService.getRemainingTvRequests().subscribe(callback);
        }
        if(this.music) {
            this.requestService.getRemainingMusicRequests().subscribe(callback);
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
