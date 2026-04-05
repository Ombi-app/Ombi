import { describe, it, expect, beforeEach, vi } from 'vitest';
import { Injector, runInInjectionContext } from '@angular/core';
import { CarouselListComponent, DiscoverType } from './carousel-list.component';
import { SearchV2Service } from '../../../services';
import { StorageService } from '../../../shared/storage/storage-service';
import { FeaturesFacade } from '../../../state/features/features.facade';
import { APP_BASE_HREF } from '@angular/common';
import { RequestType, ISearchMovieResult, ISearchTvResult } from '../../../interfaces';
import { DiscoverOption } from '../../interfaces';

function makeMovie(overrides: Partial<ISearchMovieResult> = {}): ISearchMovieResult {
  return {
    id: 1,
    title: 'Test Movie',
    overview: 'A great movie',
    posterPath: '/movie-poster.jpg',
    backdropPath: '/movie-backdrop.jpg',
    voteAverage: 7.5,
    imdbId: 'tt1234567',
    available: false,
    approved: false,
    requested: false,
    adult: false,
    releaseDate: new Date('2024-01-01'),
    genreIds: [],
    originalTitle: 'Test Movie',
    originalLanguage: 'en',
    popularity: 100,
    voteCount: 500,
    video: false,
    alreadyInCp: false,
    trailer: '',
    homepage: '',
    requestId: 0,
    plexUrl: '',
    embyUrl: '',
    jellyfinUrl: '',
    quality: '',
    digitalReleaseDate: new Date(),
    subscribed: false,
    showSubscribe: false,
    requestProcessing: false,
    processed: false,
    background: null,
    ...overrides,
  } as ISearchMovieResult;
}

function makeTvShow(overrides: Partial<ISearchTvResult> = {}): ISearchTvResult {
  return {
    id: 2,
    title: 'Test TV Show',
    overview: 'A great show',
    posterPath: '/tv-poster.jpg',
    backdropPath: '/tv-backdrop.jpg',
    rating: '8.0',
    imdbId: 'tt9876543',
    available: false,
    approved: false,
    requested: false,
    fullyAvailable: false,
    partlyAvailable: false,
    aliases: [],
    banner: '',
    seriesId: 0,
    status: '',
    firstAired: '',
    network: '',
    networkId: '',
    runtime: '',
    genre: [],
    lastUpdated: 0,
    airsDayOfWeek: '',
    airsTime: '',
    siteRating: 0,
    trailer: '',
    homepage: '',
    seasonRequests: [],
    requestAll: false,
    firstSeason: false,
    latestSeason: false,
    theTvDbId: '',
    subscribed: false,
    showSubscribe: false,
    plexUrl: '',
    embyUrl: '',
    jellyfinUrl: '',
    quality: '',
    background: null,
    open: false,
    ...overrides,
  } as ISearchTvResult;
}

function createComponent(mockSearchService: any, mockStorageService?: any, mockFeaturesFacade?: any) {
  const storage = mockStorageService || { get: vi.fn().mockReturnValue(null), save: vi.fn() };
  const features = mockFeaturesFacade || { is4kEnabled: vi.fn().mockReturnValue(false) };

  const injector = Injector.create({
    providers: [
      { provide: SearchV2Service, useValue: mockSearchService },
      { provide: StorageService, useValue: storage },
      { provide: FeaturesFacade, useValue: features },
      { provide: APP_BASE_HREF, useValue: '/' },
    ],
  });

  let component: CarouselListComponent;
  runInInjectionContext(injector, () => {
    component = new CarouselListComponent();
  });
  return { component: component!, mockStorageService: storage, mockFeaturesFacade: features };
}

function setupInputs(component: CarouselListComponent, discoverType: DiscoverType, discoverOption: DiscoverOption) {
  // Override the input signals for testing
  (component as any).discoverType = (() => discoverType) as any;
  (component as any).id = (() => 'test') as any;
  component.discoverOptions.set(discoverOption);
}

describe('CarouselListComponent', () => {
  let mockSearchService: any;

  beforeEach(() => {
    mockSearchService = {
      popularMoviesByPage: vi.fn().mockResolvedValue([]),
      nowPlayingMoviesByPage: vi.fn().mockResolvedValue([]),
      upcomingMoviesByPage: vi.fn().mockResolvedValue([]),
      recentlyRequestedMoviesByPage: vi.fn().mockResolvedValue([]),
      seasonalMoviesByPage: vi.fn().mockResolvedValue([]),
      popularTvByPage: vi.fn().mockResolvedValue([]),
      trendingTvByPage: vi.fn().mockResolvedValue([]),
      anticipatedTvByPage: vi.fn().mockResolvedValue([]),
    };
  });

  describe('TV show poster mapping', () => {
    it('should use posterPath for TV card poster when available', async () => {
      const tvShow = makeTvShow({
        posterPath: '/correct-poster.jpg',
        backdropPath: '/landscape-backdrop.jpg',
      });
      mockSearchService.popularTvByPage.mockResolvedValue([tvShow]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Tv);
      await component.ngOnInit();

      const results = component.discoverResults();
      expect(results.length).toBeGreaterThan(0);
      expect(results[0].posterPath).toBe('https://image.tmdb.org/t/p/w500//correct-poster.jpg');
    });

    it('should fall back to backdropPath when posterPath is empty', async () => {
      const tvShow = makeTvShow({
        posterPath: '',
        backdropPath: '/fallback-backdrop.jpg',
      });
      mockSearchService.popularTvByPage.mockResolvedValue([tvShow]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Tv);
      await component.ngOnInit();

      const results = component.discoverResults();
      expect(results[0].posterPath).toBe('https://image.tmdb.org/t/p/w500//fallback-backdrop.jpg');
    });

    it('should use default poster when both posterPath and backdropPath are empty', async () => {
      const tvShow = makeTvShow({
        posterPath: '',
        backdropPath: '',
      });
      mockSearchService.popularTvByPage.mockResolvedValue([tvShow]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Tv);
      await component.ngOnInit();

      const results = component.discoverResults();
      expect(results[0].posterPath).toContain('default_tv_poster.png');
    });

    it('should use backdropPath for the background field', async () => {
      const tvShow = makeTvShow({
        posterPath: '/poster.jpg',
        backdropPath: '/backdrop-for-bg.jpg',
      });
      mockSearchService.popularTvByPage.mockResolvedValue([tvShow]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Tv);
      await component.ngOnInit();

      const results = component.discoverResults();
      expect(results[0].background).toBe('/backdrop-for-bg.jpg');
    });

    it('should set TV card type to tvShow', async () => {
      const tvShow = makeTvShow();
      mockSearchService.popularTvByPage.mockResolvedValue([tvShow]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Tv);
      await component.ngOnInit();

      expect(component.discoverResults()[0].type).toBe(RequestType.tvShow);
    });

    it('should set available when fullyAvailable', async () => {
      const tvShow = makeTvShow({ fullyAvailable: true });
      mockSearchService.popularTvByPage.mockResolvedValue([tvShow]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Tv);
      await component.ngOnInit();

      expect(component.discoverResults()[0].available).toBe(true);
    });

    it('should set available when partlyAvailable', async () => {
      const tvShow = makeTvShow({ partlyAvailable: true });
      mockSearchService.popularTvByPage.mockResolvedValue([tvShow]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Tv);
      await component.ngOnInit();

      expect(component.discoverResults()[0].available).toBe(true);
    });

    it('should set approved when partlyAvailable', async () => {
      const tvShow = makeTvShow({ partlyAvailable: true, approved: false });
      mockSearchService.popularTvByPage.mockResolvedValue([tvShow]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Tv);
      await component.ngOnInit();

      expect(component.discoverResults()[0].approved).toBe(true);
    });
  });

  describe('Movie poster mapping', () => {
    it('should use posterPath for movie card poster', async () => {
      const movie = makeMovie({ posterPath: '/movie-poster.jpg' });
      mockSearchService.popularMoviesByPage.mockResolvedValue([movie]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Movie);
      await component.ngOnInit();

      expect(component.discoverResults()[0].posterPath).toBe('https://image.tmdb.org/t/p/w500//movie-poster.jpg');
    });

    it('should use default poster when movie posterPath is missing', async () => {
      const movie = makeMovie({ posterPath: '' });
      mockSearchService.popularMoviesByPage.mockResolvedValue([movie]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Movie);
      await component.ngOnInit();

      expect(component.discoverResults()[0].posterPath).toContain('default_movie_poster.png');
    });

    it('should map movie title and overview', async () => {
      const movie = makeMovie({ title: 'Inception', overview: 'Dreams within dreams' });
      mockSearchService.popularMoviesByPage.mockResolvedValue([movie]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Movie);
      await component.ngOnInit();

      const result = component.discoverResults()[0];
      expect(result.title).toBe('Inception');
      expect(result.overview).toBe('Dreams within dreams');
    });

    it('should use backdropPath for movie background', async () => {
      const movie = makeMovie({ backdropPath: '/movie-bg.jpg' });
      mockSearchService.popularMoviesByPage.mockResolvedValue([movie]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Movie);
      await component.ngOnInit();

      expect(component.discoverResults()[0].background).toBe('/movie-bg.jpg');
    });
  });

  describe('Combined mode', () => {
    it('should include both movies and TV shows', async () => {
      const movie = makeMovie({ id: 100, title: 'Movie A' });
      const tvShow = makeTvShow({ id: 200, title: 'TV Show B' });
      mockSearchService.popularMoviesByPage.mockResolvedValue([movie]);
      mockSearchService.popularTvByPage.mockResolvedValue([tvShow]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Combined);
      await component.ngOnInit();

      const results = component.discoverResults();
      // ngOnInit loads a second batch when < 20 results, so we get 2 batches
      const movieResults = results.filter(r => r.type === RequestType.movie);
      const tvResults = results.filter(r => r.type === RequestType.tvShow);
      expect(movieResults.length).toBeGreaterThan(0);
      expect(tvResults.length).toBeGreaterThan(0);
      expect(movieResults[0].title).toBe('Movie A');
      expect(tvResults[0].title).toBe('TV Show B');
    });
  });

  describe('DiscoverType routing', () => {
    it('should call nowPlaying for Trending type', async () => {
      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Trending, DiscoverOption.Movie);
      await component.ngOnInit();

      expect(mockSearchService.nowPlayingMoviesByPage).toHaveBeenCalled();
    });

    it('should call upcoming for Upcoming type', async () => {
      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Upcoming, DiscoverOption.Movie);
      await component.ngOnInit();

      expect(mockSearchService.upcomingMoviesByPage).toHaveBeenCalled();
    });

    it('should call seasonal for Seasonal type', async () => {
      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Seasonal, DiscoverOption.Movie);
      await component.ngOnInit();

      expect(mockSearchService.seasonalMoviesByPage).toHaveBeenCalled();
    });

    it('should call anticipated TV for Upcoming TV', async () => {
      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Upcoming, DiscoverOption.Tv);
      await component.ngOnInit();

      expect(mockSearchService.anticipatedTvByPage).toHaveBeenCalled();
    });

    it('should call popular TV for Popular TV', async () => {
      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Tv);
      await component.ngOnInit();

      expect(mockSearchService.popularTvByPage).toHaveBeenCalled();
    });

    it('should call trending TV for Trending TV', async () => {
      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Trending, DiscoverOption.Tv);
      await component.ngOnInit();

      expect(mockSearchService.trendingTvByPage).toHaveBeenCalled();
    });
  });

  describe('computed properties', () => {
    it('should report hasResults as false when empty', () => {
      const { component } = createComponent(mockSearchService);
      expect(component.hasResults()).toBe(false);
    });

    it('should report hasResults as true when items loaded', async () => {
      const movie = makeMovie();
      mockSearchService.popularMoviesByPage.mockResolvedValue([movie]);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Movie);
      await component.ngOnInit();

      expect(component.hasResults()).toBe(true);
    });

    it('should track totalResults', async () => {
      const movies = [makeMovie({ id: 1 }), makeMovie({ id: 2 }), makeMovie({ id: 3 })];
      mockSearchService.popularMoviesByPage.mockResolvedValue(movies);

      const { component } = createComponent(mockSearchService);
      setupInputs(component, DiscoverType.Popular, DiscoverOption.Movie);
      await component.ngOnInit();

      // ngOnInit loads a second batch when < 20 results, so results are doubled
      expect(component.totalResults()).toBe(6);
    });
  });
});
