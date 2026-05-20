import { describe, it, expect, vi, beforeEach } from 'vitest';
import { CustomPageService } from './custompage.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of({})),
    post: vi.fn().mockReturnValue(of(true)),
  };

  const service = Object.create(CustomPageService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/CustomPage';
  service.headers = { set: vi.fn() };

  return { service: service as CustomPageService, mockHttp };
}

describe('CustomPageService', () => {
  let service: CustomPageService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should call GET for getCustomPage', () => {
    service.getCustomPage();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/CustomPage', expect.anything());
  });

  it('should call POST for saveCustomPage', () => {
    const model = { enabled: true, fontColor: '#fff', html: '<p>Hi</p>' } as any;
    service.saveCustomPage(model);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/CustomPage', model, expect.anything());
  });
});
