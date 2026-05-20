import { describe, it, expect, vi, beforeEach } from 'vitest';
import { CalendarService } from './calendar.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
  };
  const service = Object.create(CalendarService.prototype);
  service.http = mockHttp;
  service.url = '/api/v2/Calendar/';
  service.headers = { set: vi.fn() };
  return { service: service as CalendarService, mockHttp };
}

describe('CalendarService', () => {
  let service: CalendarService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should GET for getCalendarEntries', async () => {
    await service.getCalendarEntries();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Calendar/', expect.anything());
  });
});
