import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { CookieComponent } from './cookie.component';

describe('CookieComponent', () => {
  let comp: CookieComponent;
  let mockRouter: any;
  let mockStore: any;

  beforeEach(() => {
    mockRouter = {
      navigate: vi.fn(),
    };
    mockStore = {
      save: vi.fn(),
    };
    comp = new CookieComponent(mockRouter, mockStore);
  });

  afterEach(() => {
    // Clean up cookies
    document.cookie = 'Auth=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
  });

  it('should save token and navigate to discover when Auth cookie exists', () => {
    document.cookie = 'Auth=test-jwt-token; path=/';
    comp.ngOnInit();
    expect(mockStore.save).toHaveBeenCalledWith('id_token', 'test-jwt-token');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['discover']);
  });

  it('should navigate to login when Auth cookie does not exist', () => {
    // Ensure no Auth cookie
    document.cookie = 'Auth=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
    comp.ngOnInit();
    expect(mockStore.save).not.toHaveBeenCalled();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['login']);
  });
});
