import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { PlexWatchlistComponent } from './plex-watchlist.component';
import { of, throwError } from 'rxjs';

describe('PlexWatchlistComponent', () => {
  let comp: PlexWatchlistComponent;
  let mockPlexService: any;

  beforeEach(() => {
    vi.useFakeTimers();

    mockPlexService = {
      getWatchlistUsers: vi.fn().mockReturnValue(of([
        { userName: 'user1', syncStatus: 0 },
        { userName: 'user2', syncStatus: 1 },
      ])),
      revalidateWatchlistUsers: vi.fn().mockReturnValue(of(undefined)),
    };

    comp = new PlexWatchlistComponent(mockPlexService);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  describe('ngOnInit', () => {
    it('should load watchlist users into dataSource', () => {
      comp.ngOnInit();
      expect(mockPlexService.getWatchlistUsers).toHaveBeenCalled();
      expect(comp.dataSource.data).toHaveLength(2);
      expect(comp.dataSource.data[0].userName).toBe('user1');
    });
  });

  describe('forceRecheck', () => {
    it('should revalidate and reload after delay', () => {
      comp.ngOnInit();
      mockPlexService.getWatchlistUsers.mockClear();

      comp.forceRecheck();
      expect(comp.isReloading).toBe(true);
      expect(mockPlexService.revalidateWatchlistUsers).toHaveBeenCalled();

      vi.advanceTimersByTime(3000);
      expect(mockPlexService.getWatchlistUsers).toHaveBeenCalled();
      expect(comp.isReloading).toBe(false);
    });

    it('should not recheck if already reloading', () => {
      comp.isReloading = true;
      comp.forceRecheck();
      expect(mockPlexService.revalidateWatchlistUsers).not.toHaveBeenCalled();
    });

    it('should reset isReloading on error', () => {
      mockPlexService.revalidateWatchlistUsers.mockReturnValue(throwError(() => new Error('fail')));
      comp.forceRecheck();
      expect(comp.isReloading).toBe(false);
    });
  });

  describe('displayedColumns', () => {
    it('should have userName and syncStatus columns', () => {
      expect(comp.displayedColumns).toEqual(['userName', 'syncStatus']);
    });
  });
});
