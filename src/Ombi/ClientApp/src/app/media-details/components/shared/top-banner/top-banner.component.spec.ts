import { describe, it, expect, vi, beforeEach } from 'vitest';
import { TopBannerComponent } from './top-banner.component';

describe('TopBannerComponent', () => {
  function createComponent() {
    const mockSanitizer = {
      bypassSecurityTrustStyle: vi.fn((val: string) => `safe-style:${val}`),
    } as any;

    const comp = new TopBannerComponent(mockSanitizer);
    return { comp, mockSanitizer };
  }

  describe('releaseDateFormat', () => {
    it('should return the date when it is a valid date with year != 1', () => {
      const { comp } = createComponent();
      const date = new Date(2023, 5, 15);
      comp.releaseDate = date;
      expect(comp.releaseDateFormat).toBe(date);
    });

    it('should return null when releaseDate is falsy', () => {
      const { comp } = createComponent();
      comp.releaseDate = null as any;
      expect(comp.releaseDateFormat).toBeNull();
    });

    it('should return null when releaseDate year is 1 (default/min date)', () => {
      const { comp } = createComponent();
      const minDate = new Date();
      minDate.setFullYear(1, 0, 1);
      comp.releaseDate = minDate;
      expect(comp.releaseDateFormat).toBeNull();
    });

    it('should return null when releaseDate is not a Date instance', () => {
      const { comp } = createComponent();
      comp.releaseDate = '2023-06-15' as any;
      expect(comp.releaseDateFormat).toBeNull();
    });
  });

  describe('getBackgroundImage', () => {
    it('should call bypassSecurityTrustStyle with the background value', () => {
      const { comp, mockSanitizer } = createComponent();
      comp.background = 'url(https://image.tmdb.org/test.jpg)';
      const result = comp.getBackgroundImage();
      expect(mockSanitizer.bypassSecurityTrustStyle).toHaveBeenCalledWith('url(https://image.tmdb.org/test.jpg)');
      expect(result).toBe('safe-style:url(https://image.tmdb.org/test.jpg)');
    });
  });
});
