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
    service = new PlexTvService(mockHttp as any);
  });

  it('should POST to plex.tv pins endpoint', () => {
    service.GetPin('client-id-123', 'Ombi');
    expect(mockHttp.post).toHaveBeenCalledWith(
      'https://plex.tv/api/v2/pins?strong=true',
      null,
      expect.objectContaining({
        headers: expect.anything(),
      })
    );
  });

  it('should include correct Plex headers', () => {
    service.GetPin('my-client', 'MyApp');
    const callArgs = mockHttp.post.mock.calls[0];
    const headers = callArgs[2].headers;
    expect(headers.get('X-Plex-Client-Identifier')).toBe('my-client');
    expect(headers.get('X-Plex-Product')).toBe('MyApp');
    expect(headers.get('X-Plex-Version')).toBe('3');
    expect(headers.get('X-Plex-Device')).toBe('Ombi (Web)');
    expect(headers.get('X-Plex-Platform')).toBe('Web');
    expect(headers.get('Accept')).toBe('application/json');
    expect(headers.get('X-Plex-Model')).toBe('Plex OAuth');
  });
});
