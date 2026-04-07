import { describe, it, expect, vi } from 'vitest';
import { SafePipe } from './SafePipe';

describe('SafePipe', () => {
  function createPipe() {
    const mockSanitizer = {
      bypassSecurityTrustResourceUrl: vi.fn((url: string) => `safe:${url}`),
    } as any;
    return { pipe: new SafePipe(mockSanitizer), mockSanitizer };
  }

  it('should call bypassSecurityTrustResourceUrl with the given URL', () => {
    const { pipe, mockSanitizer } = createPipe();
    pipe.transform('https://example.com/video');
    expect(mockSanitizer.bypassSecurityTrustResourceUrl).toHaveBeenCalledWith('https://example.com/video');
  });

  it('should return the sanitized URL', () => {
    const { pipe } = createPipe();
    const result = pipe.transform('https://youtube.com/embed/abc');
    expect(result).toBe('safe:https://youtube.com/embed/abc');
  });

  it('should handle empty string', () => {
    const { pipe, mockSanitizer } = createPipe();
    pipe.transform('');
    expect(mockSanitizer.bypassSecurityTrustResourceUrl).toHaveBeenCalledWith('');
  });

  it('should handle undefined input', () => {
    const { pipe, mockSanitizer } = createPipe();
    pipe.transform(undefined);
    expect(mockSanitizer.bypassSecurityTrustResourceUrl).toHaveBeenCalledWith(undefined);
  });
});
