import { describe, it, expect } from 'vitest';
import { HumanizePipe } from './HumanizePipe';

describe('HumanizePipe', () => {
  const pipe = new HumanizePipe();

  it('should split camelCase into separate words', () => {
    expect(pipe.transform('camelCase')).toBe('Camel Case');
  });

  it('should split PascalCase into separate words', () => {
    expect(pipe.transform('PascalCase')).toBe('Pascal Case');
  });

  it('should handle single word', () => {
    expect(pipe.transform('hello')).toBe('Hello');
  });

  it('should capitalize the first letter', () => {
    expect(pipe.transform('lowerStart')).toBe('Lower Start');
  });

  it('should handle multiple uppercase transitions', () => {
    expect(pipe.transform('thisIsALongString')).toBe('This Is A Long String');
  });

  it('should return non-string values as-is', () => {
    expect(pipe.transform(123 as any)).toBe(123);
  });
});
