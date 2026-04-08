import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MobileService } from './mobile.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
    post: vi.fn().mockReturnValue(of(true)),
  };
  const service = Object.create(MobileService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/mobile/';
  service.headers = { set: vi.fn() };
  return { service: service as MobileService, mockHttp };
}

describe('MobileService', () => {
  let service: MobileService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should GET for getUserDeviceList', () => {
    service.getUserDeviceList();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/mobile/notification/', expect.anything());
  });

  it('should POST for deleteUser with userId', () => {
    service.deleteUser('user-abc');
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/mobile/', { userId: 'user-abc' }, expect.anything());
  });
});
