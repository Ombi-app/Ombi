import { describe, it, expect, vi, beforeEach } from 'vitest';
import { SystemService } from './system.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
  };
  const service = Object.create(SystemService.prototype);
  service.http = mockHttp;
  service.url = '/api/v2/system/';
  service.headers = { set: vi.fn() };
  return { service: service as SystemService, mockHttp };
}

describe('SystemService', () => {
  let service: SystemService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should GET for getAvailableLogs', () => {
    service.getAvailableLogs();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/system/logs/', expect.anything());
  });

  it('should GET for getLog with log name and responseType text', () => {
    service.getLog('ombi.log');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/system/logs/ombi.log', { responseType: 'text' });
  });

  it('should GET for getNews with responseType text', () => {
    service.getNews();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/system/news', { responseType: 'text' });
  });
});
