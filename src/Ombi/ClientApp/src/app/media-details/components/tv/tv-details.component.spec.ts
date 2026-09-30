import { beforeEach, describe, expect, it, vi } from 'vitest';
import { of } from 'rxjs';
import { TvDetailsComponent } from './tv-details.component';

function createComponent() {
  const mockSearchService = {
    getTvInfo: vi.fn().mockResolvedValue({
      id: 1396,
      requestId: 0,
      banner: null,
      images: { original: null },
      seasonRequests: [],
    }),
  };
  const mockRoute = {
    params: of({ tvdbId: 1396 }),
  };
  const mockSanitizer = {
    bypassSecurityTrustStyle: vi.fn((value: string) => value),
  };
  const mockDialog = {
    open: vi.fn(),
  };
  const mockMessageService = {
    send: vi.fn(),
  };
  const mockRequestService = {
    getChildRequests: vi.fn().mockReturnValue(of([])),
  };
  const mockRequestService2 = {};
  const mockAuth = {
    hasRole: vi.fn().mockReturnValue(false),
  };
  const mockSonarrService = {};
  const mockSonarrFacade = {
    isEnabled: vi.fn().mockReturnValue(false),
  };
  const mockSettingsState = {
    getIssue: vi.fn().mockReturnValue(false),
  };

  const component = new TvDetailsComponent(
    mockSearchService as any,
    mockRoute as any,
    mockSanitizer as any,
    mockDialog as any,
    mockMessageService as any,
    mockRequestService as any,
    mockRequestService2 as any,
    mockAuth as any,
    mockSonarrService as any,
    mockSonarrFacade as any,
    mockSettingsState as any,
  );

  return { component, mockSearchService, mockRequestService };
}

describe('TvDetailsComponent', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('shows the unavailable metadata state when the TV lookup returns null', async () => {
    const { component, mockSearchService, mockRequestService } = createComponent();
    mockSearchService.getTvInfo.mockResolvedValue(null as any);

    await expect(component.ngOnInit()).resolves.toBeUndefined();

    expect(component.metadataUnavailable).toBe(true);
    expect(component.tv).toBeNull();
    expect(mockRequestService.getChildRequests).not.toHaveBeenCalled();
  });

  it('shows the unavailable metadata state when the TV lookup fails', async () => {
    const { component, mockSearchService, mockRequestService } = createComponent();
    mockSearchService.getTvInfo.mockRejectedValue(new Error('metadata provider unavailable'));

    await expect(component.ngOnInit()).resolves.toBeUndefined();

    expect(component.metadataUnavailable).toBe(true);
    expect(component.tv).toBeUndefined();
    expect(mockRequestService.getChildRequests).not.toHaveBeenCalled();
  });

  it('keeps the normal details flow for a valid TV result', async () => {
    const { component } = createComponent();

    await component.ngOnInit();

    expect(component.metadataUnavailable).toBe(false);
    expect(component.tv.id).toBe(1396);
    expect(component.tv.images.original).toBe('../../../images/default_movie_poster.png');
  });
});
