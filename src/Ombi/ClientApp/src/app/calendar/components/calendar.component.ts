import { Component, OnInit } from "@angular/core";

import { CalendarService } from "../../services/calendar.service";
import { ICalendarModel } from "../../interfaces/ICalendar";

@Component({
    templateUrl: "./calendar.component.html",
    styleUrls: ["./calendar.component.scss"],
})
export class CalendarComponent implements OnInit {

    public loadingFlag: boolean;
    events: any[];
    options: any;
    entries: ICalendarModel[];

    constructor(private calendarService: CalendarService) { }

    public async ngOnInit() {
        this.loading()
        this.entries = await this.calendarService.getCalendarEntries();

        this.options = {
            defaultDate: new Date(),
            header: {
                left: 'prev,next',
                center: 'title',
                right: 'agendaWeek,month'
            },
            eventClick: (e: any) => {
                e.preventDefault();
            }
        };
        this.finishLoading();
    }

    private loading() {
        this.loadingFlag = true;
    }

    private finishLoading() {
        this.loadingFlag = false;
    }
}
