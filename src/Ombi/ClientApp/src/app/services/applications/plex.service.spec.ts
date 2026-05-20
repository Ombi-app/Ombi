import { describe, it, expect, vi, beforeEach } from 'vitest';
import { PlexService } from './plex.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of({})),
    post: vi.fn().mockReturnValue(of({})),
  };
  const service = Object.create(PlexService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/Plex/';
  service.headers = { set: vi.fn() };
  return { service: service as PlexService, mockHttp };
}

describe('PlexService', () => {
  let service: PlexService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should POST for logIn with login and password', () => {
    service.logIn('admin', 'pass123');
    expect(mockHttp.post).toHaveBeenCalledWith(
      '/api/v1/Plex/',
      JSON.stringify({ login: 'admin', password: 'pass123' }),
      expect.anything()
    );
  });

  it('should POST for getServers', () => {
    service.getServers('admin', 'pass123');
    expect(mockHttp.post).toHaveBeenCalledWith(
      '/api/v1/Plex/servers',
      JSON.stringify({ login: 'admin', password: 'pass123' }),
      expect.anything()
    );
  });

  it('should GET for getServersFromSettings', () => {
    service.getServersFromSettings();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Plex/servers', expect.anything());
  });

  it('should POST for getLibraries', () => {
    const settings = { name: 'My Plex' } as any;
    service.getLibraries(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Plex/Libraries', JSON.stringify(settings), expect.anything());
  });

  it('should GET for getLibrariesFromSettings with machineId', () => {
    service.getLibrariesFromSettings('machine-abc');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Plex/Libraries/machine-abc', expect.anything());
  });

  it('should POST for addUserToServer', () => {
    const user = { username: 'newuser' } as any;
    service.addUserToServer(user);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Plex/user', JSON.stringify(user), expect.anything());
  });

  it('should GET for getFriends', () => {
    service.getFriends();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Plex/Friends', expect.anything());
  });

  it('should POST for oAuth', () => {
    const wizard = { pin: { id: 123 } } as any;
    service.oAuth(wizard);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Plex/oauth', JSON.stringify(wizard), expect.anything());
  });

  it('should GET for getWatchlistUsers', () => {
    service.getWatchlistUsers();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Plex/WatchlistUsers', expect.anything());
  });

  it('should POST for revalidateWatchlistUsers', () => {
    service.revalidateWatchlistUsers();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Plex/WatchlistUsers/revalidate', {}, expect.anything());
  });
});
