import { describe, it, expect, vi, beforeEach } from 'vitest';
import { RadarrService } from './radarr.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
    post: vi.fn().mockReturnValue(of([])),
  };
  const service = Object.create(RadarrService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/Radarr';
  service.headers = { set: vi.fn() };
  return { service: service as RadarrService, mockHttp };
}

describe('RadarrService', () => {
  let service: RadarrService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should POST for getRootFolders with settings', () => {
    const settings = { ip: '127.0.0.1', apiKey: 'abc' } as any;
    service.getRootFolders(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Radarr/RootFolders/', JSON.stringify(settings), expect.anything());
  });

  it('should POST for getQualityProfiles with settings', () => {
    const settings = { ip: '127.0.0.1' } as any;
    service.getQualityProfiles(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Radarr/Profiles/', JSON.stringify(settings), expect.anything());
  });

  it('should GET for getRootFoldersFromSettings', () => {
    service.getRootFoldersFromSettings();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Radarr/RootFolders/', expect.anything());
  });

  it('should GET for getQualityProfilesFromSettings', () => {
    service.getQualityProfilesFromSettings();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Radarr/Profiles/', expect.anything());
  });

  it('should GET for getRootFolders4kFromSettings', () => {
    service.getRootFolders4kFromSettings();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Radarr/RootFolders/4k', expect.anything());
  });

  it('should GET for getQualityProfiles4kFromSettings', () => {
    service.getQualityProfiles4kFromSettings();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Radarr/Profiles/4k', expect.anything());
  });

  it('should GET for isRadarrEnabled', () => {
    service.isRadarrEnabled();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Radarr/enabled/', expect.anything());
  });

  it('should POST for getTagsWithSettings', () => {
    const settings = { ip: '127.0.0.1' } as any;
    service.getTagsWithSettings(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Radarr/tags/', JSON.stringify(settings), expect.anything());
  });

  it('should GET for getTags', () => {
    service.getTags();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Radarr/tags/', expect.anything());
  });
});
