import { describe, it, expect } from 'vitest';
import { OmbiDatePipe } from './OmbiDatePipe';

describe('OmbiDatePipe', () => {
  const pipe = new OmbiDatePipe();

  it('should return empty string for falsy value', () => {
    expect(pipe.transform('', 'yyyy-MM-dd')).toBe('');
    expect(pipe.transform(null as any, 'yyyy-MM-dd')).toBe('');
    expect(pipe.transform(undefined as any, 'yyyy-MM-dd')).toBe('');
  });

  it('should format an ISO date string with yyyy-MM-dd', () => {
    // Use a mid-day UTC time so local timezone offsets won't shift the date
    const result = pipe.transform('2023-06-15T12:00:00Z', 'yyyy-MM-dd');
    // Accept either June 15 or adjacent date depending on local timezone
    expect(result).toMatch(/^2023-06-1[45]$/);
  });

  it('should format with a time format', () => {
    const result = pipe.transform('2023-06-15T14:30:00Z', 'HH:mm');
    expect(result).toMatch(/\d{2}:\d{2}/);
  });

  it('should format with a full date-time format', () => {
    const result = pipe.transform('2023-01-01T00:00:00Z', 'yyyy');
    expect(result).toBe('2023');
  });
});
