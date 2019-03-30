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
        debugger;
        this.loading()
        this.entries = await this.calendarService.getCalendarEntries();
        this.events = [
            {
                "title": "All Day Event",
                "start": new Date(),
                "eventColor":"black"
            },
            {
                "title": "Long Event",
                "start": "2016-01-07",
                "end": "2016-01-10"
            },
            {
                "title": "Repeating Event",
                "start": "2016-01-09T16:00:00"
            },
            {
                "title": "Repeating Event",
                "start": "2016-01-16T16:00:00"
            },
            {
                "title": "Conference",
                "start": "2016-01-11",
                "end": "2016-01-13"
            }
        ];

        this.options = {
            defaultDate: new Date(),
            header: {
                left: 'prev,next',
                center: 'title',
                right: 'month,agendaWeek'
            },
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
