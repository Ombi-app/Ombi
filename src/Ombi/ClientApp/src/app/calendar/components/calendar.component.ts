import { Component, OnInit } from "@angular/core";

import { CalendarService } from "../../services/calendar.service";
import { ICalendarModel } from "../../interfaces/ICalendar";

import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';

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
        this.options = {
            plugins: [dayGridPlugin, interactionPlugin],
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

        this.entries = await this.calendarService.getCalendarEntries();
        this.finishLoading();
    }

    private loading() {
        this.loadingFlag = true;
    }

    private finishLoading() {
        this.loadingFlag = false;
    }
}
