import { describe, it, expect, vi, beforeEach } from 'vitest';
import { PlexServerDialogComponent } from './plex-server-dialog.component';
import { of } from 'rxjs';

function createComponent() {
  const mockDialogRef = {
    close: vi.fn(),
  };

  const data = {
    server: {
      name: 'My Plex',
      ip: '127.0.0.1',
      port: 32400,
      plexSelectedLibraries: [],
    },
  };

  const mockNotify = {
    error: vi.fn(),
    success: vi.fn(),
  };

  const mockTesterService = {
    plexTest: vi.fn().mockReturnValue(of(true)),
  };

  const mockPlexService = {
    getLibraries: vi.fn().mockReturnValue(of({
      successful: true,
      data: {
        mediaContainer: {
          directory: [
            { key: '1', title: 'Movies' },
            { key: '2', title: 'TV Shows' },
          ],
        },
      },
    })),
  };

  const comp = new PlexServerDialogComponent(
    mockDialogRef as any,
    data as any,
    mockNotify as any,
    mockTesterService as any,
    mockPlexService as any,
  );

  return { comp, mockDialogRef, data, mockNotify, mockTesterService, mockPlexService };
}

describe('PlexServerDialogComponent', () => {
  describe('cancel', () => {
    it('should close dialog with closed flag', () => {
      const { comp, mockDialogRef } = createComponent();
      comp.cancel();
      expect(mockDialogRef.close).toHaveBeenCalledWith({ closed: true });
    });
  });

  describe('delete', () => {
    it('should close dialog with deleted flag', () => {
      const { comp, mockDialogRef } = createComponent();
      comp.delete();
      expect(mockDialogRef.close).toHaveBeenCalledWith({ deleted: true });
    });
  });

  describe('save', () => {
    it('should close dialog with server data', () => {
      const { comp, mockDialogRef, data } = createComponent();
      comp.save();
      expect(mockDialogRef.close).toHaveBeenCalledWith({ server: data.server });
    });
  });

  describe('testPlex', () => {
    it('should notify success when connection is valid', () => {
      const { comp, mockNotify } = createComponent();
      comp.testPlex();
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully connected to the Plex server My Plex!');
    });

    it('should notify error when connection fails', () => {
      const { comp, mockNotify, mockTesterService } = createComponent();
      mockTesterService.plexTest.mockReturnValue(of(false));
      comp.testPlex();
      expect(mockNotify.error).toHaveBeenCalledWith('We could not connect to the Plex server  My Plex!');
    });
  });

  describe('loadLibraries', () => {
    it('should populate plexSelectedLibraries from API response', () => {
      const { comp, data } = createComponent();
      comp.loadLibraries();
      expect(data.server.plexSelectedLibraries).toHaveLength(2);
      expect(data.server.plexSelectedLibraries[0]).toEqual({ key: '1', title: 'Movies', enabled: false });
      expect(data.server.plexSelectedLibraries[1]).toEqual({ key: '2', title: 'TV Shows', enabled: false });
    });

    it('should show error when server IP is null', () => {
      const { comp, data, mockNotify, mockPlexService } = createComponent();
      data.server.ip = null as any;
      comp.loadLibraries();
      expect(mockNotify.error).toHaveBeenCalledWith('Plex is not yet configured correctly');
      expect(mockPlexService.getLibraries).not.toHaveBeenCalled();
    });

    it('should show error message from API when not successful', () => {
      const { comp, mockPlexService, mockNotify } = createComponent();
      mockPlexService.getLibraries.mockReturnValue(of({ successful: false, message: 'Auth failed' }));
      comp.loadLibraries();
      expect(mockNotify.error).toHaveBeenCalledWith('Auth failed');
    });
  });
});
