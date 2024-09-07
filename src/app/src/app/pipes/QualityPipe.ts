import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'quality' })
export class QualityPipe implements PipeTransform {
    transform(value: string): string {
        if (value.toUpperCase() === "4K" || value.toUpperCase() === "8K") {
            return value;
        }
        if (value[value.length - 1].toUpperCase() === "P") {
            return value;
        }
        return value + "p";
    }
}