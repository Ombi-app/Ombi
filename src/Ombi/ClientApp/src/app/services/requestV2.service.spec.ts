import { describe, it, expect, beforeEach, vi } from 'vitest';
import { RequestServiceV2 } from './requestV2.service';
import { of } from 'rxjs';

function createMockRequestV2Service() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of({})),
    post: vi.fn().mockReturnValue(of({})),
  };

  const service = Object.create(RequestServiceV2.prototype);
  service.http = mockHttp;
  service.url = '/api/v2/Requests/';
  service.headers = {};

  return { service: service as RequestServiceV2, mockHttp };
}

describe('RequestServiceV2', () => {
  let service: RequestServiceV2;
  let mockHttp: ReturnType<typeof createMockRequestV2Service>['mockHttp'];

  beforeEach(() => {
    const mocks = createMockRequestV2Service();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  describe('Movie request endpoints', () => {
    it('should call correct URL for getMovieRequests', () => {
      service.getMovieRequests(10, 0, 'requestedDate', 'desc');
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v2/Requests/movie/10/0/requestedDate/desc',
        expect.anything()
      );
    });

    it('should call correct URL for getMovieAvailableRequests', () => {
      service.getMovieAvailableRequests(10, 0, 'title', 'asc');
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v2/Requests/movie/available/10/0/title/asc',
        expect.anything()
      );
    });

    it('should call correct URL for getMoviePendingRequests', () => {
      service.getMoviePendingRequests(10, 0, 'title', 'asc');
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v2/Requests/movie/pending/10/0/title/asc',
        expect.anything()
      );
    });

    it('should call correct URL for getMovieDeniedRequests', () => {
      service.getMovieDeniedRequests(10, 0, 'title', 'asc');
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v2/Requests/movie/denied/10/0/title/asc',
        expect.anything()
      );
    });

    it('should call correct URL for getMovieProcessingRequests', () => {
      service.getMovieProcessingRequests(10, 0, 'title', 'asc');
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v2/Requests/movie/processing/10/0/title/asc',
        expect.anything()
      );
    });

    it('should call correct URL for getMovieUnavailableRequests', () => {
      service.getMovieUnavailableRequests(10, 0, 'title', 'asc');
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v2/Requests/movie/unavailable/10/0/title/asc',
        expect.anything()
      );
    });
  });

  describe('TV request endpoints', () => {
    it('should call correct URL for getTvRequests', () => {
      service.getTvRequests(10, 0, 'title', 'asc');
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v2/Requests/tv/10/0/title/asc',
        expect.anything()
      );
    });

    it('should call correct URL for getPendingTvRequests', () => {
      service.getPendingTvRequests(10, 0, 'title', 'asc');
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v2/Requests/tv/pending/10/0/title/asc',
        expect.anything()
      );
    });

    it('should call correct URL for getDeniedTvRequests', () => {
      service.getDeniedTvRequests(10, 0, 'title', 'asc');
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v2/Requests/tv/denied/10/0/title/asc',
        expect.anything()
      );
    });
  });

  describe('Album request endpoints', () => {
    it('should call correct URL for getAlbumRequests', () => {
      service.getAlbumRequests(10, 0, 'title', 'asc');
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v2/Requests/Album/10/0/title/asc',
        expect.anything()
      );
    });
  });

  describe('Action endpoints', () => {
    it('should POST for requestTv', () => {
      const tvRequest = { theMovieDbId: 123 } as any;
      service.requestTv(tvRequest);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v2/Requests/TV/',
        JSON.stringify(tvRequest),
        expect.anything()
      );
    });

    it('should POST for requestMovieCollection', () => {
      service.requestMovieCollection(456);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v2/Requests/movie/collection/456',
        undefined,
        expect.anything()
      );
    });

    it('should GET for getRecentlyRequested', () => {
      service.getRecentlyRequested();
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v2/Requests/recentlyRequested',
        expect.anything()
      );
    });

    it('should POST for updateMovieAdvancedOptions', () => {
      const options = { requestId: 1 } as any;
      service.updateMovieAdvancedOptions(options);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v2/Requests/movie/advancedoptions',
        options,
        expect.anything()
      );
    });

    it('should POST for reprocessRequest', () => {
      service.reprocessRequest(1, 1 as any, false);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v2/Requests/reprocess/1/1/false',
        undefined,
        expect.anything()
      );
    });
  });
});
