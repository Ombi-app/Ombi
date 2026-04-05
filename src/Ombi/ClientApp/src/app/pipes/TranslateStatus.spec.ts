import { describe, it, expect, vi } from 'vitest';
import { TranslateStatusPipe } from './TranslateStatus';

describe('TranslateStatusPipe', () => {
  function createPipe(translations: Record<string, string> = {}) {
    const mockTranslate = {
      instant: vi.fn((key: string) => translations[key] ?? key),
    };
    const pipe = Object.create(TranslateStatusPipe.prototype);
    pipe.translateService = mockTranslate;
    return { pipe: pipe as TranslateStatusPipe, mockTranslate };
  }

  it('should return translated value when translation exists', () => {
    const { pipe } = createPipe({
      'MediaDetails.StatusValues.Released': 'Released',
    });
    expect(pipe.transform('Released')).toBe('Released');
  });

  it('should return original value when no translation exists', () => {
    const { pipe } = createPipe({});
    expect(pipe.transform('UnknownStatus')).toBe('UnknownStatus');
  });

  it('should construct the correct translation key', () => {
    const { pipe, mockTranslate } = createPipe({});
    pipe.transform('InProduction');
    expect(mockTranslate.instant).toHaveBeenCalledWith('MediaDetails.StatusValues.InProduction');
  });

  it('should return translated text when key resolves to different string', () => {
    const { pipe } = createPipe({
      'MediaDetails.StatusValues.Ended': 'Terminado',
    });
    expect(pipe.transform('Ended')).toBe('Terminado');
  });
});
