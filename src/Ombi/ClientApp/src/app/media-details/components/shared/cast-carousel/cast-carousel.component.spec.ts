import { describe, it, expect } from 'vitest';
import { CastCarouselComponent } from './cast-carousel.component';

describe('CastCarouselComponent', () => {
  it('should initialize responsive options', () => {
    const comp = new CastCarouselComponent();
    expect(comp.responsiveOptions).toHaveLength(4);
    expect(comp.responsiveOptions[0].breakpoint).toBe('1200px');
    expect(comp.responsiveOptions[0].numVisible).toBe(5);
  });

  it('should accept cast input', () => {
    const comp = new CastCarouselComponent();
    comp.cast = [{ name: 'Actor 1' }, { name: 'Actor 2' }];
    expect(comp.cast).toHaveLength(2);
  });
});
