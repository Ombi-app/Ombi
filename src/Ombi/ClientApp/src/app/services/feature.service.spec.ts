import { describe, it, expect, vi, beforeEach } from 'vitest';
import { FeatureService } from './feature.service';
import { of } from 'rxjs';

function createService(baseHref = '/') {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
    post: vi.fn().mockReturnValue(of([])),
  };

  const service = Object.create(FeatureService.prototype);
  service.http = mockHttp;
  service.url = baseHref.length > 1 ? baseHref + '/api/v2/Features/' : '/api/v2/Features/';
  service.headers = { set: vi.fn() };

  return { service: service as FeatureService, mockHttp };
}

describe('FeatureService', () => {
  let service: FeatureService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should call GET for getFeatures', () => {
    service.getFeatures();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Features/', expect.anything());
  });

  it('should call POST for enable', () => {
    const feature = { id: 1, name: 'Movie4K', enabled: true } as any;
    service.enable(feature);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/Features/enable', JSON.stringify(feature), expect.anything());
  });

  it('should call POST for disable', () => {
    const feature = { id: 2, name: 'Movie4K', enabled: false } as any;
    service.disable(feature);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/Features/disable', JSON.stringify(feature), expect.anything());
  });
});
