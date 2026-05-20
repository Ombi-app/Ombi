import { describe, it, expect, vi, beforeEach } from 'vitest';
import { VoteService } from './vote.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
    post: vi.fn().mockReturnValue(of({})),
  };

  const service = Object.create(VoteService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/Vote/';
  service.headers = { set: vi.fn() };

  return { service: service as VoteService, mockHttp };
}

describe('VoteService', () => {
  let service: VoteService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should call GET for getModel', async () => {
    await service.getModel();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Vote/', expect.anything());
  });

  it('should call POST for upvoteMovie with correct URL', async () => {
    await service.upvoteMovie(42);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Vote/up/movie/42', expect.anything());
  });

  it('should call POST for upvoteTv with correct URL', async () => {
    await service.upvoteTv(99);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Vote/up/tv/99', expect.anything());
  });

  it('should call POST for upvoteAlbum with correct URL', async () => {
    await service.upvoteAlbum(7);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Vote/up/album/7', expect.anything());
  });

  it('should call POST for downvoteMovie with correct URL', async () => {
    await service.downvoteMovie(42);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Vote/down/movie/42', expect.anything());
  });

  it('should call POST for downvoteTv with correct URL', async () => {
    await service.downvoteTv(99);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Vote/down/tv/99', expect.anything());
  });

  it('should call POST for downvoteAlbum with correct URL', async () => {
    await service.downvoteAlbum(7);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Vote/down/album/7', expect.anything());
  });
});
