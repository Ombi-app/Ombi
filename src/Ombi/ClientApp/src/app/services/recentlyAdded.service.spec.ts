import { describe, it, expect, vi, beforeEach } from 'vitest';
import { RecentlyAddedService } from './recentlyAdded.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
  };

  const service = Object.create(RecentlyAddedService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/recentlyadded/';
  service.headers = { set: vi.fn() };

  return { service: service as RecentlyAddedService, mockHttp };
}

describe('RecentlyAddedService', () => {
  let service: RecentlyAddedService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should call GET for getRecentlyAddedMovies', () => {
    service.getRecentlyAddedMovies();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/recentlyadded/movies/', expect.anything());
  });

  it('should call GET for getRecentlyAddedTv', () => {
    service.getRecentlyAddedTv();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/recentlyadded/tv/', expect.anything());
  });

  it('should call GET for getRecentlyAddedTvGrouped', () => {
    service.getRecentlyAddedTvGrouped();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/recentlyadded/tv/grouped', expect.anything());
  });
});
