import { describe, it, expect, beforeEach, vi } from 'vitest';
import { AuthGuard } from './auth.guard';

describe('AuthGuard', () => {
  let guard: AuthGuard;
  let mockAuth: { loggedIn: ReturnType<typeof vi.fn> };
  let mockRouter: { navigate: ReturnType<typeof vi.fn> };
  let mockStore: { remove: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    mockAuth = { loggedIn: vi.fn() };
    mockRouter = { navigate: vi.fn() };
    mockStore = { remove: vi.fn() };

    guard = Object.create(AuthGuard.prototype);
    (guard as any).auth = mockAuth;
    (guard as any).router = mockRouter;
    (guard as any).store = mockStore;
  });

  it('should return true when user is logged in', () => {
    mockAuth.loggedIn.mockReturnValue(true);
    expect(guard.canActivate()).toBe(true);
  });

  it('should return false when user is not logged in', () => {
    mockAuth.loggedIn.mockReturnValue(false);
    expect(guard.canActivate()).toBe(false);
  });

  it('should navigate to login when not logged in', () => {
    mockAuth.loggedIn.mockReturnValue(false);
    guard.canActivate();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['login']);
  });

  it('should remove token from store when not logged in', () => {
    mockAuth.loggedIn.mockReturnValue(false);
    guard.canActivate();
    expect(mockStore.remove).toHaveBeenCalledWith('token');
  });

  it('should not navigate when user is logged in', () => {
    mockAuth.loggedIn.mockReturnValue(true);
    guard.canActivate();
    expect(mockRouter.navigate).not.toHaveBeenCalled();
  });
});
