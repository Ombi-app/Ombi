/**
 * Unit tests for CarouselListComponent.
 *
 * NOTE: This project does not currently have an Angular unit-test runner configured.
 * To run these tests, add the following dev-dependencies and configure a test builder:
 *   npm install --save-dev @angular/platform-browser-dynamic karma karma-chrome-launcher
 *                          karma-jasmine jasmine-core @types/jasmine karma-coverage
 * Then add a "test" target to angular.json pointing at your karma.config.js.
 */

import { TestBed, fakeAsync, tick, flush } from '@angular/core/testing';
import { Component, signal } from '@angular/core';
import { APP_BASE_HREF } from '@angular/common';

import { CarouselListComponent, DiscoverType } from './carousel-list.component';
import { SearchV2Service } from '../../../services';
import { StorageService } from '../../../shared/storage/storage-service';
import { FeaturesFacade } from '../../../state/features/features.facade';
import { DiscoverOption } from '../../interfaces';

// ---------------------------------------------------------------------------
// Minimal stubs – only the methods exercised by changed code are defined.
// ---------------------------------------------------------------------------

function makeSearchServiceSpy(): jasmine.SpyObj<SearchV2Service> {
    return jasmine.createSpyObj<SearchV2Service>('SearchV2Service', [
        'popularMoviesByPage',
        'nowPlayingMoviesByPage',
        'upcomingMoviesByPage',
        'recentlyRequestedMoviesByPage',
        'seasonalMoviesByPage',
        'popularTvByPage',
        'trendingTvByPage',
        'anticipatedTvByPage',
    ]);
}

function makeStorageServiceStub(): Partial<StorageService> {
    return {
        get: (_key: string) => null,
        save: (_key: string, _value: string) => {},
    };
}

function makeFeaturesFacadeStub(): Partial<FeaturesFacade> {
    return {
        is4kEnabled: () => false,
    };
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/** Returns a promise that resolves with an empty array after a microtask. */
function emptyMovieResult(): Promise<any[]> {
    return Promise.resolve([]);
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('CarouselListComponent – offset / parallelism fixes', () => {
    let component: CarouselListComponent;
    let searchSpy: jasmine.SpyObj<SearchV2Service>;

    /** Configure TestBed with the standalone component and provider stubs. */
    beforeEach(async () => {
        searchSpy = makeSearchServiceSpy();

        // Default spy returns: every service method resolves with []
        const movieMethods: Array<keyof SearchV2Service> = [
            'popularMoviesByPage',
            'nowPlayingMoviesByPage',
            'upcomingMoviesByPage',
            'recentlyRequestedMoviesByPage',
            'seasonalMoviesByPage',
        ];
        const tvMethods: Array<keyof SearchV2Service> = [
            'popularTvByPage',
            'trendingTvByPage',
            'anticipatedTvByPage',
        ];
        [...movieMethods, ...tvMethods].forEach(m =>
            (searchSpy[m] as jasmine.Spy).and.returnValue(emptyMovieResult())
        );

        await TestBed.configureTestingModule({
            imports: [CarouselListComponent],
            providers: [
                { provide: SearchV2Service, useValue: searchSpy },
                { provide: StorageService, useValue: makeStorageServiceStub() },
                { provide: FeaturesFacade, useValue: makeFeaturesFacadeStub() },
                { provide: APP_BASE_HREF, useValue: '/' },
            ],
        })
            .overrideComponent(CarouselListComponent, {
                // Stub out child components / directives that require complex modules
                set: { imports: [], template: '' },
            })
            .compileComponents();

        const fixture = TestBed.createComponent(CarouselListComponent);
        component = fixture.componentInstance;

        // Set required signal inputs that have no defaults
        (component as any).discoverType = signal(DiscoverType.Popular);
        (component as any).id = signal('test-carousel');
    });

    // -----------------------------------------------------------------------
    // loadMovies – accepts and uses the explicit offset parameter
    // -----------------------------------------------------------------------

    describe('loadMovies(offset)', () => {
        it('uses the provided offset instead of currentlyLoaded when called with Popular type', async () => {
            // Set internal currentlyLoaded to a value that differs from the offset we pass
            (component as any).currentlyLoaded = 40;
            const amountToLoad = (component as any).amountToLoad as number;

            // Explicitly call private loadMovies with a specific offset
            await (component as any).loadMovies(0);

            expect(searchSpy.popularMoviesByPage).toHaveBeenCalledWith(0, amountToLoad);
            // Must NOT have been called with the stale currentlyLoaded value (40)
            expect(searchSpy.popularMoviesByPage).not.toHaveBeenCalledWith(40, amountToLoad);
        });

        it('falls back to currentlyLoaded when no offset is provided', async () => {
            (component as any).currentlyLoaded = 20;
            const amountToLoad = (component as any).amountToLoad as number;

            await (component as any).loadMovies(); // no argument

            expect(searchSpy.popularMoviesByPage).toHaveBeenCalledWith(20, amountToLoad);
        });

        it('routes to nowPlayingMoviesByPage for Trending type', async () => {
            (component as any).discoverType = signal(DiscoverType.Trending);
            const amountToLoad = (component as any).amountToLoad as number;

            await (component as any).loadMovies(10);

            expect(searchSpy.nowPlayingMoviesByPage).toHaveBeenCalledWith(10, amountToLoad);
        });

        it('routes to upcomingMoviesByPage for Upcoming type', async () => {
            (component as any).discoverType = signal(DiscoverType.Upcoming);
            const amountToLoad = (component as any).amountToLoad as number;

            await (component as any).loadMovies(5);

            expect(searchSpy.upcomingMoviesByPage).toHaveBeenCalledWith(5, amountToLoad);
        });

        it('routes to recentlyRequestedMoviesByPage for RecentlyRequested type', async () => {
            (component as any).discoverType = signal(DiscoverType.RecentlyRequested);
            const amountToLoad = (component as any).amountToLoad as number;

            await (component as any).loadMovies(0);

            expect(searchSpy.recentlyRequestedMoviesByPage).toHaveBeenCalledWith(0, amountToLoad);
        });
    });

    // -----------------------------------------------------------------------
    // loadTv – accepts and uses the explicit offset parameter
    // -----------------------------------------------------------------------

    describe('loadTv(offset)', () => {
        it('uses the provided offset instead of currentlyLoaded when called with Popular type', async () => {
            (component as any).currentlyLoaded = 40;
            const amountToLoad = (component as any).amountToLoad as number;

            await (component as any).loadTv(0);

            expect(searchSpy.popularTvByPage).toHaveBeenCalledWith(0, amountToLoad);
            expect(searchSpy.popularTvByPage).not.toHaveBeenCalledWith(40, amountToLoad);
        });

        it('falls back to currentlyLoaded when no offset is provided', async () => {
            (component as any).currentlyLoaded = 20;
            const amountToLoad = (component as any).amountToLoad as number;

            await (component as any).loadTv(); // no argument

            expect(searchSpy.popularTvByPage).toHaveBeenCalledWith(20, amountToLoad);
        });

        it('routes to trendingTvByPage for Trending type', async () => {
            (component as any).discoverType = signal(DiscoverType.Trending);
            const amountToLoad = (component as any).amountToLoad as number;

            await (component as any).loadTv(10);

            expect(searchSpy.trendingTvByPage).toHaveBeenCalledWith(10, amountToLoad);
        });

        it('routes to anticipatedTvByPage for Upcoming type', async () => {
            (component as any).discoverType = signal(DiscoverType.Upcoming);
            const amountToLoad = (component as any).amountToLoad as number;

            await (component as any).loadTv(5);

            expect(searchSpy.anticipatedTvByPage).toHaveBeenCalledWith(5, amountToLoad);
        });
    });

    // -----------------------------------------------------------------------
    // loadData – Combined mode: both loaders receive the SAME captured offset
    // -----------------------------------------------------------------------

    describe('loadData() – offset capture', () => {
        it('passes the same captured offset to both loadMovies and loadTv in Combined mode', async () => {
            (component as any).currentlyLoaded = 0;
            component.discoverOptions.set(DiscoverOption.Combined);
            const amountToLoad = (component as any).amountToLoad as number;

            await (component as any).loadData(false);

            // Both should have been called with the same offset (0), NOT tv with offset 20
            expect(searchSpy.popularMoviesByPage).toHaveBeenCalledWith(0, amountToLoad);
            expect(searchSpy.popularTvByPage).toHaveBeenCalledWith(0, amountToLoad);
        });

        it('increments currentlyLoaded by amountToLoad exactly once after both promises resolve', async () => {
            (component as any).currentlyLoaded = 0;
            component.discoverOptions.set(DiscoverOption.Combined);
            const amountToLoad = (component as any).amountToLoad as number;

            await (component as any).loadData(false);

            // Should be 0 + amountToLoad, NOT 0 + 2*amountToLoad (old buggy behavior)
            expect((component as any).currentlyLoaded).toBe(amountToLoad);
        });

        it('increments currentlyLoaded only once even in Movie-only mode', async () => {
            (component as any).currentlyLoaded = 0;
            component.discoverOptions.set(DiscoverOption.Movie);
            const amountToLoad = (component as any).amountToLoad as number;

            await (component as any).loadData(false);

            expect((component as any).currentlyLoaded).toBe(amountToLoad);
        });

        it('increments currentlyLoaded only once in Tv-only mode', async () => {
            (component as any).currentlyLoaded = 0;
            component.discoverOptions.set(DiscoverOption.Tv);
            const amountToLoad = (component as any).amountToLoad as number;

            await (component as any).loadData(false);

            expect((component as any).currentlyLoaded).toBe(amountToLoad);
        });

        it('does not call loadTv in Movie-only mode', async () => {
            component.discoverOptions.set(DiscoverOption.Movie);

            await (component as any).loadData(false);

            expect(searchSpy.popularTvByPage).not.toHaveBeenCalled();
            expect(searchSpy.trendingTvByPage).not.toHaveBeenCalled();
            expect(searchSpy.anticipatedTvByPage).not.toHaveBeenCalled();
        });

        it('does not call loadMovies in Tv-only mode', async () => {
            component.discoverOptions.set(DiscoverOption.Tv);

            await (component as any).loadData(false);

            expect(searchSpy.popularMoviesByPage).not.toHaveBeenCalled();
        });

        /**
         * Regression test for the original race-condition bug:
         * In the old code, loadMovies/loadTv each mutated currentlyLoaded,
         * so the second concurrent call would use a stale (incremented) offset.
         * With the fix, the offset is captured once before both calls.
         */
        it('regression: second loadData call uses offset = amountToLoad, not 2 * amountToLoad', async () => {
            (component as any).currentlyLoaded = 0;
            component.discoverOptions.set(DiscoverOption.Combined);
            const amountToLoad = (component as any).amountToLoad as number;

            // First load
            await (component as any).loadData(false);
            // currentlyLoaded should now be amountToLoad (e.g. 20)

            // Reset call counts
            (searchSpy.popularMoviesByPage as jasmine.Spy).calls.reset();
            (searchSpy.popularTvByPage as jasmine.Spy).calls.reset();

            // Second load - should use offset = amountToLoad (e.g. 20)
            await (component as any).loadData(false);

            expect(searchSpy.popularMoviesByPage).toHaveBeenCalledWith(amountToLoad, amountToLoad);
            expect(searchSpy.popularTvByPage).toHaveBeenCalledWith(amountToLoad, amountToLoad);
            // currentlyLoaded should be 2 * amountToLoad
            expect((component as any).currentlyLoaded).toBe(2 * amountToLoad);
        });
    });

    // -----------------------------------------------------------------------
    // newPage – captures offset before launching parallel loads
    // -----------------------------------------------------------------------

    describe('newPage() – offset capture', () => {
        beforeEach(() => {
            // Simulate being on the last page of the carousel so the load branch is entered
            (component as any).carousel = {
                page: 10,
                totalDots: () => 2, // page(10) >= totalDots(2) - 2 → end = true
            };
        });

        it('passes the same captured offset to both loadMovies and loadTv in Combined mode', async () => {
            (component as any).currentlyLoaded = 20;
            component.discoverOptions.set(DiscoverOption.Combined);
            const amountToLoad = (component as any).amountToLoad as number;

            await component.newPage();

            expect(searchSpy.popularMoviesByPage).toHaveBeenCalledWith(20, amountToLoad);
            expect(searchSpy.popularTvByPage).toHaveBeenCalledWith(20, amountToLoad);
        });

        it('increments currentlyLoaded by amountToLoad exactly once after both promises resolve', async () => {
            (component as any).currentlyLoaded = 20;
            component.discoverOptions.set(DiscoverOption.Combined);
            const amountToLoad = (component as any).amountToLoad as number;

            await component.newPage();

            expect((component as any).currentlyLoaded).toBe(20 + amountToLoad);
        });

        it('does not increment currentlyLoaded when not at the end of the carousel', async () => {
            // Override carousel to simulate NOT being at end
            (component as any).carousel = {
                page: 0,
                totalDots: () => 10, // page(0) < totalDots(10) - 2 → end = false
            };
            (component as any).currentlyLoaded = 0;

            await component.newPage();

            expect((component as any).currentlyLoaded).toBe(0);
        });
    });
});