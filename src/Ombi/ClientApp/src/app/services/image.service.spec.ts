import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ImageService } from './image.service';
import { of } from 'rxjs';

function createMockImageService(baseHref = '/') {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of({})),
  };

  const service = Object.create(ImageService.prototype);
  service.http = mockHttp;
  service.url = baseHref.length > 1 ? baseHref + '/api/v1/Images/' : '/api/v1/Images/';
  service.headers = { set: vi.fn() };

  return { service: service as ImageService, mockHttp };
}

describe('ImageService', () => {
  let service: ImageService;
  let mockHttp: { get: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    const mocks = createMockImageService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should call correct URL for getRandomBackground', () => {
    service.getRandomBackground();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Images/background/', expect.anything());
  });

  it('should call correct URL for getRandomBackgroundWithInfo', () => {
    service.getRandomBackgroundWithInfo();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Images/background/info', expect.anything());
  });

  it('should call correct URL for getTvBanner', () => {
    service.getTvBanner(12345);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Images/tv/12345', expect.anything());
  });

  it('should call correct URL for getMoviePoster', () => {
    service.getMoviePoster('550');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Images/poster/movie/550', expect.anything());
  });

  it('should call correct URL for getTvPoster', () => {
    service.getTvPoster(67890);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Images/poster/tv/67890', expect.anything());
  });

  it('should call correct URL for getMovieBackground', () => {
    service.getMovieBackground('550');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Images/background/movie/550', expect.anything());
  });

  it('should call correct URL for getTvBackground', () => {
    service.getTvBackground(12345);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Images/background/tv/12345', expect.anything());
  });

  it('should call correct URL for getTmdbTvPoster', () => {
    service.getTmdbTvPoster(99999);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Images/poster/tv/tmdb/99999', expect.anything());
  });

  it('should call correct URL for getTmdbTvBackground', () => {
    service.getTmdbTvBackground(88888);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Images/background/tv/tmdb/88888', expect.anything());
  });

  it('should call correct URL for getMovieBanner', () => {
    service.getMovieBanner('550');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Images/banner/movie/550', expect.anything());
  });

  describe('with non-root base href', () => {
    let nonRootService: ImageService;
    let nonRootHttp: { get: ReturnType<typeof vi.fn> };

    beforeEach(() => {
      const mocks = createMockImageService('/ombi');
      nonRootService = mocks.service;
      nonRootHttp = mocks.mockHttp;
    });

    it('should prepend base href to URL', () => {
      nonRootService.getRandomBackground();
      expect(nonRootHttp.get).toHaveBeenCalledWith('/ombi/api/v1/Images/background/', expect.anything());
    });
  });
});
