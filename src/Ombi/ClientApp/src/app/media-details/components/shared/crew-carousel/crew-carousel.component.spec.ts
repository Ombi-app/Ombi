import { describe, it, expect } from 'vitest';
import { CrewCarouselComponent } from './crew-carousel.component';

describe('CrewCarouselComponent', () => {
  it('should initialize responsive options', () => {
    const comp = new CrewCarouselComponent();
    expect(comp.responsiveOptions).toHaveLength(3);
    expect(comp.responsiveOptions[0].breakpoint).toBe('1024px');
  });

  it('should accept crew input', () => {
    const comp = new CrewCarouselComponent();
    comp.crew = [{ name: 'Director 1', job: 'Director' }];
    expect(comp.crew).toHaveLength(1);
  });
});
