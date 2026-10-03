import { describe, it, expect, beforeEach } from 'vitest';
import { Subject } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { MatPaginatorI18n } from './MatPaginatorI18n';

const LABELS: Record<string, string> = {
  'Paginator.itemsPerPageLabel': 'Items per page:',
  'Paginator.nextPageLabel': 'Next page',
  'Paginator.previousPageLabel': 'Previous page',
  'Paginator.firstPageLabel': 'First page',
  'Paginator.lastPageLabel': 'Last page',
};

/**
 * Stands in for TranslateService. The behaviour that matters here is the one that
 * causes the bug: until the translation file has been fetched, instant() returns
 * the key it was given rather than a translated string.
 */
function createTranslateStub() {
  const onTranslationChange = new Subject<any>();
  const onLangChange = new Subject<any>();
  const onDefaultLangChange = new Subject<any>();
  let loaded = false;

  const stub = {
    onTranslationChange,
    onLangChange,
    onDefaultLangChange,
    instant: (key: string, params?: any) => {
      if (!loaded) {
        return key;
      }
      return LABELS[key] ?? key;
    },
    /** Mimics the translation file arriving over HTTP. */
    completeLoad: () => {
      loaded = true;
      onTranslationChange.next({ lang: 'en' });
    },
    switchLanguage: () => {
      loaded = true;
      onLangChange.next({ lang: 'fr' });
    },
  };

  return stub;
}

describe('MatPaginatorI18n', () => {
  let translate: ReturnType<typeof createTranslateStub>;

  beforeEach(() => {
    translate = createTranslateStub();
  });

  const build = () => new MatPaginatorI18n(translate as unknown as TranslateService).getPaginatorIntl();

  it('falls back to the key while the translations are still loading', () => {
    const intl = build();

    expect(intl.itemsPerPageLabel).toBe('Paginator.itemsPerPageLabel');
  });

  it('replaces the keys once the translations arrive', () => {
    const intl = build();

    translate.completeLoad();

    expect(intl.itemsPerPageLabel).toBe('Items per page:');
    expect(intl.nextPageLabel).toBe('Next page');
    expect(intl.previousPageLabel).toBe('Previous page');
    expect(intl.firstPageLabel).toBe('First page');
    expect(intl.lastPageLabel).toBe('Last page');
  });

  it('notifies the paginator so it re-renders the new labels', () => {
    const intl = build();
    let emissions = 0;
    intl.changes.subscribe(() => emissions++);

    translate.completeLoad();

    expect(emissions).toBeGreaterThan(0);
  });

  it('updates the labels when the language changes', () => {
    const intl = build();

    translate.switchLanguage();

    expect(intl.itemsPerPageLabel).toBe('Items per page:');
  });

  it('still builds the range label from the translations', () => {
    const intl = build();
    translate.completeLoad();

    expect(intl.getRangeLabel(0, 10, 0)).toBe('Paginator.rangePageLabel1');
    expect(intl.getRangeLabel(1, 10, 35)).toBe('Paginator.rangePageLabel2');
  });
});
