import { Component, inject, signal, computed, OnInit, OnDestroy, ChangeDetectionStrategy } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { TranslateModule } from "@ngx-translate/core";

import { SearchV2Service } from "../../../services";
import { RequestType } from "../../../interfaces";

export interface IHeroBannerItem {
    id: number;
    title: string;
    overview: string;
    backdropPath: string;
    rating: number;
    type: RequestType;
    year: string;
}

@Component({
    standalone: true,
    selector: "discover-hero-banner",
    templateUrl: "./hero-banner.component.html",
    styleUrls: ["./hero-banner.component.scss"],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CommonModule,
        RouterModule,
        TranslateModule
    ]
})
export class HeroBannerComponent implements OnInit, OnDestroy {
    private readonly searchService = inject(SearchV2Service);

    public items = signal<IHeroBannerItem[]>([]);
    public activeIndex = signal<number>(0);
    public loaded = signal<boolean>(false);
    public paused = signal<boolean>(false);

    public activeItem = computed(() => {
        const list = this.items();
        if (list.length === 0) return null;
        return list[this.activeIndex()];
    });

    public backdropUrl = computed(() => {
        const item = this.activeItem();
        if (!item || !item.backdropPath) return '';
        return `https://image.tmdb.org/t/p/w1280${item.backdropPath}`;
    });

    private rotationInterval: any;

    async ngOnInit() {
        try {
            const movies = await this.searchService.nowPlayingMoviesByPage(0, 5);
            const bannerItems: IHeroBannerItem[] = movies
                .filter(m => m.backdropPath)
                .slice(0, 5)
                .map(m => ({
                    id: m.id,
                    title: m.title,
                    overview: m.overview,
                    backdropPath: m.backdropPath,
                    rating: m.voteAverage,
                    type: RequestType.movie,
                    year: m.releaseDate ? new Date(m.releaseDate).getFullYear().toString() : ''
                }));

            this.items.set(bannerItems);
            this.loaded.set(true);

            if (bannerItems.length > 1) {
                const prefersReducedMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches;
                if (!prefersReducedMotion) {
                    this.startRotation();
                } else {
                    this.paused.set(true);
                }
            }
        } catch (err) {
            console.warn('HeroBanner: failed to load trending movies', err);
        }
    }

    ngOnDestroy() {
        if (this.rotationInterval) {
            clearInterval(this.rotationInterval);
        }
    }

    public selectItem(index: number): void {
        const length = this.items().length;
        if (index < 0 || index >= length) {
            return;
        }
        this.activeIndex.set(index);
        // Reset the rotation timer if currently running
        if (this.rotationInterval) {
            this.stopRotation();
            this.startRotation();
        }
    }

    public togglePause(): void {
        if (this.paused()) {
            this.paused.set(false);
            this.startRotation();
        } else {
            this.paused.set(true);
            this.stopRotation();
        }
    }

    private startRotation(): void {
        this.stopRotation();
        this.rotationInterval = setInterval(() => {
            this.activeIndex.update(i => (i + 1) % this.items().length);
        }, 8000);
    }

    private stopRotation(): void {
        if (this.rotationInterval) {
            clearInterval(this.rotationInterval);
            this.rotationInterval = null;
        }
    }

    public getDetailsLink(): string {
        const item = this.activeItem();
        if (!item) return '/';
        return `/details/movie/${item.id}`;
    }

    public getRatingDisplay(): string {
        const item = this.activeItem();
        if (!item) return '';
        return (Math.round(item.rating * 10) / 10).toString();
    }
}
