import { MatPaginatorIntl } from '@angular/material/paginator';
import { TranslateService } from '@ngx-translate/core';
import { merge } from 'rxjs';

export class MatPaginatorI18n {

    constructor(private translate: TranslateService) { }

    getPaginatorIntl(): MatPaginatorIntl {
        const paginatorIntl = new MatPaginatorIntl();
        paginatorIntl.getRangeLabel = this.getRangeLabel.bind(this);

        this.applyLabels(paginatorIntl);

        // The translations are fetched over HTTP, so the labels above can be read
        // before that request has come back, and translate.instant() then hands us
        // the key itself. Re-read them whenever the translations land or the
        // language changes, and emit on `changes` so the paginator re-renders.
        merge(
            this.translate.onTranslationChange,
            this.translate.onLangChange,
            this.translate.onDefaultLangChange,
        ).subscribe(() => this.applyLabels(paginatorIntl));

        return paginatorIntl;
    }

    private applyLabels(paginatorIntl: MatPaginatorIntl): void {
        paginatorIntl.itemsPerPageLabel = this.translate.instant('Paginator.itemsPerPageLabel');
        paginatorIntl.nextPageLabel = this.translate.instant('Paginator.nextPageLabel');
        paginatorIntl.previousPageLabel = this.translate.instant('Paginator.previousPageLabel');
        paginatorIntl.firstPageLabel = this.translate.instant('Paginator.firstPageLabel');
        paginatorIntl.lastPageLabel = this.translate.instant('Paginator.lastPageLabel');
        paginatorIntl.changes.next();
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
