import { describe, it, expect, vi, beforeEach } from 'vitest';
import { RemainingRequestsComponent } from './remaining-requests.component';
import { RequestType } from '../../interfaces';
import { of } from 'rxjs';

function createComponent(type: RequestType) {
  const mockRequestService = {
    getRemainingMovieRequests: vi.fn().mockReturnValue(of({ hasLimit: false, limit: 0, remaining: 0, nextRequest: new Date().toISOString() })),
    getRemainingTvRequests: vi.fn().mockReturnValue(of({ hasLimit: false, limit: 0, remaining: 0, nextRequest: new Date().toISOString() })),
    getRemainingMusicRequests: vi.fn().mockReturnValue(of({ hasLimit: false, limit: 0, remaining: 0, nextRequest: new Date().toISOString() })),
  } as any;

  const mockTranslate = {
    instant: vi.fn((key: string, params?: any) => {
      if (params) return `${key}:${JSON.stringify(params)}`;
      return key;
    }),
  } as any;

  const comp = new RemainingRequestsComponent(mockRequestService, mockTranslate);
  comp.type = type;

  return { comp, mockRequestService, mockTranslate };
}

describe('RemainingRequestsComponent', () => {
  describe('ngOnInit', () => {
    it('should call getRemainingMovieRequests for movie type', () => {
      const { comp, mockRequestService } = createComponent(RequestType.movie);
      comp.ngOnInit();
      expect(mockRequestService.getRemainingMovieRequests).toHaveBeenCalled();
      expect(comp.matIcon).toBe('fas fa-film');
    });

    it('should call getRemainingTvRequests for tvShow type', () => {
      const { comp, mockRequestService } = createComponent(RequestType.tvShow);
      comp.ngOnInit();
      expect(mockRequestService.getRemainingTvRequests).toHaveBeenCalled();
      expect(comp.matIcon).toBe('fas fa-tv');
    });

    it('should call getRemainingMusicRequests for album type', () => {
      const { comp, mockRequestService } = createComponent(RequestType.album);
      comp.ngOnInit();
      expect(mockRequestService.getRemainingMusicRequests).toHaveBeenCalled();
      expect(comp.matIcon).toBe('fas fa-music');
    });
  });

  describe('calculateTime with limits', () => {
    it('should calculate days, hours, and minutes until next request', () => {
      const { comp, mockRequestService } = createComponent(RequestType.movie);
      const future = new Date(Date.now() + 2 * 24 * 60 * 60 * 1000); // 2 days from now
      mockRequestService.getRemainingMovieRequests.mockReturnValue(of({
        hasLimit: true, limit: 5, remaining: 2, nextRequest: future.toISOString(),
      }));

      comp.ngOnInit();
      expect(comp.remaining).toBeDefined();
      expect(comp.daysUntil).toBeGreaterThanOrEqual(1);
      expect(comp.hoursUntil).toBeGreaterThanOrEqual(1);
      expect(comp.minutesUntil).toBeGreaterThanOrEqual(1);
    });
  });

  describe('getTooltipContent', () => {
    it('should return days message when daysUntil > 1', () => {
      const { comp, mockTranslate } = createComponent(RequestType.movie);
      comp.daysUntil = 3;
      comp.hoursUntil = 72;
      comp.minutesUntil = 4320;

      comp.getTooltipContent();
      expect(mockTranslate.instant).toHaveBeenCalledWith('Requests.Remaining.NextDays', { time: 3 });
    });

    it('should return hours message when hoursUntil > 1 and daysUntil <= 1', () => {
      const { comp, mockTranslate } = createComponent(RequestType.movie);
      comp.daysUntil = 1;
      comp.hoursUntil = 12;
      comp.minutesUntil = 720;

      comp.getTooltipContent();
      expect(mockTranslate.instant).toHaveBeenCalledWith('Requests.Remaining.NextHours', { time: 12 });
    });

    it('should return minutes message when minutesUntil >= 1 and hoursUntil <= 1', () => {
      const { comp, mockTranslate } = createComponent(RequestType.movie);
      comp.daysUntil = 0;
      comp.hoursUntil = 0;
      comp.minutesUntil = 30;

      comp.getTooltipContent();
      expect(mockTranslate.instant).toHaveBeenCalledWith('Requests.Remaining.NextMinutes', { time: 30 });
    });

    it('should return singular minute message when minutesUntil == 1', () => {
      const { comp, mockTranslate } = createComponent(RequestType.movie);
      comp.daysUntil = 0;
      comp.hoursUntil = 0;
      comp.minutesUntil = 1;

      comp.getTooltipContent();
      expect(mockTranslate.instant).toHaveBeenCalledWith('Requests.Remaining.NextMinute', { time: 1 });
    });
  });
});
