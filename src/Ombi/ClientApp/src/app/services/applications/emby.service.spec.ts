import { describe, it, expect, vi, beforeEach } from 'vitest';
import { EmbyService } from './emby.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
    post: vi.fn().mockReturnValue(of({})),
  };
  const service = Object.create(EmbyService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/Emby/';
  service.headers = { set: vi.fn() };
  return { service: service as EmbyService, mockHttp };
}

describe('EmbyService', () => {
  let service: EmbyService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should POST for logIn', () => {
    const settings = { enable: true } as any;
    service.logIn(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Emby/', JSON.stringify(settings), expect.anything());
  });

  it('should GET for getUsers', () => {
    service.getUsers();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Emby/users', expect.anything());
  });

  it('should POST for getPublicInfo', () => {
    const server = { name: 'My Emby' } as any;
    service.getPublicInfo(server);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Emby/info', JSON.stringify(server), expect.anything());
  });

  it('should POST for getLibraries', () => {
    const settings = { name: 'My Emby' } as any;
    service.getLibraries(settings);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Emby/Library', JSON.stringify(settings), expect.anything());
  });
});
