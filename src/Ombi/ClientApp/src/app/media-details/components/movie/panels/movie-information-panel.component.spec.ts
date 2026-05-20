import { describe, it, expect, vi } from 'vitest';
import { MovieInformationPanelComponent } from './movie-information-panel.component';
import { of } from 'rxjs';

function createComponent(baseHref = '/') {
  const mockSearchService = {
    getMovieStreams: vi.fn().mockReturnValue(of([{ provider: 'Netflix' }])),
  };
  const comp = new MovieInformationPanelComponent(mockSearchService as any, baseHref);
  comp.movie = { id: 550, title: 'Fight Club', releaseDate: '1999-10-15' } as any;
  comp.request = {} as any;
  return { comp, mockSearchService };
}

describe('MovieInformationPanelComponent', () => {
  it('should set baseUrl when href > 1 char', () => {
    const { comp } = createComponent('/ombi');
    comp.ngOnInit();
    expect(comp.baseUrl).toBe('/ombi');
  });

  it('should not set baseUrl for root href', () => {
    const { comp } = createComponent('/');
    comp.ngOnInit();
    expect(comp.baseUrl).toBeUndefined();
  });

  it('should load movie streams on init', () => {
    const { comp, mockSearchService } = createComponent();
    comp.ngOnInit();
    expect(mockSearchService.getMovieStreams).toHaveBeenCalledWith(550);
    expect(comp.streams).toHaveLength(1);
  });

  describe('getStatus', () => {
    it('should return RequestDenied for denied movie', () => {
      const { comp } = createComponent();
      expect(comp.getStatus({ available: false, requested: true, denied: true } as any)).toBe('Common.RequestDenied');
    });

    it('should return ProcessingRequest for approved movie', () => {
      const { comp } = createComponent();
      expect(comp.getStatus({ available: false, requested: true, denied: false, approved: true } as any)).toBe('Common.ProcessingRequest');
    });

    it('should return PendingApproval for requested but not approved', () => {
      const { comp } = createComponent();
      expect(comp.getStatus({ available: false, requested: true, denied: false, approved: false } as any)).toBe('Common.PendingApproval');
    });

    it('should return RequestDenied4K for denied 4K', () => {
      const { comp } = createComponent();
      expect(comp.getStatus({ available: true, requested: false, available4K: false, has4KRequest: true, denied4K: true } as any)).toBe('Common.RequestDenied4K');
    });

    it('should return ProcessingRequest4K for approved 4K', () => {
      const { comp } = createComponent();
      expect(comp.getStatus({ available: true, requested: false, available4K: false, has4KRequest: true, denied4K: false, approved4K: true } as any)).toBe('Common.ProcessingRequest4K');
    });

    it('should return PendingApproval4K for pending 4K', () => {
      const { comp } = createComponent();
      expect(comp.getStatus({ available: true, requested: false, available4K: false, has4KRequest: true, denied4K: false, approved4K: false } as any)).toBe('Common.PendingApproval4K');
    });
  });
});
