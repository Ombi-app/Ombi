import { Pipe, PipeTransform } from '@angular/core';
import { orderBy as _orderBy } from 'lodash';

@Pipe({
	name: 'orderBy',
})
export class OrderPipe<T> implements PipeTransform {
	transform(data: T[], orderBy: string, direction: 'asc' | 'desc' = 'asc'): T[] {
		return _orderBy(data, orderBy, direction);
	}
}
