import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
    standalone: true,
    name: "humanize",
})
export class HumanizePipe implements PipeTransform  {
    public transform(value: any): any {
        if ((typeof value) !== "string" || !value) {
            return value;
        }
        let str = value as string;
        str = str.split(/(?=[A-Z])/).join(" ");
        str = str[0].toUpperCase() + str.slice(1);
        return str;
    }
}
