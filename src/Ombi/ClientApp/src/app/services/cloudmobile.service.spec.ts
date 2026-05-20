import { describe, it, expect, vi } from 'vitest';
import { CloudMobileService } from './cloudmobile.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
    post: vi.fn().mockReturnValue(of(true)),
  };
  const service = Object.create(CloudMobileService.prototype);
  service.http = mockHttp;
  service.url = '/api/v2/mobile/';
  service.headers = {};
  return { service: service as CloudMobileService, mockHttp };
}

describe('CloudMobileService', () => {
  it('should GET devices', () => {
    const { service, mockHttp } = createService();
    service.getDevices();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/mobile/users/', expect.anything());
  });

  it('should POST send message', async () => {
    const { service, mockHttp } = createService();
    await service.send('user-1', 'Hello');
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/mobile/send/', { userId: 'user-1', message: 'Hello' }, expect.anything());
  });
});
