import { describe, it, expect, vi, beforeEach } from 'vitest';
import { JobService } from './job.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of(true)),
    post: vi.fn().mockReturnValue(of(true)),
  };

  const service = Object.create(JobService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/Job/';
  service.headers = { set: vi.fn() };

  return { service: service as JobService, mockHttp };
}

describe('JobService', () => {
  let service: JobService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should call POST for forceUpdate', () => {
    service.forceUpdate();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Job/update/', expect.anything());
  });

  it('should call GET for checkForNewUpdate', () => {
    service.checkForNewUpdate();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Job/update/', expect.anything());
  });

  it('should call GET for getCachedUpdate', () => {
    service.getCachedUpdate();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Job/updateCached/', expect.anything());
  });

  it('should call POST for runPlexImporter', () => {
    service.runPlexImporter();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Job/plexUserImporter/', expect.anything());
  });

  it('should call POST for runPlexWatchlistImport', () => {
    service.runPlexWatchlistImport();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Job/plexwatchlist/', expect.anything());
  });

  it('should call POST for runEmbyImporter', () => {
    service.runEmbyImporter();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Job/embyUserImporter/', expect.anything());
  });

  it('should call POST for runJellyfinImporter', () => {
    service.runJellyfinImporter();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Job/jellyfinUserImporter/', expect.anything());
  });

  it('should call POST for runPlexCacher', () => {
    service.runPlexCacher();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Job/plexcontentcacher/', expect.anything());
  });

  it('should call POST for runPlexRecentlyAddedCacher', () => {
    service.runPlexRecentlyAddedCacher();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Job/plexrecentlyadded/', expect.anything());
  });

  it('should call POST for runEmbyRecentlyAddedCacher', () => {
    service.runEmbyRecentlyAddedCacher();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Job/embyrecentlyadded/', expect.anything());
  });

  it('should call POST for clearMediaserverData', () => {
    service.clearMediaserverData();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Job/clearmediaserverdata/', expect.anything());
  });

  it('should call POST for runEmbyCacher', () => {
    service.runEmbyCacher();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Job/embycontentcacher/', expect.anything());
  });

  it('should call POST for runJellyfinCacher', () => {
    service.runJellyfinCacher();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Job/jellyfincontentcacher/', expect.anything());
  });

  it('should call POST for runNewsletter', () => {
    service.runNewsletter();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Job/newsletter/', expect.anything());
  });

  it('should call POST for runArrAvailabilityChecker', () => {
    service.runArrAvailabilityChecker();
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Job/arrAvailability/', expect.anything());
  });
});
