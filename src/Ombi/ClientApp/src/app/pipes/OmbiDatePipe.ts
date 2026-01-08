import { Pipe, PipeTransform } from "@angular/core";
import { parseISO, format } from 'date-fns';

@Pipe({
    standalone: true,
    name: "ombiDate",
})
export class OmbiDatePipe implements PipeTransform  {

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
