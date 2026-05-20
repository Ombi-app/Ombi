import { describe, it, expect } from 'vitest';
import { DiscoverOption, DisplayOption } from './interfaces';

describe('Discover Interfaces', () => {
  describe('DiscoverOption', () => {
    it('should have Combined = 1', () => {
      expect(DiscoverOption.Combined).toBe(1);
    });

    it('should have Movie = 2', () => {
      expect(DiscoverOption.Movie).toBe(2);
    });

    it('should have Tv = 3', () => {
      expect(DiscoverOption.Tv).toBe(3);
    });
  });

  describe('DisplayOption', () => {
    it('should have Card = 1', () => {
      expect(DisplayOption.Card).toBe(1);
    });

    it('should have List = 2', () => {
      expect(DisplayOption.List).toBe(2);
    });
  });
});
