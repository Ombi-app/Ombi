import { MatPaginatorIntl } from '@angular/material/paginator';
import { TranslateService } from '@ngx-translate/core';

export class MatPaginatorI18n {

    constructor(private translate: TranslateService) { }

    getPaginatorIntl(): MatPaginatorIntl {
        const paginatorIntl = new MatPaginatorIntl();
        paginatorIntl.itemsPerPageLabel = this.translate.instant('Paginator.itemsPerPageLabel');
        paginatorIntl.nextPageLabel = this.translate.instant('Paginator.nextPageLabel');
        paginatorIntl.previousPageLabel = this.translate.instant('Paginator.previousPageLabel');
        paginatorIntl.firstPageLabel = this.translate.instant('Paginator.firstPageLabel');
        paginatorIntl.lastPageLabel = this.translate.instant('Paginator.lastPageLabel');
        paginatorIntl.getRangeLabel = this.getRangeLabel.bind(this);
        return paginatorIntl;
    }

    private getRangeLabel(page: number, pageSize: number, length: number): string {
        if (length == 0 || pageSize == 0) {
            return this.translate.instant('Paginator.rangePageLabel1', { length });
        }

        length = Math.max(length, 0);

        const startIndex = page * pageSize;

        // If the start index exceeds the list length, do not try and fix the end index to the end.
        const endIndex =
            startIndex < length ? Math.min(startIndex + pageSize, length) : startIndex + pageSize;

        return this.translate.instant('Paginator.rangePageLabel2', { startIndex: startIndex + 1, endIndex, length });
    }
}