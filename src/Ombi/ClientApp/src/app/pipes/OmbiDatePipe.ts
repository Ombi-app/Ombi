import { Pipe, PipeTransform } from "@angular/core";
import { FormatPipe } from 'ngx-date-fns';
import { parseISO, format } from 'date-fns';

@Pipe({
    name: "ombiDate",
})
export class OmbiDatePipe implements PipeTransform  {

    constructor(
        private FormatPipe: FormatPipe,
      ) {}

    public transform(value: string, formatStr: string ) {
        if (!value) {
            return '';
        }
        
        // Parse the ISO string as UTC
        const utcDate = parseISO(value);
        
        // Format the date using date-fns format function
        // This will automatically handle the UTC to local conversion
        return format(utcDate, formatStr);
    }
}
