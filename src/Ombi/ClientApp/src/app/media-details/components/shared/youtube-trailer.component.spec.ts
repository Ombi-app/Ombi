import { describe, it, expect } from 'vitest';
import { YoutubeTrailerComponent } from './youtube-trailer.component';

describe('YoutubeTrailerComponent', () => {
  it('should store the youtube link from dialog data', () => {
    const mockDialogRef = {} as any;
    const comp = new YoutubeTrailerComponent(mockDialogRef, 'https://youtube.com/watch?v=abc');
    expect(comp.youtubeLink).toBe('https://youtube.com/watch?v=abc');
  });
});
