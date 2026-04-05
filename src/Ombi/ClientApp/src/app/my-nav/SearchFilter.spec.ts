import { describe, it, expect } from 'vitest';
import { SearchFilter } from './SearchFilter';

describe('SearchFilter', () => {
  it('should create with default undefined properties', () => {
    const filter = new SearchFilter();
    expect(filter.movies).toBeUndefined();
    expect(filter.tvShows).toBeUndefined();
    expect(filter.music).toBeUndefined();
    expect(filter.people).toBeUndefined();
  });

  it('should create with partial initialization', () => {
    const filter = new SearchFilter({ movies: true, tvShows: false });
    expect(filter.movies).toBe(true);
    expect(filter.tvShows).toBe(false);
    expect(filter.music).toBeUndefined();
    expect(filter.people).toBeUndefined();
  });

  it('should create with full initialization', () => {
    const filter = new SearchFilter({ movies: true, tvShows: true, music: true, people: true });
    expect(filter.movies).toBe(true);
    expect(filter.tvShows).toBe(true);
    expect(filter.music).toBe(true);
    expect(filter.people).toBe(true);
  });

  it('should allow all false values', () => {
    const filter = new SearchFilter({ movies: false, tvShows: false, music: false, people: false });
    expect(filter.movies).toBe(false);
    expect(filter.tvShows).toBe(false);
    expect(filter.music).toBe(false);
    expect(filter.people).toBe(false);
  });
});
