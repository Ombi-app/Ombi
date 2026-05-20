import { describe, it, expect, vi, beforeEach } from 'vitest';
import { RequestRetryService } from './requestretry.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
    delete: vi.fn().mockReturnValue(of(true)),
  };

  const service = Object.create(RequestRetryService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/requestretry/';
  service.headers = { set: vi.fn() };

  return { service: service as RequestRetryService, mockHttp };
}

describe('RequestRetryService', () => {
  let service: RequestRetryService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should call GET for getFailedRequests', () => {
    service.getFailedRequests();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/requestretry/', expect.anything());
  });

  it('should call DELETE for deleteFailedRequest with correct id', () => {
    service.deleteFailedRequest(42);
    expect(mockHttp.delete).toHaveBeenCalledWith('/api/v1/requestretry/42', expect.anything());
  });
});
