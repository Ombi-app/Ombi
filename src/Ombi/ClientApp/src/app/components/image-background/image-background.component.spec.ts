import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { ImageBackgroundComponent } from './image-background.component';
import { of } from 'rxjs';

describe('ImageBackgroundComponent', () => {
  let comp: ImageBackgroundComponent;
  let mockImageService: any;
  let mockSanitizer: any;

  beforeEach(() => {
    vi.useFakeTimers();

    mockImageService = {
      getRandomBackgroundWithInfo: vi.fn().mockReturnValue(of({ url: 'https://image.tmdb.org/bg.jpg', name: 'Test Movie' })),
    };

    mockSanitizer = {
      bypassSecurityTrustStyle: vi.fn((val: string) => `safe:${val}`),
    };

    comp = new ImageBackgroundComponent(mockImageService, mockSanitizer);
  });

  afterEach(() => {
    comp.ngOnDestroy();
    vi.useRealTimers();
  });

  it('should fetch background on init', () => {
    comp.ngOnInit();
    expect(mockImageService.getRandomBackgroundWithInfo).toHaveBeenCalledTimes(1);
  });

  it('should set background and name from service response', () => {
    comp.ngOnInit();
    expect(mockSanitizer.bypassSecurityTrustStyle).toHaveBeenCalledWith('url(https://image.tmdb.org/bg.jpg)');
    expect(comp.name).toBe('Test Movie');
  });

  it('should cycle background every 30 seconds', () => {
    comp.ngOnInit();
    expect(mockImageService.getRandomBackgroundWithInfo).toHaveBeenCalledTimes(1);

    vi.advanceTimersByTime(30000);
    expect(mockImageService.getRandomBackgroundWithInfo).toHaveBeenCalledTimes(2);

    vi.advanceTimersByTime(30000);
    expect(mockImageService.getRandomBackgroundWithInfo).toHaveBeenCalledTimes(3);
  });

  it('should clear interval on destroy', () => {
    comp.ngOnInit();
    comp.ngOnDestroy();

    vi.advanceTimersByTime(60000);
    // Should still be 1 (only the initial call)
    expect(mockImageService.getRandomBackgroundWithInfo).toHaveBeenCalledTimes(1);
  });
});
