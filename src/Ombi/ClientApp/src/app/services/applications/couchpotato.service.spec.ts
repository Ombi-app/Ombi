import { describe, it, expect, vi, beforeEach } from 'vitest';
import { CouchPotatoService } from './couchpotato.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    post: vi.fn().mockReturnValue(of({})),
  };
  const service = Object.create(CouchPotatoService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/CouchPotato/';
  service.headers = { set: vi.fn() };
  return { service: service as CouchPotatoService, mockHttp };
}

describe('CouchPotatoService', () => {
  let service: CouchPotatoService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should POST for getProfiles', () => {
    const settings = { enabled: true, url: 'http://localhost' } as any;
    service.getProfiles(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/CouchPotato/profile', JSON.stringify(settings), expect.anything());
  });

  it('should POST for getApiKey', () => {
    const settings = { enabled: true, url: 'http://localhost' } as any;
    service.getApiKey(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/CouchPotato/apikey', JSON.stringify(settings), expect.anything());
  });
});
