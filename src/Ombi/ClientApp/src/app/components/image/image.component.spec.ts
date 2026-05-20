import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ImageComponent } from './image.component';
import { RequestType } from '../../interfaces/IRequestModel';

describe('ImageComponent', () => {
  function createComponent(baseHref = '/') {
    return new ImageComponent(baseHref);
  }

  describe('constructor', () => {
    it('should set empty baseUrl when href is "/"', () => {
      const comp = createComponent('/');
      expect(comp.getPlaceholderImage).toBeDefined();
    });

    it('should prepend baseUrl for non-root href', () => {
      const comp = createComponent('/ombi');
      comp.type = RequestType.movie;
      expect(comp.getPlaceholderImage()).toBe('/ombi/images/default_movie_poster.png');
    });
  });

  describe('getPlaceholderImage', () => {
    it('should return movie placeholder for movie type', () => {
      const comp = createComponent();
      comp.type = RequestType.movie;
      expect(comp.getPlaceholderImage()).toBe('/images/default_movie_poster.png');
    });

    it('should return tv placeholder for tvShow type', () => {
      const comp = createComponent();
      comp.type = RequestType.tvShow;
      expect(comp.getPlaceholderImage()).toBe('/images/default_tv_poster.png');
    });

    it('should return music placeholder for album type', () => {
      const comp = createComponent();
      comp.type = RequestType.album;
      expect(comp.getPlaceholderImage()).toBe('/images/default-music-placeholder.png');
    });
  });

  describe('getImageSrc', () => {
    it('should return src when set', () => {
      const comp = createComponent();
      comp.src = 'https://image.tmdb.org/poster.jpg';
      expect(comp.getImageSrc()).toBe('https://image.tmdb.org/poster.jpg');
    });

    it('should return placeholder when src is not set', () => {
      const comp = createComponent();
      comp.type = RequestType.movie;
      expect(comp.getImageSrc()).toBe('/images/default_movie_poster.png');
    });
  });

  describe('ngOnInit', () => {
    it('should set src to placeholder when src is not set', () => {
      const comp = createComponent();
      comp.type = RequestType.tvShow;
      // Suppress console.warn during test
      vi.spyOn(console, 'warn').mockImplementation(() => {});
      comp.ngOnInit();
      expect(comp.src).toBe('/images/default_tv_poster.png');
    });

    it('should not change src when it is already set', () => {
      const comp = createComponent();
      comp.src = 'https://image.tmdb.org/poster.jpg';
      comp.ngOnInit();
      expect(comp.src).toBe('https://image.tmdb.org/poster.jpg');
    });
  });

  describe('onError', () => {
    it('should set event target src to placeholder', () => {
      const comp = createComponent();
      comp.type = RequestType.movie;
      comp.src = 'https://image.tmdb.org/broken.jpg';
      const event = { target: { src: '' } };
      comp.onError(event);
      expect(event.target.src).toBe('/images/default_movie_poster.png');
    });

    it('should not retry if src is not set', () => {
      const comp = createComponent();
      comp.type = RequestType.movie;
      const event = { target: { src: '' } };
      vi.spyOn(console, 'warn').mockImplementation(() => {});
      comp.ngOnInit(); // sets src to placeholder
      comp.onError(event);
      // Should just set placeholder, no retry timer
      expect(event.target.src).toBe('/images/default_movie_poster.png');
    });
  });

  describe('src setter', () => {
    it('should log a warning when receiving null/undefined', () => {
      const comp = createComponent();
      const warnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});
      comp.src = null as any;
      expect(warnSpy).toHaveBeenCalled();
    });
  });
});
