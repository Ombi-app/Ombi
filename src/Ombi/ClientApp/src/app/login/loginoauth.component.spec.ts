import { describe, it, expect, vi, beforeEach } from 'vitest';
import { LoginOAuthComponent } from './loginoauth.component';
import { of, throwError, Subject } from 'rxjs';

describe('LoginOAuthComponent', () => {
  let comp: LoginOAuthComponent;
  let mockAuthService: any;
  let mockRouter: any;
  let mockRoute: any;
  let mockNotify: any;
  let mockStore: any;
  let paramsSubject: Subject<any>;

  beforeEach(() => {
    paramsSubject = new Subject();

    mockAuthService = {
      oAuth: vi.fn().mockReturnValue(of({ access_token: 'jwt-token-123' })),
      loggedIn: vi.fn().mockReturnValue(true),
    };
    mockRouter = { navigate: vi.fn() };
    mockRoute = { params: paramsSubject.asObservable() };
    mockNotify = { error: vi.fn() };
    mockStore = { save: vi.fn() };

    comp = new LoginOAuthComponent(mockAuthService, mockRouter, mockRoute, mockNotify, mockStore);
  });

  it('should set pin from route params', () => {
    paramsSubject.next({ pin: 12345 });
    expect(comp.pin).toBe(12345);
  });

  it('should save token and navigate on successful auth', () => {
    comp.pin = 99999;
    comp.auth();
    expect(mockAuthService.oAuth).toHaveBeenCalledWith(99999);
    expect(mockStore.save).toHaveBeenCalledWith('id_token', 'jwt-token-123');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['search']);
  });

  it('should set error message when auth returns errorMessage', () => {
    mockAuthService.oAuth.mockReturnValue(of({ errorMessage: 'Invalid PIN' }));
    comp.pin = 12345;
    comp.auth();
    expect(comp.error).toBe('Invalid PIN');
  });

  it('should not save token when no access_token returned', () => {
    mockAuthService.oAuth.mockReturnValue(of({ errorMessage: 'Bad request' }));
    comp.pin = 12345;
    comp.auth();
    expect(mockStore.save).not.toHaveBeenCalled();
  });

  it('should notify error and navigate to login on HTTP error', () => {
    mockAuthService.oAuth.mockReturnValue(throwError(() => ({ statusText: 'Forbidden' })));
    comp.pin = 12345;
    comp.auth();
    expect(mockNotify.error).toHaveBeenCalledWith('Forbidden');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['login']);
  });
});
