import { describe, it, expect, vi, beforeEach } from 'vitest';
import { SearchV2Service } from './searchV2.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
    post: vi.fn().mockReturnValue(of([])),
  };
  const service = Object.create(SearchV2Service.prototype);
  service.http = mockHttp;
  service.url = '/api/v2/search';
  service.headers = { set: vi.fn() };
  return { service: service as SearchV2Service, mockHttp };
}

describe('SearchV2Service', () => {
  let service: SearchV2Service;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => { const m = createService(); service = m.service; mockHttp = m.mockHttp; });

  it('should POST multiSearch', () => { service.multiSearch('test', {} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/search/multi/test', expect.anything()); });
  it('should GET getGenres', () => { service.getGenres('movie'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Genres/movie', expect.anything()); });
  it('should GET getLanguages', () => { service.getLanguages(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Languages', expect.anything()); });
  it('should GET getFullMovieDetails', () => { service.getFullMovieDetails(550); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/550'); });
  it('should GET getMovieByImdbId', () => { service.getMovieByImdbId('tt0137523'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/imdb/tt0137523'); });
  it('should GET getFullMovieDetailsByRequestId', async () => { await service.getFullMovieDetailsByRequestId(1); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/request/1'); });
  it('should GET getFullMovieDetailsPromise', async () => { await service.getFullMovieDetailsPromise(550); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/550'); });
  it('should POST similarMovies', () => { service.similarMovies(550, 'en'); expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/search/Movie/similar', { theMovieDbId: 550, languageCode: 'en' }); });
  it('should GET popularMovies', () => { service.popularMovies(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/Popular'); });
  it('should GET popularMoviesByPage', async () => { await service.popularMoviesByPage(0, 10); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/Popular/0/10'); });
  it('should POST advancedSearch', async () => { await service.advancedSearch({} as any, 0, 10); expect(mockHttp.post).toHaveBeenCalledWith('/api/v2/search/advancedSearch/Movie/0/10', expect.anything()); });
  it('should GET upcomingMovies', () => { service.upcomingMovies(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/upcoming'); });
  it('should GET upcomingMoviesByPage', async () => { await service.upcomingMoviesByPage(0, 10); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/upcoming/0/10'); });
  it('should GET recentlyRequestedMoviesByPage', async () => { await service.recentlyRequestedMoviesByPage(0, 10); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/requested/0/10'); });
  it('should GET recentlyRequestedTvByPage', async () => { await service.recentlyRequestedTvByPage(0, 10); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/tv/requested/0/10'); });
  it('should GET seasonalMoviesByPage', async () => { await service.seasonalMoviesByPage(0, 10); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/seasonal/0/10'); });
  it('should GET nowPlayingMovies', () => { service.nowPlayingMovies(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/nowplaying'); });
  it('should GET nowPlayingMoviesByPage', async () => { await service.nowPlayingMoviesByPage(0, 10); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/nowplaying/0/10'); });
  it('should GET topRatedMovies', () => { service.topRatedMovies(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Movie/toprated'); });
  it('should GET popularTv', () => { service.popularTv(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/popular', expect.anything()); });
  it('should GET popularTvByPage', async () => { await service.popularTvByPage(0, 10); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/popular/0/10', expect.anything()); });
  it('should GET mostWatchedTv', () => { service.mostWatchedTv(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/mostwatched', expect.anything()); });
  it('should GET anticipatedTv', () => { service.anticipatedTv(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/anticipated', expect.anything()); });
  it('should GET anticipatedTvByPage', async () => { await service.anticipatedTvByPage(0, 10); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/anticipated/0/10', expect.anything()); });
  it('should GET trendingTv', () => { service.trendingTv(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/trending', expect.anything()); });
  it('should GET trendingTvByPage', async () => { await service.trendingTvByPage(0, 10); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/trending/0/10', expect.anything()); });
  it('should GET getTvInfo', async () => { await service.getTvInfo(1396); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/1396', expect.anything()); });
  it('should GET getTvInfoWithRequestId', async () => { await service.getTvInfoWithRequestId(5); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/request/5', expect.anything()); });
  it('should GET getTvInfoWithMovieDbId', async () => { await service.getTvInfoWithMovieDbId(999); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/Tv/moviedb/999', expect.anything()); });
  it('should GET getMovieCollections', async () => { await service.getMovieCollections(10); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/movie/collection/10', expect.anything()); });
  it('should GET getMoviesByActor', () => { service.getMoviesByActor(123); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/actor/123/movie', expect.anything()); });
  it('should GET getTvByActor', () => { service.getTvByActor(123); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/actor/123/tv', expect.anything()); });
  it('should GET getArtistInformation', () => { service.getArtistInformation('art-1'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/artist/art-1'); });
  it('should GET getReleaseGroupArt', () => { service.getReleaseGroupArt('mbid-1'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/releasegroupart/mbid-1'); });
  it('should GET getAlbum', () => { service.getAlbum('mbid-2'); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/artist/album/mbid-2'); });
  it('should GET getRottenMovieRatings', () => { service.getRottenMovieRatings('Inception', 2010); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/ratings/movie/Inception/2010'); });
  it('should GET getRottenTvRatings', () => { service.getRottenTvRatings('Breaking Bad', 2008); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/ratings/tv/Breaking Bad/2008'); });
  it('should GET getMovieStreams', () => { service.getMovieStreams(550); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/stream/movie/550'); });
  it('should GET getTvStreams', () => { service.getTvStreams(1396); expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/search/stream/tv/1396'); });
});
