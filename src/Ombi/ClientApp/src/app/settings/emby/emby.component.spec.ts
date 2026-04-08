import { describe, it, expect, vi, beforeEach } from 'vitest';
import { EmbyComponent } from './emby.component';
import { of, throwError } from 'rxjs';

function createComponent() {
  const mockSettingsService = {
    getEmby: vi.fn().mockReturnValue(of({ servers: [], enable: true })),
    saveEmby: vi.fn().mockReturnValue(of(true)),
  };

  const mockNotify = {
    error: vi.fn(),
    success: vi.fn(),
  };

  const mockTesterService = {
    embyTest: vi.fn().mockReturnValue(of(true)),
  };

  const mockJobService = {
    runEmbyCacher: vi.fn().mockReturnValue(of(true)),
    runEmbyRecentlyAddedCacher: vi.fn().mockReturnValue(of(true)),
    clearMediaserverData: vi.fn().mockReturnValue(of(true)),
  };

  const mockEmbyService = {
    getPublicInfo: vi.fn().mockReturnValue(of({ serverName: 'Emby Server', id: 'emby-123' })),
    getLibraries: vi.fn().mockReturnValue(of({
      totalRecordCount: 2,
      items: [
        { id: '1', name: 'Movies', collectionType: 'movies' },
        { id: '2', name: 'TV Shows', collectionType: 'tvshows' },
      ],
    })),
  };

  const comp = new EmbyComponent(
    mockSettingsService as any,
    mockNotify as any,
    mockTesterService as any,
    mockJobService as any,
    mockEmbyService as any,
  );

  return { comp, mockSettingsService, mockNotify, mockTesterService, mockJobService, mockEmbyService };
}

describe('EmbyComponent', () => {
  describe('ngOnInit', () => {
    it('should load emby settings', () => {
      const { comp, mockSettingsService } = createComponent();
      comp.ngOnInit();
      expect(mockSettingsService.getEmby).toHaveBeenCalled();
      expect(comp.settings).toBeDefined();
    });
  });

  describe('discoverServerInfo', () => {
    it('should set server name and id from public info', async () => {
      const { comp, mockEmbyService } = createComponent();
      const server = { ip: '127.0.0.1' } as any;
      await comp.discoverServerInfo(server);
      expect(server.name).toBe('Emby Server');
      expect(server.serverId).toBe('emby-123');
      expect(comp.hasDiscoveredOrDirty).toBe(true);
    });
  });

  describe('addTab', () => {
    it('should add a new server when "Add Server" tab is selected', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      const event = { tab: { textLabel: 'Add Server' } } as any;
      comp.addTab(event);
      expect(comp.settings.servers).toHaveLength(1);
      expect(comp.settings.servers[0].name).toContain('New');
    });

    it('should initialize servers array when null', () => {
      const { comp, mockSettingsService } = createComponent();
      mockSettingsService.getEmby.mockReturnValue(of({ servers: null }));
      comp.ngOnInit();
      const event = { tab: { textLabel: 'Add Server' } } as any;
      comp.addTab(event);
      expect(comp.settings.servers).toHaveLength(1);
    });

    it('should not add server for other tabs', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      const event = { tab: { textLabel: 'Server 1' } } as any;
      comp.addTab(event);
      expect(comp.settings.servers).toHaveLength(0);
    });
  });

  describe('test', () => {
    it('should notify success on valid connection', () => {
      const { comp, mockNotify } = createComponent();
      const server = { name: 'My Emby' } as any;
      comp.test(server);
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully connected to the Emby server My Emby!');
    });

    it('should notify error on failed connection', () => {
      const { comp, mockNotify, mockTesterService } = createComponent();
      mockTesterService.embyTest.mockReturnValue(of(false));
      const server = { name: 'My Emby' } as any;
      comp.test(server);
      expect(mockNotify.error).toHaveBeenCalledWith('We could not connect to the Emby server  My Emby!');
    });
  });

  describe('removeServer', () => {
    it('should remove server from settings', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      const server1 = { name: 'Server1', id: 1 } as any;
      const server2 = { name: 'Server2', id: 2 } as any;
      comp.settings.servers = [server1, server2];

      comp.removeServer(server1);
      expect(comp.settings.servers).toHaveLength(1);
      expect(comp.settings.servers[0].name).toBe('Server2');
      expect(comp.hasDiscoveredOrDirty).toBe(true);
    });
  });

  describe('save', () => {
    it('should save settings and notify success', () => {
      const { comp, mockSettingsService, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.save();
      expect(mockSettingsService.saveEmby).toHaveBeenCalled();
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved Emby settings');
    });
  });

  describe('runCacher', () => {
    it('should trigger emby cacher', () => {
      const { comp, mockJobService, mockNotify } = createComponent();
      comp.runCacher();
      expect(mockJobService.runEmbyCacher).toHaveBeenCalled();
      expect(mockNotify.success).toHaveBeenCalledWith('Triggered the Emby Content Cacher');
    });
  });

  describe('runRecentlyAddedCacher', () => {
    it('should trigger recently added cacher', () => {
      const { comp, mockJobService, mockNotify } = createComponent();
      comp.runRecentlyAddedCacher();
      expect(mockJobService.runEmbyRecentlyAddedCacher).toHaveBeenCalled();
      expect(mockNotify.success).toHaveBeenCalledWith('Triggered the Emby Recently Added Sync');
    });
  });

  describe('clearDataAndResync', () => {
    it('should trigger clear and resync', () => {
      const { comp, mockJobService, mockNotify } = createComponent();
      comp.clearDataAndResync();
      expect(mockJobService.clearMediaserverData).toHaveBeenCalled();
      expect(mockNotify.success).toHaveBeenCalledWith('Triggered the Clear MediaServer Resync');
    });
  });

  describe('loadLibraries', () => {
    it('should populate embySelectedLibraries from API', () => {
      const { comp } = createComponent();
      const server = { ip: '127.0.0.1', embySelectedLibraries: [] } as any;
      comp.loadLibraries(server);
      expect(server.embySelectedLibraries).toHaveLength(2);
      expect(server.embySelectedLibraries[0]).toEqual({
        key: '1', title: 'Movies', enabled: false, collectionType: 'movies',
      });
    });

    it('should error when server IP is null', () => {
      const { comp, mockNotify, mockEmbyService } = createComponent();
      const server = { ip: null } as any;
      comp.loadLibraries(server);
      expect(mockNotify.error).toHaveBeenCalledWith('Emby is not yet configured correctly');
      expect(mockEmbyService.getLibraries).not.toHaveBeenCalled();
    });

    it('should error when no libraries found', () => {
      const { comp, mockNotify, mockEmbyService } = createComponent();
      mockEmbyService.getLibraries.mockReturnValue(of({ totalRecordCount: 0, items: [] }));
      const server = { ip: '127.0.0.1' } as any;
      comp.loadLibraries(server);
      expect(mockNotify.error).toHaveBeenCalledWith("Couldn't find any libraries");
    });
  });

  describe('toggle', () => {
    it('should set hasDiscoveredOrDirty to true', () => {
      const { comp } = createComponent();
      comp.toggle();
      expect(comp.hasDiscoveredOrDirty).toBe(true);
    });
  });
});
