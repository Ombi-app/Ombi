import { describe, it, expect, vi, beforeEach } from 'vitest';
import { RequestServiceV2 } from './requestV2.service';
import { RequestType } from '../interfaces';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of({})),
    post: vi.fn().mockReturnValue(of({})),
  };
  const service = Object.create(RequestServiceV2.prototype);
  service.http = mockHttp;
  service.url = '/api/v2/Requests/';
  service.headers = { set: vi.fn() };
  return { service: service as RequestServiceV2, mockHttp };
}

describe('RequestServiceV2', () => {
  let service: RequestServiceV2;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => { const m = createService(); service = m.service; mockHttp = m.mockHttp; });

  // Movie requests
  it('should GET getMovieRequests', () => { service.getMovieRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/movie/10/0/title/asc', expect.anything()); });
  it('should GET getMovieAvailableRequests', () => { service.getMovieAvailableRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/movie/available/10/0/title/asc', expect.anything()); });
  it('should GET getMovieProcessingRequests', () => { service.getMovieProcessingRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/movie/processing/10/0/title/asc', expect.anything()); });
  it('should GET getMoviePendingRequests', () => { service.getMoviePendingRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/movie/pending/10/0/title/asc', expect.anything()); });
  it('should GET getMovieDeniedRequests', () => { service.getMovieDeniedRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/movie/denied/10/0/title/asc', expect.anything()); });
  it('should GET getMovieUnavailableRequests', () => { service.getMovieUnavailableRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/movie/unavailable/10/0/title/asc', expect.anything()); });

  // TV requests
  it('should GET getTvRequests', () => { service.getTvRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/tv/10/0/title/asc', expect.anything()); });
  it('should GET getPendingTvRequests', () => { service.getPendingTvRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/tv/pending/10/0/title/asc', expect.anything()); });
  it('should GET getProcessingTvRequests', () => { service.getProcessingTvRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/tv/processing/10/0/title/asc', expect.anything()); });
  it('should GET getAvailableTvRequests', () => { service.getAvailableTvRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/tv/available/10/0/title/asc', expect.anything()); });
  it('should GET getDeniedTvRequests', () => { service.getDeniedTvRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/tv/denied/10/0/title/asc', expect.anything()); });
  it('should GET getTvUnavailableRequests', () => { service.getTvUnavailableRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/tv/unavailable/10/0/title/asc', expect.anything()); });

  // Album requests
  it('should GET getAlbumRequests', () => { service.getAlbumRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/Album/10/0/title/asc', expect.anything()); });
  it('should GET getAlbumAvailableRequests', () => { service.getAlbumAvailableRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/Album/available/10/0/title/asc', expect.anything()); });
  it('should GET getAlbumProcessingRequests', () => { service.getAlbumProcessingRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/Album/processing/10/0/title/asc', expect.anything()); });
  it('should GET getAlbumPendingRequests', () => { service.getAlbumPendingRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/Album/pending/10/0/title/asc', expect.anything()); });
  it('should GET getAlbumDeniedRequests', () => { service.getAlbumDeniedRequests(10, 0, 'title', 'asc'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/Album/denied/10/0/title/asc', expect.anything()); });

  // Actions
  it('should POST updateMovieAdvancedOptions', () => { service.updateMovieAdvancedOptions({ requestId: 1 } as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/Requests/movie/advancedoptions', expect.anything(), expect.anything()); });
  it('should POST updateTvAdvancedOptions', () => { service.updateTvAdvancedOptions({ requestId: 1 } as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/Requests/tv/advancedoptions', expect.anything(), expect.anything()); });
  it('should POST requestTv', () => { service.requestTv({ theMovieDbId: 1 } as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/Requests/TV/', expect.any(String), expect.anything()); });
  it('should POST reprocessRequest', () => { service.reprocessRequest(42, RequestType.movie, false); expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/Requests/reprocess/1/42/false', undefined, expect.anything()); });
  it('should POST requestMovieCollection', () => { service.requestMovieCollection(100); expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/Requests/movie/collection/100', undefined, expect.anything()); });
  it('should GET getRecentlyRequested', () => { service.getRecentlyRequested(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Requests/recentlyRequested', expect.anything()); });
});
