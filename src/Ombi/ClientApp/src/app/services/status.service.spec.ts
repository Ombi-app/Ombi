import { describe, it, expect, vi, beforeEach } from 'vitest';
import { StatusService } from './status.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of({ result: true })),
  };

  const service = Object.create(StatusService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/status/';
  service.headers = { set: vi.fn() };

  return { service: service as StatusService, mockHttp };
}

describe('StatusService', () => {
  let service: StatusService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should call GET for getWizardStatus', () => {
    service.getWizardStatus();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/status/Wizard/', expect.anything());
  });

  it('should return the wizard result', () => {
    mockHttp.get.mockReturnValue(of({ result: false }));
    return new Promise<void>((resolve) => {
      service.getWizardStatus().subscribe((res) => {
        expect(res.result).toBe(false);
        resolve();
      });
    });
  });
});
