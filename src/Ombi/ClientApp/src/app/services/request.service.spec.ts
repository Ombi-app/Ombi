import { describe, it, expect, beforeEach, vi } from 'vitest';
import { RequestService } from './request.service';
import { of } from 'rxjs';

function createMockRequestService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of({})),
    post: vi.fn().mockReturnValue(of({})),
    put: vi.fn().mockReturnValue(of({})),
    delete: vi.fn().mockReturnValue(of({})),
  };

  const service = Object.create(RequestService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/Request/';
  service.headers = {};

  return { service: service as RequestService, mockHttp };
}

describe('RequestService', () => {
  let service: RequestService;
  let mockHttp: ReturnType<typeof createMockRequestService>['mockHttp'];

  beforeEach(() => {
    const mocks = createMockRequestService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  describe('Movie endpoints', () => {
    it('should POST for requestMovie', () => {
      const movie = { theMovieDbId: 550 } as any;
      service.requestMovie(movie);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v1/Request/Movie/',
        JSON.stringify(movie),
        expect.anything()
      );
    });

    it('should POST for approveMovie', () => {
      const movie = { id: 1, is4K: false } as any;
      service.approveMovie(movie);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v1/Request/Movie/Approve',
        JSON.stringify(movie),
        expect.anything()
      );
    });

    it('should PUT for denyMovie', () => {
      const movie = { id: 1, reason: 'test' } as any;
      service.denyMovie(movie);
      expect(mockHttp.put).toHaveBeenCalledWith(
        '/api/v1/Request/Movie/Deny',
        JSON.stringify(movie),
        expect.anything()
      );
    });

    it('should POST for markMovieAvailable', () => {
      service.markMovieAvailable({ id: 1, is4K: false } as any);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v1/Request/Movie/available',
        expect.any(String),
        expect.anything()
      );
    });

    it('should POST for markMovieUnavailable', () => {
      service.markMovieUnavailable({ id: 1, is4K: false } as any);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v1/Request/Movie/unavailable',
        expect.any(String),
        expect.anything()
      );
    });

    it('should GET for getTotalMovies', () => {
      service.getTotalMovies();
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v1/Request/Movie/total',
        expect.anything()
      );
    });

    it('should GET for getRemainingMovieRequests', () => {
      service.getRemainingMovieRequests();
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v1/Request/movie/remaining',
        expect.anything()
      );
    });

    it('should GET for searchMovieRequests', () => {
      service.searchMovieRequests('Inception');
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v1/Request/movie/search/Inception',
        expect.anything()
      );
    });

    it('should GET for getMovieRequest by id', () => {
      mockHttp.get.mockReturnValue(of({}));
      service.getMovieRequest(42);
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v1/Request/movie/info/42',
        expect.anything()
      );
    });

    it('should DELETE for removeMovieRequestAsync', () => {
      service.removeMovieRequestAsync(99);
      expect(mockHttp.delete).toHaveBeenCalledWith(
        '/api/v1/Request/movie/99',
        expect.anything()
      );
    });

    it('should PUT for updateMovieRequest', () => {
      const request = { id: 1, title: 'Test' } as any;
      service.updateMovieRequest(request);
      expect(mockHttp.put).toHaveBeenCalledWith(
        '/api/v1/Request/movie/',
        JSON.stringify(request),
        expect.anything()
      );
    });

    it('should POST for subscribeToMovie', () => {
      service.subscribeToMovie(123);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v1/Request/movie/subscribe/123',
        expect.anything()
      );
    });

    it('should POST for unSubscribeToMovie', () => {
      service.unSubscribeToMovie(123);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v1/Request/movie/unsubscribe/123',
        expect.anything()
      );
    });
  });

  describe('TV endpoints', () => {
    it('should GET for getRemainingTvRequests', () => {
      service.getRemainingTvRequests();
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v1/Request/tv/remaining',
        expect.anything()
      );
    });

    it('should GET for getTotalTv', () => {
      service.getTotalTv();
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v1/Request/tv/total',
        expect.anything()
      );
    });

    it('should GET for getChildRequests', () => {
      service.getChildRequests(55);
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v1/Request/tv/55/child',
        expect.anything()
      );
    });

    it('should GET for searchTvRequests', () => {
      service.searchTvRequests('Breaking Bad');
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v1/Request/tv/search/Breaking Bad',
        expect.anything()
      );
    });

    it('should POST for approveChild', () => {
      const child = { id: 1 } as any;
      service.approveChild(child);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v1/Request/tv/approve',
        JSON.stringify(child),
        expect.anything()
      );
    });

    it('should PUT for denyChild', () => {
      const child = { id: 1, reason: 'test' } as any;
      service.denyChild(child);
      expect(mockHttp.put).toHaveBeenCalledWith(
        '/api/v1/Request/tv/deny',
        JSON.stringify(child),
        expect.anything()
      );
    });

    it('should DELETE for deleteChild', () => {
      service.deleteChild(77);
      expect(mockHttp.delete).toHaveBeenCalledWith(
        '/api/v1/Request/tv/child/77',
        expect.anything()
      );
    });

    it('should POST for subscribeToTv', () => {
      service.subscribeToTv(88);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v1/Request/tv/subscribe/88',
        expect.anything()
      );
    });

    it('should PUT for setQualityProfile', () => {
      service.setQualityProfile(10, 5);
      expect(mockHttp.put).toHaveBeenCalledWith(
        '/api/v1/Request/tv/quality/10/5',
        expect.anything()
      );
    });

    it('should PUT for setRootFolder', () => {
      service.setRootFolder(10, 3);
      expect(mockHttp.put).toHaveBeenCalledWith(
        '/api/v1/Request/tv/root/10/3',
        expect.anything()
      );
    });
  });

  describe('Music endpoints', () => {
    it('should POST for requestAlbum', () => {
      const album = { foreignAlbumId: 'abc' } as any;
      service.requestAlbum(album);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v1/Request/music/',
        JSON.stringify(album),
        expect.anything()
      );
    });

    it('should GET for getRemainingMusicRequests', () => {
      service.getRemainingMusicRequests();
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v1/Request/music/remaining',
        expect.anything()
      );
    });

    it('should GET for getTotalAlbums', () => {
      service.getTotalAlbums();
      expect(mockHttp.get).toHaveBeenCalledWith(
        '/api/v1/Request/music/total',
        expect.anything()
      );
    });

    it('should POST for approveAlbum', () => {
      service.approveAlbum({ id: 1 } as any);
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v1/Request/music/Approve',
        expect.any(String),
        expect.anything()
      );
    });

    it('should PUT for denyAlbum', () => {
      service.denyAlbum({ id: 1, reason: 'nope' } as any);
      expect(mockHttp.put).toHaveBeenCalledWith(
        '/api/v1/Request/music/Deny',
        expect.any(String),
        expect.anything()
      );
    });

    it('should DELETE for removeAlbumRequest', () => {
      service.removeAlbumRequest(42);
      expect(mockHttp.delete).toHaveBeenCalledWith(
        '/api/v1/Request/music/42',
        expect.anything()
      );
    });
  });
});
