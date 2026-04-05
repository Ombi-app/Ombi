import { describe, it, expect, vi } from 'vitest';
import { ServiceHelpers } from './service.helpers';

describe('ServiceHelpers', () => {
  it('should set Content-Type header to application/json', () => {
    const mockHttp = {} as any;
    const helper = new (ServiceHelpers as any)(mockHttp, '/api/v1/test', '/');
    expect(helper.headers.get('Content-Type')).toBe('application/json');
  });

  it('should not prepend base href when it is "/"', () => {
    const mockHttp = {} as any;
    const helper = new (ServiceHelpers as any)(mockHttp, '/api/v1/test', '/');
    expect(helper.url).toBe('/api/v1/test');
  });

  it('should prepend base href when it is longer than 1 character', () => {
    const mockHttp = {} as any;
    const helper = new (ServiceHelpers as any)(mockHttp, '/api/v1/test', '/ombi');
    expect(helper.url).toBe('/ombi/api/v1/test');
  });

  it('should store the http client reference', () => {
    const mockHttp = { get: vi.fn() } as any;
    const helper = new (ServiceHelpers as any)(mockHttp, '/api/v1/test', '/');
    expect(helper.http).toBe(mockHttp);
  });
});
