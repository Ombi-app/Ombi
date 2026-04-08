import { describe, it, expect, vi, beforeEach } from 'vitest';
import { DetailedCardComponent } from './detailed-card.component';
import { RequestType, IRecentlyRequested } from '../../interfaces';
import { of, Subject } from 'rxjs';

function createComponent() {
  const mockImageService = {
    getMoviePoster: vi.fn().mockReturnValue(of('/poster.jpg')),
    getTmdbTvPoster: vi.fn().mockReturnValue(of('/tv-poster.jpg')),
    getMovieBackground: vi.fn().mockReturnValue(of('/bg.jpg')),
    getTmdbTvBackground: vi.fn().mockReturnValue(of('/tv-bg.jpg')),
  } as any;

  const mockSanitizer = {
    bypassSecurityTrustStyle: vi.fn((val: string) => `safe:${val}`),
  } as any;

  const comp = new DetailedCardComponent(mockImageService, mockSanitizer);
  return { comp, mockImageService, mockSanitizer };
}

function makeRequest(overrides: Partial<IRecentlyRequested> = {}): IRecentlyRequested {
  return {
    mediaId: '550',
    title: 'Test Movie',
    type: RequestType.movie,
    posterPath: null,
    background: null,
    denied: false,
    approved: false,
    available: false,
    tvPartiallyAvailable: false,
    ...overrides,
  } as IRecentlyRequested;
}

describe('DetailedCardComponent', () => {
  describe('getStatus', () => {
    it('should return Denied when request is denied', () => {
      const { comp } = createComponent();
      expect(comp.getStatus(makeRequest({ denied: true }))).toBe('Common.Denied');
    });

    it('should return Available when request is available', () => {
      const { comp } = createComponent();
      expect(comp.getStatus(makeRequest({ available: true }))).toBe('Common.Available');
    });

    it('should return PartiallyAvailable for partially available TV', () => {
      const { comp } = createComponent();
      expect(comp.getStatus(makeRequest({ tvPartiallyAvailable: true }))).toBe('Common.PartiallyAvailable');
    });

    it('should return Approved when request is approved', () => {
      const { comp } = createComponent();
      expect(comp.getStatus(makeRequest({ approved: true }))).toBe('Common.Approved');
    });

    it('should return Pending by default', () => {
      const { comp } = createComponent();
      expect(comp.getStatus(makeRequest())).toBe('Common.Pending');
    });

    it('should prioritize denied over available', () => {
      const { comp } = createComponent();
      expect(comp.getStatus(makeRequest({ denied: true, available: true }))).toBe('Common.Denied');
    });
  });

  describe('getClass', () => {
    it('should return danger for denied', () => {
      const { comp } = createComponent();
      expect(comp.getClass(makeRequest({ denied: true }))).toBe('danger');
    });

    it('should return success for available', () => {
      const { comp } = createComponent();
      expect(comp.getClass(makeRequest({ available: true }))).toBe('success');
    });

    it('should return success for partially available', () => {
      const { comp } = createComponent();
      expect(comp.getClass(makeRequest({ tvPartiallyAvailable: true }))).toBe('success');
    });

    it('should return primary for approved', () => {
      const { comp } = createComponent();
      expect(comp.getClass(makeRequest({ approved: true }))).toBe('primary');
    });

    it('should return info for pending', () => {
      const { comp } = createComponent();
      expect(comp.getClass(makeRequest())).toBe('info');
    });
  });

  describe('event emitters', () => {
    it('should emit onClick when click is called', () => {
      const { comp } = createComponent();
      const spy = vi.fn();
      comp.onClick.subscribe(spy);
      comp.click();
      expect(spy).toHaveBeenCalled();
    });

    it('should emit onApprove when approve is called', () => {
      const { comp } = createComponent();
      const spy = vi.fn();
      comp.onApprove.subscribe(spy);
      comp.approve();
      expect(spy).toHaveBeenCalled();
    });

    it('should emit onDeny when deny is called', () => {
      const { comp } = createComponent();
      const spy = vi.fn();
      comp.onDeny.subscribe(spy);
      comp.deny();
      expect(spy).toHaveBeenCalled();
    });

    it('should stop propagation and emit onApprove on onApproveClick', () => {
      const { comp } = createComponent();
      const spy = vi.fn();
      comp.onApprove.subscribe(spy);
      const event = { stopPropagation: vi.fn() } as any;
      comp.onApproveClick(event);
      expect(event.stopPropagation).toHaveBeenCalled();
      expect(spy).toHaveBeenCalled();
    });

    it('should stop propagation and emit onDeny on onDenyClick', () => {
      const { comp } = createComponent();
      const spy = vi.fn();
      comp.onDeny.subscribe(spy);
      const event = { stopPropagation: vi.fn() } as any;
      comp.onDenyClick(event);
      expect(event.stopPropagation).toHaveBeenCalled();
      expect(spy).toHaveBeenCalled();
    });

    it('should prevent default and emit click on onSpaceKey', () => {
      const { comp } = createComponent();
      const spy = vi.fn();
      comp.onClick.subscribe(spy);
      const event = { preventDefault: vi.fn() } as any;
      comp.onSpaceKey(event);
      expect(event.preventDefault).toHaveBeenCalled();
      expect(spy).toHaveBeenCalled();
    });
  });

  describe('ngOnInit', () => {
    it('should load movie poster from image service when posterPath is null', () => {
      const { comp, mockImageService } = createComponent();
      comp.request = makeRequest({ posterPath: null, type: RequestType.movie });
      comp.ngOnInit();
      expect(mockImageService.getMoviePoster).toHaveBeenCalledWith('550');
    });

    it('should load TV poster from image service when posterPath is null', () => {
      const { comp, mockImageService } = createComponent();
      comp.request = makeRequest({ posterPath: null, type: RequestType.tvShow, mediaId: '1396' });
      comp.ngOnInit();
      expect(mockImageService.getTmdbTvPoster).toHaveBeenCalledWith(1396);
    });

    it('should use existing posterPath when provided', () => {
      const { comp, mockImageService } = createComponent();
      comp.request = makeRequest({ posterPath: '/existing.jpg' });
      comp.ngOnInit();
      expect(mockImageService.getMoviePoster).not.toHaveBeenCalled();
    });
  });

  describe('ngOnDestroy', () => {
    it('should complete the image subscription subject', () => {
      const { comp } = createComponent();
      comp.request = makeRequest({ posterPath: '/existing.jpg', background: '/bg.jpg' });
      comp.ngOnInit();
      // Should not throw
      expect(() => comp.ngOnDestroy()).not.toThrow();
    });
  });
});
