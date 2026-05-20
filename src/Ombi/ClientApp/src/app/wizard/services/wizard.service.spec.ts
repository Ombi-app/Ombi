import { describe, it, expect, vi, beforeEach } from 'vitest';
import { WizardService } from './wizard.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    post: vi.fn().mockReturnValue(of({})),
  };
  const service = Object.create(WizardService.prototype);
  service.http = mockHttp;
  service.url = '/api/v2/wizard/';
  service.headers = { set: vi.fn() };
  return { service: service as WizardService, mockHttp };
}

describe('WizardService', () => {
  let service: WizardService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should POST for addOmbiConfig', () => {
    const config = { applicationName: 'Ombi', applicationUrl: 'http://localhost' } as any;
    service.addOmbiConfig(config);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/wizard/config', config, expect.anything());
  });

  it('should POST for addDatabaseConfig', () => {
    const config = { ombiDatabase: 'sqlite' } as any;
    service.addDatabaseConfig(config);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/wizard/database', config, expect.anything());
  });
});
