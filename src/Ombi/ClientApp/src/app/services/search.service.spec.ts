import { describe, it, expect, vi, beforeEach } from 'vitest';
import { SearchService } from './search.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
    post: vi.fn().mockReturnValue(of([])),
  };

  const service = Object.create(SearchService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/search';
  service.headers = { set: vi.fn() };

  return { service: service as SearchService, mockHttp };
}

describe('SearchService', () => {
  let service: SearchService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  // Movie endpoints
  it('should call GET for searchMovie', () => {
    service.searchMovie('Inception');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Movie/Inception');
  });

  it('should call POST for searchMovieWithRefined', () => {
    service.searchMovieWithRefined('Inception', 2010, 'en');
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/search/Movie/', { searchTerm: 'Inception', year: 2010, languageCode: 'en' });
  });

  it('should call POST for similarMovies', () => {
    service.similarMovies(550, 'en');
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/search/Movie/similar', { theMovieDbId: 550, languageCode: 'en' });
  });

  it('should call GET for popularMovies', () => {
    service.popularMovies();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Movie/Popular');
  });

  it('should call GET for upcomingMovies', () => {
    service.upcomingMovies();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Movie/upcoming');
  });

  it('should call GET for nowPlayingMovies', () => {
    service.nowPlayingMovies();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Movie/nowplaying');
  });

  it('should call GET for topRatedMovies', () => {
    service.topRatedMovies();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Movie/toprated');
  });

  it('should call GET for getMovieInformation', () => {
    service.getMovieInformation(550);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Movie/info/550');
  });

  it('should call POST for getMovieInformationWithRefined', () => {
    service.getMovieInformationWithRefined(550, 'fr');
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/search/Movie/info', { theMovieDbId: 550, languageCode: 'fr' });
  });

  it('should call POST for searchMovieByActor', () => {
    service.searchMovieByActor('Tom Hanks', 'en');
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/search/Movie/Actor', { searchTerm: 'Tom Hanks', languageCode: 'en' });
  });

  // TV endpoints
  it('should call GET for searchTv', () => {
    service.searchTv('Breaking Bad');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Tv/Breaking Bad', expect.anything());
  });

  it('should call GET for searchTvTreeNode', () => {
    service.searchTvTreeNode('Lost');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Tv/Lost/tree', expect.anything());
  });

  it('should call GET for getShowInformationTreeNode', () => {
    service.getShowInformationTreeNode(1396);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Tv/info/1396/Tree', expect.anything());
  });

  it('should call GET for getShowInformation', () => {
    service.getShowInformation(1396);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Tv/info/1396', expect.anything());
  });

  it('should call GET for popularTv', () => {
    service.popularTv();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Tv/popular', expect.anything());
  });

  it('should call GET for mostWatchedTv', () => {
    service.mostWatchedTv();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Tv/mostwatched', expect.anything());
  });

  it('should call GET for anticipatedTv', () => {
    service.anticipatedTv();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Tv/anticipated', expect.anything());
  });

  it('should call GET for trendingTv', () => {
    service.trendingTv();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Tv/trending', expect.anything());
  });

  // Music endpoints
  it('should call GET for searchArtist', () => {
    service.searchArtist('Radiohead');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Music/Artist/Radiohead');
  });

  it('should call GET for searchAlbum', () => {
    service.searchAlbum('OK Computer');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Music/Album/OK Computer');
  });

  it('should call GET for getAlbumInformation', () => {
    service.getAlbumInformation('abc-123');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Music/Album/info/abc-123');
  });

  it('should call GET for getAlbumsForArtist', () => {
    service.getAlbumsForArtist('artist-456');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/search/Music/Artist/Album/artist-456');
  });
});
