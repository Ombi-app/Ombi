import { describe, it, expect, vi } from 'vitest';
import { HubService } from './hub.service';
import { of } from 'rxjs';

describe('HubService', () => {
  it('should GET connected users', async () => {
    const mockHttp = { get: vi.fn().mockReturnValue(of([{ userId: '1', displayName: 'Admin' }])) };
    const service = Object.create(HubService.prototype);
    service.http = mockHttp;
    service.url = '/api/v2/hub/';
    service.headers = {};
    const result = await service.getConnectedUsers();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/hub/users', expect.anything());
    expect(result).toHaveLength(1);
  });
});
