import { describe, it, expect } from 'vitest';
import { ThousandShortPipe } from './ThousandShortPipe';

describe('ThousandShortPipe', () => {
  const pipe = new ThousandShortPipe();

  it('should return numbers below 1000 unchanged', () => {
    expect(pipe.transform(500)).toBe(500);
    expect(pipe.transform(0)).toBe(0);
    expect(pipe.transform(999)).toBe(999);
  });

  it('should shorten thousands with k suffix', () => {
    expect(pipe.transform(1000, 1)).toBe('1.0k');
    expect(pipe.transform(1500, 1)).toBe('1.5k');
    expect(pipe.transform(45000, 0)).toBe('45k');
  });

  it('should shorten millions with M suffix', () => {
    expect(pipe.transform(1000000, 1)).toBe('1.0M');
    expect(pipe.transform(2500000, 1)).toBe('2.5M');
  });

  it('should shorten billions with G suffix', () => {
    expect(pipe.transform(1000000000, 1)).toBe('1.0G');
  });

  it('should return null for NaN', () => {
    expect(pipe.transform(NaN)).toBeNull();
  });

  it('should respect decimal precision argument', () => {
    expect(pipe.transform(1234, 2)).toBe('1.23k');
    expect(pipe.transform(1234, 0)).toBe('1k');
  });
});
