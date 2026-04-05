import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { Injector, runInInjectionContext } from '@angular/core';
import { HeroBannerComponent } from './hero-banner.component';
import { SearchV2Service } from '../../../services';
import { APP_BASE_HREF } from '@angular/common';
import { ISearchMovieResult, RequestType } from '../../../interfaces';

function makeMovie(overrides: Partial<ISearchMovieResult> = {}): ISearchMovieResult {
  return {
    id: 1,
    title: 'Trending Movie',
    overview: 'An exciting movie',
    posterPath: '/poster.jpg',
    backdropPath: '/backdrop.jpg',
    voteAverage: 8.2,
    imdbId: 'tt0001',
    available: false,
    approved: false,
    requested: false,
    adult: false,
    releaseDate: new Date('2024-06-15'),
    genreIds: [],
    originalTitle: 'Trending Movie',
    originalLanguage: 'en',
    popularity: 200,
    voteCount: 1000,
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

function createComponent(mockSearchService: any) {
  const injector = Injector.create({
    providers: [
      { provide: SearchV2Service, useValue: mockSearchService },
      { provide: APP_BASE_HREF, useValue: '/' },
    ],
  });

  let component: HeroBannerComponent;
  runInInjectionContext(injector, () => {
    component = new HeroBannerComponent();
  });
  return component!;
}

describe('HeroBannerComponent', () => {
  let component: HeroBannerComponent;
  let mockSearchService: any;

  beforeEach(() => {
    mockSearchService = {
      nowPlayingMoviesByPage: vi.fn().mockResolvedValue([]),
    };
    component = createComponent(mockSearchService);
  });

  afterEach(() => {
    component.ngOnDestroy();
  });

  describe('ngOnInit', () => {
    it('should load trending movies and set items', async () => {
      const movies = [
        makeMovie({ id: 1, title: 'Movie 1', backdropPath: '/bg1.jpg' }),
        makeMovie({ id: 2, title: 'Movie 2', backdropPath: '/bg2.jpg' }),
        makeMovie({ id: 3, title: 'Movie 3', backdropPath: '/bg3.jpg' }),
      ];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);

      await component.ngOnInit();

      expect(component.items().length).toBe(3);
      expect(component.loaded()).toBe(true);
      expect(component.activeIndex()).toBe(0);
    });

    it('should filter out movies without backdrop', async () => {
      const movies = [
        makeMovie({ id: 1, backdropPath: '/bg.jpg' }),
        makeMovie({ id: 2, backdropPath: '' }),
        makeMovie({ id: 3, backdropPath: null as any }),
      ];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);

      await component.ngOnInit();

      expect(component.items().length).toBe(1);
      expect(component.items()[0].id).toBe(1);
    });

    it('should limit to 5 items max', async () => {
      const movies = Array.from({ length: 8 }, (_, i) =>
        makeMovie({ id: i, title: `Movie ${i}`, backdropPath: `/bg${i}.jpg` })
      );
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);

      await component.ngOnInit();

      expect(component.items().length).toBe(5);
    });

    it('should extract year from release date', async () => {
      const movies = [makeMovie({ releaseDate: new Date('2024-06-15'), backdropPath: '/bg.jpg' })];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);

      await component.ngOnInit();

      expect(component.items()[0].year).toBe('2024');
    });

    it('should handle empty release date', async () => {
      const movies = [makeMovie({ releaseDate: null as any, backdropPath: '/bg.jpg' })];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);

      await component.ngOnInit();

      expect(component.items()[0].year).toBe('');
    });

    it('should handle API error gracefully', async () => {
      mockSearchService.nowPlayingMoviesByPage.mockRejectedValue(new Error('Network error'));

      await component.ngOnInit();

      expect(component.items().length).toBe(0);
      expect(component.loaded()).toBe(false);
    });

    it('should map rating from voteAverage', async () => {
      const movies = [makeMovie({ voteAverage: 7.8, backdropPath: '/bg.jpg' })];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);

      await component.ngOnInit();

      expect(component.items()[0].rating).toBe(7.8);
    });

    it('should set type to movie', async () => {
      const movies = [makeMovie({ backdropPath: '/bg.jpg' })];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);

      await component.ngOnInit();

      expect(component.items()[0].type).toBe(RequestType.movie);
    });
  });

  describe('activeItem (computed)', () => {
    it('should return null when items list is empty', () => {
      expect(component.activeItem()).toBeNull();
    });

    it('should return the item at activeIndex', async () => {
      const movies = [
        makeMovie({ id: 10, title: 'First', backdropPath: '/bg1.jpg' }),
        makeMovie({ id: 20, title: 'Second', backdropPath: '/bg2.jpg' }),
      ];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);
      await component.ngOnInit();

      expect(component.activeItem()!.title).toBe('First');

      component.selectItem(1);
      expect(component.activeItem()!.title).toBe('Second');
    });
  });

  describe('backdropUrl (computed)', () => {
    it('should return empty string when no active item', () => {
      expect(component.backdropUrl()).toBe('');
    });

    it('should return full TMDB URL for active item backdrop', async () => {
      const movies = [makeMovie({ backdropPath: '/my-backdrop.jpg' })];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);
      await component.ngOnInit();

      expect(component.backdropUrl()).toBe('https://image.tmdb.org/t/p/w1280/my-backdrop.jpg');
    });
  });

  describe('selectItem', () => {
    it('should update activeIndex', async () => {
      const movies = [
        makeMovie({ id: 1, backdropPath: '/bg1.jpg' }),
        makeMovie({ id: 2, backdropPath: '/bg2.jpg' }),
        makeMovie({ id: 3, backdropPath: '/bg3.jpg' }),
      ];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);
      await component.ngOnInit();

      component.selectItem(2);
      expect(component.activeIndex()).toBe(2);
    });

    it('should ignore out-of-range index', async () => {
      const movies = [
        makeMovie({ id: 1, backdropPath: '/bg1.jpg' }),
        makeMovie({ id: 2, backdropPath: '/bg2.jpg' }),
      ];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);
      await component.ngOnInit();

      component.selectItem(5);
      expect(component.activeIndex()).toBe(0);

      component.selectItem(-1);
      expect(component.activeIndex()).toBe(0);
    });
  });

  describe('getDetailsLink', () => {
    it('should return root when no active item', () => {
      expect(component.getDetailsLink()).toBe('/');
    });

    it('should return movie details link', async () => {
      const movies = [makeMovie({ id: 42, backdropPath: '/bg.jpg' })];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);
      await component.ngOnInit();

      expect(component.getDetailsLink()).toBe('/details/movie/42');
    });
  });

  describe('getRatingDisplay', () => {
    it('should return empty string when no active item', () => {
      expect(component.getRatingDisplay()).toBe('');
    });

    it('should round rating to one decimal', async () => {
      const movies = [makeMovie({ voteAverage: 7.85, backdropPath: '/bg.jpg' })];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);
      await component.ngOnInit();

      expect(component.getRatingDisplay()).toBe('7.9');
    });

    it('should show whole number when no decimal needed', async () => {
      const movies = [makeMovie({ voteAverage: 8.0, backdropPath: '/bg.jpg' })];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);
      await component.ngOnInit();

      expect(component.getRatingDisplay()).toBe('8');
    });
  });

  describe('ngOnDestroy', () => {
    it('should not throw when no rotation interval', () => {
      expect(() => component.ngOnDestroy()).not.toThrow();
    });

    it('should clear rotation interval after init with multiple items', async () => {
      const movies = [
        makeMovie({ id: 1, backdropPath: '/bg1.jpg' }),
        makeMovie({ id: 2, backdropPath: '/bg2.jpg' }),
      ];
      mockSearchService.nowPlayingMoviesByPage.mockResolvedValue(movies);
      await component.ngOnInit();

      // Should not throw
      expect(() => component.ngOnDestroy()).not.toThrow();
    });
  });
});
