import { Pipe, PipeTransform } from "@angular/core";
import { FormatPipe } from 'ngx-date-fns';

@Pipe({
    name: "ombiDate",
})
export class OmbiDatePipe implements PipeTransform  {

    constructor(
        private FormatPipe: FormatPipe,
      ) {}

    public transform(value: string, format: string ) {
        const date = new Date(value);
        return this.FormatPipe.transform(date, format);
    }
}
