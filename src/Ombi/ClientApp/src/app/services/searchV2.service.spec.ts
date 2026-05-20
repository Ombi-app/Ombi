import { describe, it, expect, beforeEach, vi } from 'vitest';
import { SearchV2Service } from './searchV2.service';
import { of } from 'rxjs';

function createMockSearchV2Service() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
    post: vi.fn().mockReturnValue(of([])),
  };

  const service = Object.create(SearchV2Service.prototype);
  service.http = mockHttp;
  service.url = '/api/v2/search';
  service.headers = {};

  return { service: service as SearchV2Service, mockHttp };
}

describe('SearchV2Service', () => {
  let service: SearchV2Service;
  let mockHttp: ReturnType<typeof createMockSearchV2Service>['mockHttp'];

  beforeEach(() => {
    const mocks = createMockSearchV2Service();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  describe('Movie endpoints', () => {
    it('should call correct URL for popularMovies', () => {
      service.popularMovies();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/Popular');
    });

    it('should call correct URL for popularMoviesByPage', () => {
      mockHttp.get.mockReturnValue(of([]));
      service.popularMoviesByPage(0, 20);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/Popular/0/20');
    });

    it('should call correct URL for upcomingMovies', () => {
      service.upcomingMovies();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/upcoming');
    });

    it('should call correct URL for upcomingMoviesByPage', () => {
      mockHttp.get.mockReturnValue(of([]));
      service.upcomingMoviesByPage(10, 20);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/upcoming/10/20');
    });

    it('should call correct URL for nowPlayingMovies', () => {
      service.nowPlayingMovies();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/nowplaying');
    });

    it('should call correct URL for nowPlayingMoviesByPage', () => {
      mockHttp.get.mockReturnValue(of([]));
      service.nowPlayingMoviesByPage(0, 10);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/nowplaying/0/10');
    });

    it('should call correct URL for topRatedMovies', () => {
      service.topRatedMovies();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/toprated');
    });

    it('should call correct URL for recentlyRequestedMoviesByPage', () => {
      mockHttp.get.mockReturnValue(of([]));
      service.recentlyRequestedMoviesByPage(0, 20);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/requested/0/20');
    });

    it('should call correct URL for seasonalMoviesByPage', () => {
      mockHttp.get.mockReturnValue(of([]));
      service.seasonalMoviesByPage(0, 20);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/seasonal/0/20');
    });

    it('should call correct URL for getFullMovieDetails', () => {
      service.getFullMovieDetails(550);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/550');
    });

    it('should call correct URL for getMovieByImdbId', () => {
      service.getMovieByImdbId('tt1234567');
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/imdb/tt1234567');
    });

    it('should call correct URL for similarMovies', () => {
      service.similarMovies(550, 'en');
      expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/search/Movie/similar', { theMovieDbId: 550, languageCode: 'en' });
    });
  });

  describe('TV endpoints', () => {
    it('should call correct URL for popularTv', () => {
      service.popularTv();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/popular', expect.anything());
    });

    it('should call correct URL for popularTvByPage', () => {
      mockHttp.get.mockReturnValue(of([]));
      service.popularTvByPage(0, 20);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/popular/0/20', expect.anything());
    });

    it('should call correct URL for trendingTv', () => {
      service.trendingTv();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/trending', expect.anything());
    });

    it('should call correct URL for anticipatedTv', () => {
      service.anticipatedTv();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/anticipated', expect.anything());
    });

    it('should call correct URL for getTvInfo', () => {
      mockHttp.get.mockReturnValue(of({}));
      service.getTvInfo(12345);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/12345', expect.anything());
    });

    it('should call correct URL for getTvInfoWithMovieDbId', () => {
      mockHttp.get.mockReturnValue(of({}));
      service.getTvInfoWithMovieDbId(67890);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/moviedb/67890', expect.anything());
    });
  });

  describe('Multi-search', () => {
    it('should POST to multi search endpoint with encoded term', () => {
      const filter = { movies: true, tvShows: true, music: false, people: false };
      service.multiSearch('test query', filter as any);
      expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/search/multi/test%20query', filter);
    });

    it('should encode special characters in search term', () => {
      const filter = { movies: true, tvShows: false, music: false, people: false };
      service.multiSearch('test&query=1', filter as any);
      expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/search/multi/test%26query%3D1', filter);
    });
  });

  describe('Other endpoints', () => {
    it('should call correct URL for getGenres', () => {
      service.getGenres('movie');
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Genres/movie', expect.anything());
    });

    it('should call correct URL for getLanguages', () => {
      service.getLanguages();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Languages', expect.anything());
    });

    it('should call correct URL for getMovieCollections', () => {
      mockHttp.get.mockReturnValue(of({}));
      service.getMovieCollections(100);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/movie/collection/100', expect.anything());
    });

    it('should call correct URL for getMoviesByActor', () => {
      service.getMoviesByActor(999);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/actor/999/movie', expect.anything());
    });

    it('should call correct URL for getTvByActor', () => {
      service.getTvByActor(999);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/actor/999/tv', expect.anything());
    });

    it('should call correct URL for getRottenMovieRatings', () => {
      service.getRottenMovieRatings('Inception', 2010);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/ratings/movie/Inception/2010');
    });

    it('should call correct URL for getMovieStreams', () => {
      service.getMovieStreams(550);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/stream/movie/550');
    });

    it('should call correct URL for getTvStreams', () => {
      service.getTvStreams(1234);
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/stream/tv/1234');
    });

    it('should POST for advancedSearch', () => {
      mockHttp.post.mockReturnValue(of([]));
      const model = { keywordIds: [1], genreIds: [2] } as any;
      service.advancedSearch(model, 0, 20);
      expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/search/advancedSearch/Movie/0/20', model);
    });
  });
});
