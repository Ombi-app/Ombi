import { describe, it, expect, vi, beforeEach } from 'vitest';
import { LidarrService } from './lidarr.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
    post: vi.fn().mockReturnValue(of([])),
  };
  const service = Object.create(LidarrService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/Lidarr';
  service.headers = { set: vi.fn() };
  return { service: service as LidarrService, mockHttp };
}

describe('LidarrService', () => {
  let service: LidarrService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should GET for enabled', () => {
    service.enabled();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Lidarr/enabled/', expect.anything());
  });

  it('should POST for getRootFolders with settings', () => {
    const settings = { ip: '127.0.0.1' } as any;
    service.getRootFolders(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Lidarr/RootFolders/', JSON.stringify(settings), expect.anything());
  });

  it('should POST for getQualityProfiles with settings', () => {
    const settings = { ip: '127.0.0.1' } as any;
    service.getQualityProfiles(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Lidarr/Profiles/', JSON.stringify(settings), expect.anything());
  });

  it('should GET for getRootFoldersFromSettings', () => {
    service.getRootFoldersFromSettings();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Lidarr/RootFolders/', expect.anything());
  });

  it('should GET for getQualityProfilesFromSettings', () => {
    service.getQualityProfilesFromSettings();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Lidarr/Profiles/', expect.anything());
  });

  it('should POST for getMetadataProfiles with settings', () => {
    const settings = { ip: '127.0.0.1' } as any;
    service.getMetadataProfiles(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Lidarr/Metadata/', JSON.stringify(settings), expect.anything());
  });
});
