import { describe, it, expect } from 'vitest';
import { QualityPipe } from './QualityPipe';

describe('QualityPipe', () => {
  const pipe = new QualityPipe();

  it('should return 4K as-is', () => {
    expect(pipe.transform('4K')).toBe('4K');
    expect(pipe.transform('4k')).toBe('4k');
  });

  it('should return 8K as-is', () => {
    expect(pipe.transform('8K')).toBe('8K');
    expect(pipe.transform('8k')).toBe('8k');
  });

  it('should return values already ending in p as-is', () => {
    expect(pipe.transform('1080p')).toBe('1080p');
    expect(pipe.transform('720P')).toBe('720P');
    expect(pipe.transform('480p')).toBe('480p');
  });

  it('should append p to values without a p suffix', () => {
    expect(pipe.transform('1080')).toBe('1080p');
    expect(pipe.transform('720')).toBe('720p');
    expect(pipe.transform('480')).toBe('480p');
  });
});
