import { Component, OnInit, Input } from "@angular/core";
import { RequestService } from "../services";
import { IRemainingRequests } from "../interfaces/IRemainingRequests";

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

    constructor(private requestService: RequestService)
    {
    }

    ngOnInit(): void {
        var self = this;
        this.update();
        setInterval(function(){
            self.update()
        }, 10000)
    }

    update(): void {
        var callback = (remaining => {
            this.remaining = remaining;
            this.daysUntil = Math.ceil(this.daysUntilNextRequest());
            this.hoursUntil = Math.ceil(this.hoursUntilNextRequest());
            this.minutesUntil = Math.ceil(this.minutesUntilNextRequest())
        });

        if (this.movie) {
            this.requestService.getRemainingMovieRequests().subscribe(callback);
        } else {
            this.requestService.getRemainingTvRequests().subscribe(callback);
        }
    }

    daysUntilNextRequest(): number {
        return (new Date(this.remaining.nextRequest).getTime() - new Date().getTime()) / 1000 / 60 / 60 / 24;
    }

    hoursUntilNextRequest(): number {
        return (new Date(this.remaining.nextRequest).getTime() - new Date().getTime()) / 1000 / 60 / 60;
    }

    minutesUntilNextRequest(): number {
        return (new Date(this.remaining.nextRequest).getTime() - new Date().getTime()) / 1000 / 60;
    }
}