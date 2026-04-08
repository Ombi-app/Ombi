import { describe, it, expect, vi, beforeEach } from 'vitest';
import { JellyfinComponent } from './jellyfin.component';
import { of } from 'rxjs';

function createComponent() {
  const mockSettingsService = {
    getJellyfin: vi.fn().mockReturnValue(of({ servers: [], enable: true })),
    saveJellyfin: vi.fn().mockReturnValue(of(true)),
  };

  const mockNotify = {
    error: vi.fn(),
    success: vi.fn(),
  };

  const mockTesterService = {
    jellyfinTest: vi.fn().mockReturnValue(of(true)),
  };

  const mockJobService = {
    runJellyfinCacher: vi.fn().mockReturnValue(of(true)),
    clearMediaserverData: vi.fn().mockReturnValue(of(true)),
  };

  const mockJellyfinService = {
    getPublicInfo: vi.fn().mockReturnValue(of({ serverName: 'Jellyfin Server', id: 'jf-123' })),
    getLibraries: vi.fn().mockReturnValue(of({
      totalRecordCount: 2,
      items: [
        { id: '1', name: 'Movies', collectionType: 'movies' },
        { id: '2', name: 'TV Shows', collectionType: 'tvshows' },
      ],
    })),
  };

  const comp = new JellyfinComponent(
    mockSettingsService as any,
    mockNotify as any,
    mockTesterService as any,
    mockJobService as any,
    mockJellyfinService as any,
  );

  return { comp, mockSettingsService, mockNotify, mockTesterService, mockJobService, mockJellyfinService };
}

describe('JellyfinComponent', () => {
  describe('ngOnInit', () => {
    it('should load jellyfin settings', () => {
      const { comp, mockSettingsService } = createComponent();
      comp.ngOnInit();
      expect(mockSettingsService.getJellyfin).toHaveBeenCalled();
      expect(comp.settings).toBeDefined();
    });
  });

  describe('discoverServerInfo', () => {
    it('should set server name and id', async () => {
      const { comp } = createComponent();
      const server = { ip: '127.0.0.1' } as any;
      await comp.discoverServerInfo(server);
      expect(server.name).toBe('Jellyfin Server');
      expect(server.serverId).toBe('jf-123');
      expect(comp.hasDiscoveredOrDirty).toBe(true);
    });
  });

  describe('addTab', () => {
    it('should add a new server on "Add Server" tab', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      comp.addTab({ tab: { textLabel: 'Add Server' } } as any);
      expect(comp.settings.servers).toHaveLength(1);
    });

    it('should not add for other tabs', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      comp.addTab({ tab: { textLabel: 'Server 1' } } as any);
      expect(comp.settings.servers).toHaveLength(0);
    });
  });

  describe('test', () => {
    it('should notify success on valid connection', () => {
      const { comp, mockNotify } = createComponent();
      comp.test({ name: 'My JF' } as any);
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully connected to the Jellyfin server My JF!');
    });

    it('should notify error on failed connection', () => {
      const { comp, mockNotify, mockTesterService } = createComponent();
      mockTesterService.jellyfinTest.mockReturnValue(of(false));
      comp.test({ name: 'My JF' } as any);
      expect(mockNotify.error).toHaveBeenCalledWith('We could not connect to the Jellyfin server  My JF!');
    });
  });

  describe('removeServer', () => {
    it('should remove server and mark dirty', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      const server = { name: 'Server1' } as any;
      comp.settings.servers = [server];
      comp.removeServer(server);
      expect(comp.settings.servers).toHaveLength(0);
      expect(comp.hasDiscoveredOrDirty).toBe(true);
    });
  });

  describe('save', () => {
    it('should save and notify success', () => {
      const { comp, mockSettingsService, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.save();
      expect(mockSettingsService.saveJellyfin).toHaveBeenCalled();
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved Jellyfin settings');
    });
  });

  describe('runCacher', () => {
    it('should trigger jellyfin cacher', () => {
      const { comp, mockJobService, mockNotify } = createComponent();
      comp.runCacher();
      expect(mockJobService.runJellyfinCacher).toHaveBeenCalled();
      expect(mockNotify.success).toHaveBeenCalledWith('Triggered the Jellyfin Content Cacher');
    });
  });

  describe('clearDataAndResync', () => {
    it('should trigger clear and resync', () => {
      const { comp, mockJobService, mockNotify } = createComponent();
      comp.clearDataAndResync();
      expect(mockJobService.clearMediaserverData).toHaveBeenCalled();
    });
  });

  describe('loadLibraries', () => {
    it('should populate jellyfinSelectedLibraries', () => {
      const { comp } = createComponent();
      const server = { ip: '127.0.0.1', jellyfinSelectedLibraries: [] } as any;
      comp.loadLibraries(server);
      expect(server.jellyfinSelectedLibraries).toHaveLength(2);
      expect(server.jellyfinSelectedLibraries[0]).toEqual({
        key: '1', title: 'Movies', enabled: false, collectionType: 'movies',
      });
    });

    it('should error when server IP is null', () => {
      const { comp, mockNotify, mockJellyfinService } = createComponent();
      comp.loadLibraries({ ip: null } as any);
      expect(mockNotify.error).toHaveBeenCalledWith('Jellyfin is not yet configured correctly');
      expect(mockJellyfinService.getLibraries).not.toHaveBeenCalled();
    });

    it('should error when no libraries found', () => {
      const { comp, mockNotify, mockJellyfinService } = createComponent();
      mockJellyfinService.getLibraries.mockReturnValue(of({ totalRecordCount: 0, items: [] }));
      comp.loadLibraries({ ip: '127.0.0.1' } as any);
      expect(mockNotify.error).toHaveBeenCalledWith("Couldn't find any libraries");
    });
  });
});
