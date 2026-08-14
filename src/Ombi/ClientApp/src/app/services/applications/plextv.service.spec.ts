import { describe, it, expect, vi, beforeEach } from 'vitest';
import { PlexTvService } from './plextv.service';
import { of } from 'rxjs';

describe('PlexTvService', () => {
  let service: PlexTvService;
  let mockHttp: { post: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    mockHttp = {
      post: vi.fn().mockReturnValue(of({})),
    };
    service = new PlexTvService(mockHttp as any, '/');
  });

  it('should create the Plex PIN through Ombi instead of posting directly to plex.tv', () => {
    service.GetPin();
    expect(mockHttp.post).toHaveBeenCalledWith(
      '/api/v1/token/plexpin',
      {},
      expect.objectContaining({
        headers: expect.anything(),
      })
    );
  });

  it('should preserve Ombi base path when creating a PIN', () => {
    const subPathService = new PlexTvService(mockHttp as any, '/requests');
    subPathService.GetPin();
    expect(mockHttp.post).toHaveBeenCalledWith(
      '/requests/api/v1/token/plexpin',
      {},
      expect.anything()
    );
  });
});
