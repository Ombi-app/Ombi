import { describe, it, expect, vi } from 'vitest';
import { of } from 'rxjs';
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
  const mockIdentityService = {
    getUsersDropdown: vi.fn().mockReturnValue(of([])),
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
    mockIdentityService as any,
    mockRequestServiceV1 as any,
    mockNotification as any,
    mockTranslate as any,
    mockFeatureFacade as any,
  );

  return { comp, mockAuth, mockStorageService, mockIdentityService, mockFeatureFacade, mockRequestServiceV1, mockNotification, mockRequestService };
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

    it('should load grid count and filter from storage', () => {
      const { comp, mockStorageService } = createComponent();
      mockStorageService.get.mockImplementation((key: string) => {
        if (key === 'Movie_DefaultGridCount') return '25';
        if (key === 'Movie_DefaultFilter') return '2';
        return null;
      });

      comp.ngOnInit();

      expect(comp.gridCount).toBe(25);
      expect(comp.currentFilter).toBe(2);
    });

    it('should keep defaults when storage returns null', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      expect(comp.gridCount).toBe(15);
      expect(comp.currentFilter).toBe(RequestFilterType.All);
    });
  });

  describe('getStatusClass', () => {
    it('should return status-available for available requests', () => {
      const { comp } = createComponent();
      expect(comp.getStatusClass({ requestStatus: 'Common.Available' })).toBe('status-available');
    });

    it('should return status-pending for pending requests', () => {
      const { comp } = createComponent();
      expect(comp.getStatusClass({ requestStatus: 'Common.Pending' })).toBe('status-pending');
    });

    it('should return status-processing for processing requests', () => {
      const { comp } = createComponent();
      expect(comp.getStatusClass({ requestStatus: 'Common.ProcessingRequest' })).toBe('status-processing');
    });

    it('should return status-denied for denied requests', () => {
      const { comp } = createComponent();
      expect(comp.getStatusClass({ requestStatus: 'Common.Denied' })).toBe('status-denied');
    });

    it('should return status-default for unknown status', () => {
      const { comp } = createComponent();
      expect(comp.getStatusClass({ requestStatus: 'SomethingElse' })).toBe('status-default');
    });

    it('should return status-default when requestStatus is null', () => {
      const { comp } = createComponent();
      expect(comp.getStatusClass({ requestStatus: null })).toBe('status-default');
    });
  });

  describe('getRequestDate', () => {
    it('should return requestedDate when year is not 1', () => {
      const { comp } = createComponent();
      const request = { requestedDate: new Date('2023-06-15'), requestedDate4k: new Date('2023-07-20') } as any;
      expect(comp.getRequestDate(request)).toEqual(new Date('2023-06-15'));
    });

    it('should return requestedDate4k when requestedDate year is 1', () => {
      const { comp } = createComponent();
      const request = { requestedDate: new Date('0001-01-01'), requestedDate4k: new Date('2023-07-20') } as any;
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
    it('should update currentFilter via the method', () => {
      const { comp } = createComponent();
      (comp as any).paginator = { firstPage: vi.fn() };
      (comp as any).reload$ = { next: vi.fn() };

      comp.switchFilter(RequestFilterType.Pending);

      expect(comp.currentFilter).toBe(RequestFilterType.Pending);
      expect((comp as any).paginator.firstPage).toHaveBeenCalled();
      expect((comp as any).reload$.next).toHaveBeenCalled();
    });
  });

  describe('view mode', () => {
    it('should default to cards', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      expect(comp.viewMode).toBe('cards');
    });

    it('should load view mode from storage', () => {
      const { comp, mockStorageService } = createComponent();
      mockStorageService.get.mockImplementation((key: string) =>
        key === 'Movie_DefaultViewMode' ? 'compact' : null);
      comp.ngOnInit();
      expect(comp.viewMode).toBe('compact');
    });

    it('should switch view and persist it', () => {
      const { comp, mockStorageService } = createComponent();
      comp.switchView('compact');
      expect(comp.viewMode).toBe('compact');
      expect(mockStorageService.save).toHaveBeenCalledWith('Movie_DefaultViewMode', 'compact');
    });

    it('should not persist when switching to the current view', () => {
      const { comp, mockStorageService } = createComponent();
      comp.switchView('cards');
      expect(mockStorageService.save).not.toHaveBeenCalled();
    });
  });

  describe('sorting', () => {
    it('should load sort settings from storage', () => {
      const { comp, mockStorageService } = createComponent();
      mockStorageService.get.mockImplementation((key: string) => {
        if (key === 'Movie_DefaultRequestListSort') return 'title';
        if (key === 'Movie_DefaultRequestListSortOrder') return 'asc';
        return null;
      });
      comp.ngOnInit();
      expect(comp.sortActive).toBe('title');
      expect(comp.sortDirection).toBe('asc');
    });

    it('should toggle sort direction', () => {
      const { comp } = createComponent();
      (comp as any).paginator = { firstPage: vi.fn() };
      expect(comp.sortDirection).toBe('desc');
      comp.toggleSortDirection();
      expect(comp.sortDirection).toBe('asc');
      comp.toggleSortDirection();
      expect(comp.sortDirection).toBe('desc');
    });

    it('should set a new sort field ascending via setSort', () => {
      const { comp } = createComponent();
      (comp as any).paginator = { firstPage: vi.fn() };
      comp.setSort('title');
      expect(comp.sortActive).toBe('title');
      expect(comp.sortDirection).toBe('asc');
    });

    it('should flip direction when setSort is called with the active field', () => {
      const { comp } = createComponent();
      (comp as any).paginator = { firstPage: vi.fn() };
      comp.setSort('requestedDate');
      expect(comp.sortDirection).toBe('asc');
      comp.setSort('requestedDate');
      expect(comp.sortDirection).toBe('desc');
    });
  });

  describe('user filter', () => {
    it('should load users for admins', () => {
      const { comp, mockAuth, mockIdentityService } = createComponent();
      mockAuth.hasRole.mockImplementation((role: string) => role === 'admin');
      mockIdentityService.getUsersDropdown.mockReturnValue(of([{ id: 'abc', username: 'someuser', email: '' }]));
      comp.ngOnInit();
      expect(comp.users.length).toBe(1);
    });

    it('should not load users for regular users', () => {
      const { comp, mockIdentityService } = createComponent();
      comp.ngOnInit();
      expect(mockIdentityService.getUsersDropdown).not.toHaveBeenCalled();
    });

    it('should pass the selected user to the request service', () => {
      const { comp, mockRequestService } = createComponent();
      (comp as any).paginator = { pageIndex: 0 };
      comp.selectedUserId = 'user-1';
      comp.loadData();
      expect(mockRequestService.getMovieRequests).toHaveBeenCalledWith(15, 0, 'requestedDate', 'desc', 'user-1');
    });

    it('should pass undefined when no user is selected', () => {
      const { comp, mockRequestService } = createComponent();
      (comp as any).paginator = { pageIndex: 0 };
      comp.loadData();
      expect(mockRequestService.getMovieRequests).toHaveBeenCalledWith(15, 0, 'requestedDate', 'desc', undefined);
    });
  });
});
