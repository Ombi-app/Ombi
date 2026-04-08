import { describe, it, expect, vi, beforeEach } from 'vitest';
import { UpdateService } from './update.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of({})),
  };
  const service = Object.create(UpdateService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/Update/';
  service.headers = { set: vi.fn() };
  return { service: service as UpdateService, mockHttp };
}

describe('UpdateService', () => {
  let service: UpdateService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should GET for checkForUpdate', () => {
    service.checkForUpdate();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Update/', expect.anything());
  });
});
