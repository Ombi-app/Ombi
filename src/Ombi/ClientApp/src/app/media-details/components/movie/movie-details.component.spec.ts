import { describe, it, expect, beforeEach, vi } from 'vitest';
import { MovieDetailsComponent } from './movie-details.component';
import { of } from 'rxjs';
import { RequestType } from '../../../interfaces';

function createComponent() {
  const mockSearchService = {
    getFullMovieDetails: vi.fn().mockReturnValue(of({})),
    getMovieByImdbId: vi.fn().mockReturnValue(of({})),
  };
  const mockRoute = {
    snapshot: { params: { movieDbId: '550' } },
    params: of({ movieDbId: '550' }),
  };
  const mockRouter = {
    url: '/details/movie/550',
    navigate: vi.fn(),
    routeReuseStrategy: { shouldReuseRoute: vi.fn() },
    onSameUrlNavigation: 'ignore',
  };
  const mockSanitizer = {
    bypassSecurityTrustStyle: vi.fn((x: string) => x),
    bypassSecurityTrustResourceUrl: vi.fn((x: string) => x),
  };
  const mockImageService = {
    getMovieBanner: vi.fn().mockReturnValue(of('banner-url')),
  };
  const mockDialog = {
    open: vi.fn().mockReturnValue({ afterClosed: () => of(null) }),
  };
  const mockRequestService = {
    requestMovie: vi.fn().mockReturnValue(of({ result: true, requestId: 1 })),
    approveMovie: vi.fn().mockReturnValue(of({ result: true })),
    markMovieAvailable: vi.fn().mockReturnValue(of({ result: true })),
    markMovieUnavailable: vi.fn().mockReturnValue(of({ result: true })),
    getMovieRequest: vi.fn().mockResolvedValue({ id: 1 }),
    subscribeToMovie: vi.fn().mockReturnValue(of(true)),
    unSubscribeToMovie: vi.fn().mockReturnValue(of(true)),
  };
  const mockRequestService2 = {
    updateMovieAdvancedOptions: vi.fn().mockReturnValue(of({})),
    reprocessRequest: vi.fn().mockReturnValue(of({ result: true, message: 'Reprocessed' })),
  };
  const mockRadarrService = {
    isRadarrEnabled: vi.fn().mockReturnValue(of(false)),
    getQualityProfilesFromSettings: vi.fn().mockReturnValue(of([])),
    getRootFoldersFromSettings: vi.fn().mockReturnValue(of([])),
  };
  const mockMessageService = {
    send: vi.fn(),
    sendRequestEngineResultError: vi.fn(),
  };
  const mockAuth = {
    hasRole: vi.fn().mockReturnValue(false),
  };
  const mockSettingsState = {
    getIssue: vi.fn().mockReturnValue(false),
  };
  const mockTranslate = {
    instant: vi.fn((key: string) => key),
    currentLang: 'en',
  };
  const mockFeatureFacade = {
    is4kEnabled: vi.fn().mockReturnValue(false),
  };

  const comp = new MovieDetailsComponent(
    mockSearchService as any,
    mockRoute as any,
    mockRouter as any,
    mockSanitizer as any,
    mockImageService as any,
    mockDialog as any,
    mockRequestService as any,
    mockRequestService2 as any,
    mockRadarrService as any,
    mockMessageService as any,
    mockAuth as any,
    mockSettingsState as any,
    mockTranslate as any,
    mockFeatureFacade as any,
  );

  return {
    comp, mockSearchService, mockRequestService, mockRequestService2,
    mockMessageService, mockAuth, mockSettingsState, mockFeatureFacade,
    mockDialog, mockTranslate, mockRadarrService, mockImageService, mockSanitizer,
  };
}

describe('MovieDetailsComponent', () => {
  describe('ngOnInit', () => {
    it('should set is4KEnabled from feature facade', async () => {
      const { comp, mockFeatureFacade, mockSearchService } = createComponent();
      mockFeatureFacade.is4kEnabled.mockReturnValue(true);
      mockSearchService.getFullMovieDetails.mockReturnValue(of({
        posterPath: '/poster.jpg',
        credits: { crew: [] },
        requestId: 0,
        backdropPath: null,
      }));
      await comp.ngOnInit();
      expect(comp.is4KEnabled).toBe(true);
    });

    it('should set issuesEnabled from settings state', async () => {
      const { comp, mockSettingsState, mockSearchService } = createComponent();
      mockSettingsState.getIssue.mockReturnValue(true);
      mockSearchService.getFullMovieDetails.mockReturnValue(of({
        posterPath: '/poster.jpg',
        credits: { crew: [] },
        requestId: 0,
        backdropPath: null,
      }));
      await comp.ngOnInit();
      expect(comp.issuesEnabled).toBe(true);
    });

    it('should set isAdmin true when user has admin role', async () => {
      const { comp, mockAuth, mockSearchService, mockRadarrService } = createComponent();
      mockAuth.hasRole.mockImplementation((role: string) => role === 'admin');
      mockRadarrService.isRadarrEnabled.mockReturnValue(of(false));
      mockSearchService.getFullMovieDetails.mockReturnValue(of({
        posterPath: '/poster.jpg',
        credits: { crew: [] },
        requestId: 0,
        backdropPath: null,
      }));
      await comp.ngOnInit();
      expect(comp.isAdmin).toBe(true);
    });

    it('should set isAdmin true when user has poweruser role', async () => {
      const { comp, mockAuth, mockSearchService, mockRadarrService } = createComponent();
      mockAuth.hasRole.mockImplementation((role: string) => role === 'poweruser');
      mockRadarrService.isRadarrEnabled.mockReturnValue(of(false));
      mockSearchService.getFullMovieDetails.mockReturnValue(of({
        posterPath: '/poster.jpg',
        credits: { crew: [] },
        requestId: 0,
        backdropPath: null,
      }));
      await comp.ngOnInit();
      expect(comp.isAdmin).toBe(true);
    });
  });

  describe('approve', () => {
    it('should approve standard request and set movie.approved', async () => {
      const { comp, mockRequestService, mockMessageService } = createComponent();
      comp.movie = { approved: false, approved4K: false } as any;
      comp.movieRequest = { id: 1 } as any;

      await comp.approve(false);

      expect(mockRequestService.approveMovie).toHaveBeenCalledWith({ id: 1, is4K: false });
      expect(comp.movie.approved).toBe(true);
      expect(mockMessageService.send).toHaveBeenCalled();
    });

    it('should approve 4K request and set movie.approved4K', async () => {
      const { comp, mockRequestService } = createComponent();
      comp.movie = { approved: false, approved4K: false } as any;
      comp.movieRequest = { id: 1 } as any;

      await comp.approve(true);

      expect(mockRequestService.approveMovie).toHaveBeenCalledWith({ id: 1, is4K: true });
      expect(comp.movie.approved4K).toBe(true);
    });

    it('should send error when approve fails', async () => {
      const { comp, mockRequestService, mockMessageService } = createComponent();
      comp.movie = { approved: false } as any;
      comp.movieRequest = { id: 1 } as any;
      mockRequestService.approveMovie.mockReturnValue(of({ result: false, errorCode: 0 }));

      await comp.approve(false);

      expect(mockMessageService.sendRequestEngineResultError).toHaveBeenCalled();
    });
  });

  describe('markAvailable', () => {
    it('should mark standard request available', async () => {
      const { comp, mockRequestService, mockMessageService } = createComponent();
      comp.movie = { available: false, available4K: false } as any;
      comp.movieRequest = { id: 1 } as any;

      await comp.markAvailable(false);

      expect(comp.movie.available).toBe(true);
      expect(mockMessageService.send).toHaveBeenCalled();
    });

    it('should mark 4K request available', async () => {
      const { comp, mockRequestService } = createComponent();
      comp.movie = { available: false, available4K: false } as any;
      comp.movieRequest = { id: 1 } as any;

      await comp.markAvailable(true);

      expect(comp.movie.available4K).toBe(true);
    });
  });

  describe('markUnavailable', () => {
    it('should mark standard request unavailable', async () => {
      const { comp, mockRequestService } = createComponent();
      comp.movie = { available: true, available4K: true } as any;
      comp.movieRequest = { id: 1 } as any;

      await comp.markUnavailable(false);

      expect(comp.movie.available).toBe(false);
    });

    it('should mark 4K request unavailable', async () => {
      const { comp } = createComponent();
      comp.movie = { available: true, available4K: true } as any;
      comp.movieRequest = { id: 1 } as any;

      await comp.markUnavailable(true);

      expect(comp.movie.available4K).toBe(false);
    });
  });

  describe('request (non-admin)', () => {
    it('should request movie and update state on success', async () => {
      const { comp, mockRequestService, mockMessageService } = createComponent();
      comp.isAdmin = false;
      comp.is4KEnabled = false;
      comp.movie = { id: 550, title: 'Test Movie', requested: false } as any;
      (comp as any).theMovidDbId = 550;

      mockRequestService.requestMovie.mockReturnValue(of({ result: true, requestId: 42 }));
      mockRequestService.getMovieRequest.mockResolvedValue({ id: 42 });

      await comp.request(false);

      expect(comp.movie.requested).toBe(true);
      expect(comp.movie.requestId).toBe(42);
      expect(mockMessageService.send).toHaveBeenCalled();
    });

    it('should set has4KRequest for 4K requests', async () => {
      const { comp, mockRequestService } = createComponent();
      comp.isAdmin = false;
      comp.is4KEnabled = true;
      comp.movie = { id: 550, title: 'Test', has4KRequest: false } as any;
      (comp as any).theMovidDbId = 550;

      mockRequestService.requestMovie.mockReturnValue(of({ result: true, requestId: 42 }));
      mockRequestService.getMovieRequest.mockResolvedValue({ id: 42 });

      await comp.request(true);

      expect(comp.movie.has4KRequest).toBe(true);
    });

    it('should force is4K to false when 4K is not enabled', async () => {
      const { comp, mockRequestService } = createComponent();
      comp.isAdmin = false;
      comp.is4KEnabled = false;
      comp.movie = { id: 550, title: 'Test', requested: false } as any;
      (comp as any).theMovidDbId = 550;

      mockRequestService.requestMovie.mockReturnValue(of({ result: true, requestId: 1 }));
      mockRequestService.getMovieRequest.mockResolvedValue({ id: 1 });

      await comp.request(true); // pass true, but 4K disabled

      expect(mockRequestService.requestMovie).toHaveBeenCalledWith(
        expect.objectContaining({ is4KRequest: false })
      );
    });

    it('should send error on failed request', async () => {
      const { comp, mockRequestService, mockMessageService } = createComponent();
      comp.isAdmin = false;
      comp.movie = { id: 550, title: 'Test' } as any;
      (comp as any).theMovidDbId = 550;

      mockRequestService.requestMovie.mockReturnValue(of({ result: false, errorCode: 0 }));

      await comp.request(false);

      expect(mockMessageService.sendRequestEngineResultError).toHaveBeenCalled();
    });
  });

  describe('deny', () => {
    it('should not open dialog when no movieRequest', async () => {
      const { comp, mockDialog } = createComponent();
      comp.movieRequest = undefined;

      await comp.deny();

      expect(mockDialog.open).not.toHaveBeenCalled();
    });

    it('should open deny dialog when movieRequest exists', async () => {
      const { comp, mockDialog } = createComponent();
      comp.movieRequest = { id: 1 } as any;

      await comp.deny();

      expect(mockDialog.open).toHaveBeenCalled();
    });
  });

  describe('reProcessRequest', () => {
    it('should call reprocessRequest and send success message', () => {
      const { comp, mockRequestService2, mockMessageService } = createComponent();
      comp.movieRequest = { id: 1 } as any;
      mockRequestService2.reprocessRequest.mockReturnValue(of({ result: true, message: 'Reprocessed' }));

      comp.reProcessRequest(false);

      expect(mockRequestService2.reprocessRequest).toHaveBeenCalledWith(1, RequestType.movie, false);
      expect(mockMessageService.send).toHaveBeenCalledWith('Reprocessed', 'Ok');
    });

    it('should send error on failed reprocess', () => {
      const { comp, mockRequestService2, mockMessageService } = createComponent();
      comp.movieRequest = { id: 1 } as any;
      mockRequestService2.reprocessRequest.mockReturnValue(of({ result: false, errorCode: 0 }));

      comp.reProcessRequest(false);

      expect(mockMessageService.sendRequestEngineResultError).toHaveBeenCalled();
    });
  });

  describe('notify / unNotify', () => {
    it('should set subscribed to true on notify success', () => {
      const { comp, mockRequestService, mockMessageService } = createComponent();
      comp.movie = { subscribed: false, title: 'Test' } as any;
      comp.movieRequest = { id: 1 } as any;
      mockRequestService.subscribeToMovie.mockReturnValue(of(true));

      comp.notify();

      expect(comp.movie.subscribed).toBe(true);
      expect(mockMessageService.send).toHaveBeenCalled();
    });

    it('should set subscribed to false on unNotify success', () => {
      const { comp, mockRequestService, mockMessageService } = createComponent();
      comp.movie = { subscribed: true, title: 'Test' } as any;
      comp.movieRequest = { id: 1 } as any;
      mockRequestService.unSubscribeToMovie.mockReturnValue(of(true));

      comp.unNotify();

      expect(comp.movie.subscribed).toBe(false);
    });
  });

  describe('openDialog', () => {
    it('should open YouTube trailer dialog', () => {
      const { comp, mockDialog } = createComponent();
      comp.movie = { videos: { results: [{ key: 'abc123' }] } } as any;

      comp.openDialog();

      expect(mockDialog.open).toHaveBeenCalledWith(
        expect.anything(),
        expect.objectContaining({ width: '560px', data: 'abc123' })
      );
    });
  });

  describe('setAdvancedOptions', () => {
    it('should set advancedOptions and override titles when both IDs provided', () => {
      const { comp } = createComponent();
      comp.movieRequest = {} as any;

      const data = {
        rootFolderId: 1,
        profileId: 2,
        profiles: [{ id: 2, name: '1080p' }],
        rootFolders: [{ id: 1, path: '/movies' }],
      } as any;

      comp.setAdvancedOptions(data);

      expect(comp.advancedOptions).toBe(data);
      expect(comp.movieRequest.qualityOverrideTitle).toBe('1080p');
      expect(comp.movieRequest.rootPathOverrideTitle).toBe('/movies');
    });

    it('should skip qualityOverrideTitle when rootFolderId is falsy (condition is inverted in source)', () => {
      const { comp } = createComponent();
      comp.movieRequest = {} as any;

      const data = {
        rootFolderId: 0,
        profileId: 2,
        profiles: [{ id: 2, name: '1080p' }],
        rootFolders: [{ id: 0, path: '/default' }, { id: 1, path: '/movies' }],
      } as any;

      comp.setAdvancedOptions(data);

      // BUG: qualityOverrideTitle is gated by rootFolderId instead of profileId
      // When rootFolderId is falsy, the `if (data.rootFolderId)` block is skipped
      expect(comp.movieRequest.qualityOverrideTitle).toBeUndefined();
      // But rootPathOverrideTitle IS set because profileId is truthy (inverted gate)
      // and it filters rootFolders by rootFolderId=0
      expect(comp.movieRequest.rootPathOverrideTitle).toBe('/default');
    });

    it('should skip rootPathOverrideTitle when profileId is falsy (condition is inverted in source)', () => {
      const { comp } = createComponent();
      comp.movieRequest = {} as any;

      const data = {
        rootFolderId: 1,
        profileId: 0,
        profiles: [{ id: 0, name: 'Any' }, { id: 2, name: '1080p' }],
        rootFolders: [{ id: 1, path: '/movies' }],
      } as any;

      comp.setAdvancedOptions(data);

      // BUG: rootPathOverrideTitle is gated by profileId instead of rootFolderId
      // When profileId is falsy, the `if (data.profileId)` block is skipped
      expect(comp.movieRequest.rootPathOverrideTitle).toBeUndefined();
      // But qualityOverrideTitle IS set because rootFolderId is truthy (inverted gate)
      // and it filters profiles by profileId=0
      expect(comp.movieRequest.qualityOverrideTitle).toBe('Any');
    });
  });
});
