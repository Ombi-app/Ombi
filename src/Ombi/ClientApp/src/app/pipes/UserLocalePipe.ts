import { Pipe, PipeTransform } from "@angular/core";
import * as moment from "moment";

const momentConstructor = moment;

@Pipe({ name: "amUserLocale" })
export class UserLocalePipe implements PipeTransform {
  transform(value: moment.MomentInput): moment.Moment {
    const locale = navigator.language;
    return momentConstructor(value).locale(locale);
  }
}
