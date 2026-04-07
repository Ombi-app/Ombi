import { describe, it, expect, vi, beforeEach } from 'vitest';
import { SonarrService } from './sonarr.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
    post: vi.fn().mockReturnValue(of([])),
  };
  const service = Object.create(SonarrService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/Sonarr';
  service.headers = { set: vi.fn() };
  return { service: service as SonarrService, mockHttp };
}

describe('SonarrService', () => {
  let service: SonarrService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should POST for getRootFolders with settings', () => {
    const settings = { ip: '127.0.0.1' } as any;
    service.getRootFolders(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Sonarr/RootFolders/', JSON.stringify(settings), expect.anything());
  });

  it('should POST for getQualityProfiles with settings', () => {
    const settings = { ip: '127.0.0.1' } as any;
    service.getQualityProfiles(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Sonarr/Profiles/', JSON.stringify(settings), expect.anything());
  });

  it('should GET for getRootFoldersWithoutSettings', () => {
    service.getRootFoldersWithoutSettings();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Sonarr/RootFolders/', expect.anything());
  });

  it('should GET for getQualityProfilesWithoutSettings', () => {
    service.getQualityProfilesWithoutSettings();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Sonarr/Profiles/', expect.anything());
  });

  it('should POST for getV3LanguageProfiles with settings', () => {
    const settings = { ip: '127.0.0.1' } as any;
    service.getV3LanguageProfiles(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Sonarr/v3/languageprofiles/', JSON.stringify(settings), expect.anything());
  });

  it('should GET for getV3LanguageProfilesWithoutSettings', () => {
    service.getV3LanguageProfilesWithoutSettings();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Sonarr/v3/languageprofiles/', expect.anything());
  });

  it('should POST for getTags with settings', () => {
    const settings = { ip: '127.0.0.1' } as any;
    service.getTags(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Sonarr/tags/', JSON.stringify(settings), expect.anything());
  });

  it('should GET for isEnabled', () => {
    service.isEnabled();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Sonarr/enabled/', expect.anything());
  });

  it('should GET for getVersion', () => {
    service.getVersion();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Sonarr/version/', expect.anything());
  });
});
