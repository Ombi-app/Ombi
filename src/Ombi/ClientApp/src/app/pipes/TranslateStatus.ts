import { Pipe, PipeTransform } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Pipe({
    name: 'translateStatus'
  })
export class TranslateStatusPipe implements PipeTransform {
  constructor(private translateService: TranslateService) {}

  transform(value: string): string {
    const textKey = 'MediaDetails.StatusValues.' + value;
    const text = this.translateService.instant(textKey);
    if (text !== textKey) {
        return text;
    } else {
        return value;
    }
  }
}