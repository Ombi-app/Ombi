import { describe, it, expect, vi, beforeEach } from 'vitest';
import { LandingPageService } from './landingpage.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of({})),
  };
  const service = Object.create(LandingPageService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/LandingPage/';
  service.headers = { set: vi.fn() };
  return { service: service as LandingPageService, mockHttp };
}

describe('LandingPageService', () => {
  let service: LandingPageService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should GET for getServerStatus', () => {
    service.getServerStatus();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/LandingPage/', expect.anything());
  });
});
