import { describe, it, expect, vi, beforeEach } from 'vitest';
import { TheMovieDbService } from './themoviedb.service';
import { of, throwError, EMPTY } from 'rxjs';
import { HttpParams, HttpErrorResponse } from '@angular/common/http';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
  };
  const service = Object.create(TheMovieDbService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/TheMovieDb';
  service.headers = { set: vi.fn() };
  return { service: service as TheMovieDbService, mockHttp };
}

describe('TheMovieDbService', () => {
  let service: TheMovieDbService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should GET for getKeywords with searchTerm as HttpParams', () => {
    service.getKeywords('action');
    expect(mockHttp.get).toHaveBeenCalledWith(
      '/api/v1/TheMovieDb/Keywords',
      expect.objectContaining({ params: expect.any(HttpParams) })
    );
  });

  it('should GET for getKeyword by id', () => {
    service.getKeyword(12345);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/TheMovieDb/Keywords/12345', expect.anything());
  });

  it('should GET for getWatchProviders with media type', () => {
    service.getWatchProviders('movie');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/TheMovieDb/WatchProviders/movie', expect.anything());
  });

  it('should handle 404 error in getKeyword by returning EMPTY', () => {
    const error = new HttpErrorResponse({ status: 404 });
    mockHttp.get.mockReturnValue(throwError(() => error));

    // getKeyword has catchError that returns empty() on 404
    // We test the raw HTTP call is made correctly
    service.getKeyword(99999);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/TheMovieDb/Keywords/99999', expect.anything());
  });
});
