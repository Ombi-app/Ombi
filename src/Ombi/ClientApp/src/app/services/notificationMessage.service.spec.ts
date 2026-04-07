import { describe, it, expect, vi, beforeEach } from 'vitest';
import { NotificationMessageService } from './notificationMessage.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    post: vi.fn().mockReturnValue(of(true)),
  };
  const service = Object.create(NotificationMessageService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/notifications/';
  service.headers = { set: vi.fn() };
  return { service: service as NotificationMessageService, mockHttp };
}

describe('NotificationMessageService', () => {
  let service: NotificationMessageService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should POST for sendMassEmail', () => {
    const model = { subject: 'Test', body: 'Hello', users: [] } as any;
    service.sendMassEmail(model);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/notifications/massemail/', JSON.stringify(model), expect.anything());
  });
});
