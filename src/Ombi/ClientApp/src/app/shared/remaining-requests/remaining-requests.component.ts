import { Component, Input, OnInit } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { RequestType } from "../../interfaces";
import { IRemainingRequests } from "../../interfaces/IRemainingRequests";
import { RequestService } from "../../services";
@Component({
    selector: "app-remaining-requests",
    templateUrl: "remaining-requests.component.html",
    styles: [`.mat-icon {
        vertical-align: middle;
     }`],
})
export class RemainingRequestsComponent implements OnInit {

    @Input() type: RequestType;
    public RequestType = RequestType;
    public remaining: IRemainingRequests;
    public daysUntil: number;
    public hoursUntil: number;
    public minutesUntil: number;
    public matIcon: string;

    constructor(private requestService: RequestService,
                private translate: TranslateService) { }

    public ngOnInit(): void {
        this.start();
    }

    public getTooltipContent() : string {
        if (this.daysUntil > 1) {
            return this.translate.instant('Requests.Remaining.NextDays', { time: this.daysUntil});
        }
        if (this.hoursUntil > 1 && this.daysUntil <= 1) {
            return this.translate.instant('Requests.Remaining.NextHours', { time: this.hoursUntil});
        }
        if (this.minutesUntil >= 1 && this.hoursUntil <= 1 && this.daysUntil <= 1) {
            return this.minutesUntil == 1
            ? this.translate.instant('Requests.Remaining.NextMinute', { time: this.minutesUntil})
            : this.translate.instant('Requests.Remaining.NextMinutes', { time: this.minutesUntil});
        }
    }

    private start() {

        const callback = (remaining => {
            this.remaining = remaining;
            if (this.remaining && this.remaining.hasLimit) {
                this.calculateTime();
            }
        });

        switch (this.type) {
            case RequestType.movie:
                this.requestService.getRemainingMovieRequests().subscribe(callback);
                this.matIcon = "movie";

                break;
            case RequestType.tvShow:
                this.requestService.getRemainingTvRequests().subscribe(callback);
                this.matIcon = "tv";

                break;
            case RequestType.album:
                this.requestService.getRemainingMusicRequests().subscribe(callback);
                this.matIcon = "library_music";

                break;
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
