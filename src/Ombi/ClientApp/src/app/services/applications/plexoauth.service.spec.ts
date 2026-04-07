import { describe, it, expect, vi, beforeEach } from 'vitest';
import { PlexOAuthService } from './plexoauth.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of({})),
  };
  const service = Object.create(PlexOAuthService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/PlexOAuth/';
  service.headers = { set: vi.fn() };
  return { service: service as PlexOAuthService, mockHttp };
}

describe('PlexOAuthService', () => {
  let service: PlexOAuthService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should GET for oAuth with pin', () => {
    service.oAuth(12345);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/PlexOAuth/12345', expect.anything());
  });
});
