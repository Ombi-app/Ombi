import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'quality' })
export class QualityPipe implements PipeTransform {
    transform(value: string): string {
        if (value.toUpperCase() === "4K" || value.toUpperCase() === "8K") {
            return value;
        }
        return value + "p";
    }
}