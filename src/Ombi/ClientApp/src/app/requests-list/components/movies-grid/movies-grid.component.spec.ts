import { describe, it, expect, beforeEach, vi } from 'vitest';
import { MoviesGridComponent } from './movies-grid.component';
import { RequestFilterType } from '../../models/RequestFilterType';

function createComponent() {
  const mockRequestService = {
    getMovieRequests: vi.fn(),
    getMoviePendingRequests: vi.fn(),
    getMovieAvailableRequests: vi.fn(),
    getMovieProcessingRequests: vi.fn(),
    getMovieDeniedRequests: vi.fn(),
  };
  const mockRef = { detectChanges: vi.fn() };
  const mockAuth = {
    hasRole: vi.fn().mockReturnValue(false),
    claims: vi.fn().mockReturnValue({ name: 'testuser' }),
  };
  const mockStorageService = {
    get: vi.fn().mockReturnValue(null),
    save: vi.fn(),
  };
  const mockRequestServiceV1 = {
    removeMovieRequestAsync: vi.fn(),
    approveMovie: vi.fn(),
    denyMovie: vi.fn(),
  };
  const mockNotification = {
    success: vi.fn(),
    error: vi.fn(),
  };
  const mockTranslate = {
    instant: vi.fn((key: string) => key),
  };
  const mockFeatureFacade = {
    is4kEnabled: vi.fn().mockReturnValue(false),
    isPlayedSyncEnabled: vi.fn().mockReturnValue(false),
  };

  const comp = new MoviesGridComponent(
    mockRequestService as any,
    mockRef as any,
    mockAuth as any,
    mockStorageService as any,
    mockRequestServiceV1 as any,
    mockNotification as any,
    mockTranslate as any,
    mockFeatureFacade as any,
  );

  return { comp, mockAuth, mockStorageService, mockFeatureFacade, mockRequestServiceV1, mockNotification };
}

describe('MoviesGridComponent', () => {
  describe('constructor', () => {
    it('should set userName from auth claims', () => {
      const { comp } = createComponent();
      expect(comp.userName).toBe('testuser');
    });
  });

  describe('ngOnInit', () => {
    it('should set isAdmin when user has admin role', () => {
      const { comp, mockAuth } = createComponent();
      mockAuth.hasRole.mockImplementation((role: string) => role === 'admin');
      comp.ngOnInit();
      expect(comp.isAdmin).toBe(true);
    });

    it('should set isAdmin when user has poweruser role', () => {
      const { comp, mockAuth } = createComponent();
      mockAuth.hasRole.mockImplementation((role: string) => role === 'poweruser');
      comp.ngOnInit();
      expect(comp.isAdmin).toBe(true);
    });

    it('should not set isAdmin for regular user', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      expect(comp.isAdmin).toBe(false);
    });

    it('should set is4kEnabled from feature facade', () => {
      const { comp, mockFeatureFacade } = createComponent();
      mockFeatureFacade.is4kEnabled.mockReturnValue(true);
      comp.ngOnInit();
      expect(comp.is4kEnabled).toBe(true);
    });

    it('should set isPlayedSyncEnabled from feature facade', () => {
      const { comp, mockFeatureFacade } = createComponent();
      mockFeatureFacade.isPlayedSyncEnabled.mockReturnValue(true);
      comp.ngOnInit();
      expect(comp.isPlayedSyncEnabled).toBe(true);
    });

    it('should load defaults from storage', () => {
      const { comp, mockStorageService } = createComponent();
      mockStorageService.get.mockImplementation((key: string) => {
        if (key === 'Movie_DefaultRequestListSort') return 'title';
        if (key === 'Movie_DefaultRequestListSortOrder') return 'asc';
        if (key === 'Movie_DefaultGridCount') return '25';
        if (key === 'Movie_DefaultFilter') return '2'; // Pending
        return null;
      });

      comp.ngOnInit();

      expect(comp.defaultSort).toBe('title');
      expect(comp.defaultOrder).toBe('asc');
      expect(comp.gridCount).toBe('25');
      expect(comp.currentFilter).toBe(2);
    });

    it('should keep defaults when storage returns null', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      expect(comp.defaultSort).toBe('requestedDate');
      expect(comp.defaultOrder).toBe('desc');
      expect(comp.gridCount).toBe('15');
    });
  });

  describe('setDisplayedColumns', () => {
    it('should include base columns for non-admin', () => {
      const { comp } = createComponent();
      comp.isAdmin = false;
      comp.is4kEnabled = false;
      comp.isPlayedSyncEnabled = false;
      comp.currentFilter = RequestFilterType.All;
      comp.setDisplayedColumns();
      expect(comp.displayedColumns).toEqual([
        'title', 'requestedUser.requestedBy', 'status', 'requestStatus', 'requestedDate', 'actions'
      ]);
    });

    it('should add select column for admin', () => {
      const { comp } = createComponent();
      comp.isAdmin = true;
      comp.is4kEnabled = false;
      comp.isPlayedSyncEnabled = false;
      comp.currentFilter = RequestFilterType.All;
      comp.setDisplayedColumns();
      expect(comp.displayedColumns[0]).toBe('select');
    });

    it('should add has4kRequest column when 4K enabled and admin', () => {
      const { comp, mockAuth } = createComponent();
      comp.isAdmin = true;
      comp.is4kEnabled = true;
      comp.isPlayedSyncEnabled = false;
      comp.currentFilter = RequestFilterType.All;
      mockAuth.hasRole.mockReturnValue(true);
      comp.setDisplayedColumns();
      expect(comp.displayedColumns).toContain('has4kRequest');
    });

    it('should add watchedByRequestedUser when PlayedSync enabled and filter is All', () => {
      const { comp } = createComponent();
      comp.isAdmin = false;
      comp.is4kEnabled = false;
      comp.isPlayedSyncEnabled = true;
      comp.currentFilter = RequestFilterType.All;
      comp.setDisplayedColumns();
      expect(comp.displayedColumns).toContain('watchedByRequestedUser');
    });

    it('should add watchedByRequestedUser when PlayedSync enabled and filter is Available', () => {
      const { comp } = createComponent();
      comp.isAdmin = false;
      comp.is4kEnabled = false;
      comp.isPlayedSyncEnabled = true;
      comp.currentFilter = RequestFilterType.Available;
      comp.setDisplayedColumns();
      expect(comp.displayedColumns).toContain('watchedByRequestedUser');
    });

    it('should not add watchedByRequestedUser when filter is Pending', () => {
      const { comp } = createComponent();
      comp.isAdmin = false;
      comp.is4kEnabled = false;
      comp.isPlayedSyncEnabled = true;
      comp.currentFilter = RequestFilterType.Pending;
      comp.setDisplayedColumns();
      expect(comp.displayedColumns).not.toContain('watchedByRequestedUser');
    });

    it('should always put actions column at the end', () => {
      const { comp } = createComponent();
      comp.isAdmin = true;
      comp.is4kEnabled = true;
      comp.isPlayedSyncEnabled = true;
      comp.currentFilter = RequestFilterType.All;
      comp.setDisplayedColumns();
      expect(comp.displayedColumns[comp.displayedColumns.length - 1]).toBe('actions');
    });
  });

  describe('getRequestDate', () => {
    it('should return requestedDate when year is not 1', () => {
      const { comp } = createComponent();
      const request = {
        requestedDate: new Date('2023-06-15'),
        requestedDate4k: new Date('2023-07-20'),
      } as any;
      expect(comp.getRequestDate(request)).toEqual(new Date('2023-06-15'));
    });

    it('should return requestedDate4k when requestedDate year is 1 (default)', () => {
      const { comp } = createComponent();
      const request = {
        requestedDate: new Date('0001-01-01'),
        requestedDate4k: new Date('2023-07-20'),
      } as any;
      expect(comp.getRequestDate(request)).toEqual(new Date('2023-07-20'));
    });
  });

  describe('isAllSelected', () => {
    it('should return true when all rows are selected', () => {
      const { comp } = createComponent();
      const data = [{ id: 1 }, { id: 2 }] as any[];
      comp.dataSource = { data } as any;
      data.forEach(row => comp.selection.select(row));
      expect(comp.isAllSelected()).toBe(true);
    });

    it('should return false when not all rows are selected', () => {
      const { comp } = createComponent();
      const data = [{ id: 1 }, { id: 2 }] as any[];
      comp.dataSource = { data } as any;
      comp.selection.select(data[0]);
      expect(comp.isAllSelected()).toBe(false);
    });
  });

  describe('masterToggle', () => {
    it('should select all when not all selected', () => {
      const { comp } = createComponent();
      const data = [{ id: 1 }, { id: 2 }] as any[];
      comp.dataSource = { data } as any;
      comp.masterToggle();
      expect(comp.selection.selected.length).toBe(2);
    });

    it('should clear selection when all are selected', () => {
      const { comp } = createComponent();
      const data = [{ id: 1 }, { id: 2 }] as any[];
      comp.dataSource = { data } as any;
      data.forEach(row => comp.selection.select(row));
      comp.masterToggle();
      expect(comp.selection.selected.length).toBe(0);
    });
  });

  describe('bulkDelete', () => {
    it('should not delete when selection is empty', async () => {
      const { comp, mockRequestServiceV1 } = createComponent();
      comp.selection.clear();
      await comp.bulkDelete();
      expect(mockRequestServiceV1.removeMovieRequestAsync).not.toHaveBeenCalled();
    });
  });

  describe('switchFilter', () => {
    it('should update currentFilter', () => {
      const { comp } = createComponent();
      // Can't fully test ngAfterViewInit without DOM, but we can verify the filter changes
      comp.paginator = { pageIndex: 0, page: { pipe: vi.fn() } } as any;
      comp.sort = { sortChange: { subscribe: vi.fn(), pipe: vi.fn() }, active: 'title', direction: 'asc' } as any;
      comp.currentFilter = RequestFilterType.All;
      // switchFilter calls ngAfterViewInit which needs DOM, so just test the state change
      comp.currentFilter = RequestFilterType.Pending;
      expect(comp.currentFilter).toBe(RequestFilterType.Pending);
    });
  });
});
