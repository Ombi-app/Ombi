import { describe, it, expect, vi, beforeEach } from 'vitest';
import { PlexComponent } from './plex.component';
import { of, throwError, EMPTY } from 'rxjs';
import { PlexSyncType } from './components/models';

function createComponent() {
  const mockSettingsService = {
    getPlex: vi.fn().mockReturnValue(of({ servers: [], enable: true })),
    savePlex: vi.fn().mockReturnValue(of(true)),
  };

  const mockNotify = {
    error: vi.fn(),
    success: vi.fn(),
    warning: vi.fn(),
  };

  const mockPlexService = {
    getServers: vi.fn().mockReturnValue(of({ success: true, servers: [] })),
  };

  const mockJobService = {
    runPlexCacher: vi.fn().mockReturnValue(of(true)),
    runPlexRecentlyAddedCacher: vi.fn().mockReturnValue(of(true)),
    clearMediaserverData: vi.fn().mockReturnValue(of(true)),
    runPlexWatchlistImport: vi.fn().mockReturnValue(of(true)),
  };

  const mockDialog = {
    open: vi.fn().mockReturnValue({ afterClosed: () => of({ closed: true }) }),
  };

  const comp = new PlexComponent(
    mockSettingsService as any,
    mockNotify as any,
    mockPlexService as any,
    mockJobService as any,
    mockDialog as any,
  );

  return { comp, mockSettingsService, mockNotify, mockPlexService, mockJobService, mockDialog };
}

describe('PlexComponent', () => {
  describe('ngOnInit', () => {
    it('should load plex settings', () => {
      const { comp, mockSettingsService } = createComponent();
      comp.ngOnInit();
      expect(mockSettingsService.getPlex).toHaveBeenCalled();
      expect(comp.settings).toBeDefined();
    });

    it('should initialize servers array when null', () => {
      const { comp, mockSettingsService } = createComponent();
      mockSettingsService.getPlex.mockReturnValue(of({ servers: null, enable: true }));
      comp.ngOnInit();
      expect(comp.settings.servers).toEqual([]);
    });
  });

  describe('requestServers', () => {
    it('should load servers and notify on success', () => {
      const { comp, mockPlexService, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.username = 'admin';
      comp.password = 'pass';

      comp.requestServers();
      expect(mockPlexService.getServers).toHaveBeenCalledWith('admin', 'pass');
      expect(comp.serversButton).toBe(true);
      expect(mockNotify.success).toHaveBeenCalledWith('Found the servers! Please select one!');
    });

    it('should warn when getServers returns success=false', () => {
      const { comp, mockPlexService, mockNotify } = createComponent();
      mockPlexService.getServers.mockReturnValue(of({ success: false }));
      comp.ngOnInit();
      comp.requestServers();
      expect(mockNotify.warning).toHaveBeenCalled();
    });
  });

  describe('selectServer', () => {
    it('should create a server from selected device', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();

      const device = {
        localAddresses: '192.168.1.100',
        name: 'My Plex',
        machineIdentifier: 'abc123',
        accessToken: 'token',
        port: '32400',
        scheme: 'https',
      };

      comp.selectServer(device as any);
      expect(mockNotify.success).toHaveBeenCalledWith('Selected My Plex!');
    });

    it('should use last address when multiple local addresses exist', () => {
      const { comp, mockDialog } = createComponent();
      comp.ngOnInit();

      const device = {
        localAddresses: '192.168.1.100,10.0.0.1',
        name: 'My Plex',
        machineIdentifier: 'abc123',
        accessToken: 'token',
        port: '32400',
        scheme: 'http',
      };

      comp.selectServer(device as any);
      expect(mockDialog.open).toHaveBeenCalled();
    });
  });

  describe('save', () => {
    it('should filter out empty server names and save', () => {
      const { comp, mockSettingsService, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.settings.servers = [
        { name: 'Server1', ip: '127.0.0.1', serverHostname: '' } as any,
        { name: '', ip: '' } as any,
      ];

      comp.save();
      expect(mockSettingsService.savePlex).toHaveBeenCalled();
      expect(comp.settings.servers).toHaveLength(1);
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved Plex settings');
    });

    it('should error when serverHostname does not start with http', () => {
      const { comp, mockNotify, mockSettingsService } = createComponent();
      comp.ngOnInit();
      comp.settings.servers = [
        { name: 'Server1', ip: '127.0.0.1', serverHostname: 'plex.example.com' } as any,
      ];

      comp.save();
      expect(mockNotify.error).toHaveBeenCalledWith(
        'Please ensure that your External Hostname is a full URL including the Scheme (http/https)'
      );
      expect(mockSettingsService.savePlex).not.toHaveBeenCalled();
    });

    it('should allow valid http hostname', () => {
      const { comp, mockSettingsService } = createComponent();
      comp.ngOnInit();
      comp.settings.servers = [
        { name: 'Server1', ip: '127.0.0.1', serverHostname: 'https://plex.example.com' } as any,
      ];

      comp.save();
      expect(mockSettingsService.savePlex).toHaveBeenCalled();
    });
  });

  describe('runSync', () => {
    it('should trigger full sync for PlexSyncType.Full', () => {
      const { comp, mockJobService, mockNotify } = createComponent();
      comp.runSync(PlexSyncType.Full);
      expect(mockJobService.runPlexCacher).toHaveBeenCalled();
      expect(mockNotify.success).toHaveBeenCalledWith('Triggered the Plex Full Sync');
    });

    it('should trigger recently added for PlexSyncType.RecentlyAdded', () => {
      const { comp, mockJobService, mockNotify } = createComponent();
      comp.runSync(PlexSyncType.RecentlyAdded);
      expect(mockJobService.runPlexRecentlyAddedCacher).toHaveBeenCalled();
      expect(mockNotify.success).toHaveBeenCalledWith('Triggered the Plex Recently Added Sync');
    });

    it('should trigger clear and resync for PlexSyncType.ClearAndReSync', () => {
      const { comp, mockJobService, mockNotify } = createComponent();
      comp.runSync(PlexSyncType.ClearAndReSync);
      expect(mockJobService.clearMediaserverData).toHaveBeenCalled();
      expect(mockNotify.success).toHaveBeenCalledWith('Triggered the Clear MediaServer Resync');
    });

    it('should trigger watchlist import for PlexSyncType.WatchlistImport', () => {
      const { comp, mockJobService, mockNotify } = createComponent();
      comp.runSync(PlexSyncType.WatchlistImport);
      expect(mockJobService.runPlexWatchlistImport).toHaveBeenCalled();
      expect(mockNotify.success).toHaveBeenCalledWith('Triggered the Watchlist Import');
    });
  });

  describe('ngOnDestroy', () => {
    it('should complete subscriptions', () => {
      const { comp } = createComponent();
      expect(() => comp.ngOnDestroy()).not.toThrow();
    });
  });
});
