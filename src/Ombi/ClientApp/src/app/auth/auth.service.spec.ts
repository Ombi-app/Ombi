import { describe, it, expect, beforeEach, vi } from 'vitest';
import { AuthService } from './auth.service';
import { StorageService } from '../shared/storage/storage-service';

// Mock the Angular/HTTP dependencies at the module boundary
function createMockAuthService() {
  const mockStore = new StorageService();
  const mockJwtHelper = {
    tokenGetter: vi.fn(),
    isTokenExpired: vi.fn(),
    decodeToken: vi.fn(),
  };
  const mockHttp = {
    post: vi.fn(),
    get: vi.fn(),
  };

  // Create the service by directly setting its properties
  // (bypassing Angular DI since we're testing logic, not DI wiring)
  const service = Object.create(AuthService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/token';
  service.href = '/';
  service.headers = { set: vi.fn() };
  service.jwtHelperService = mockJwtHelper;
  service.store = mockStore;

  return { service: service as AuthService, mockJwtHelper, mockHttp, mockStore };
}

describe('AuthService', () => {
  let service: AuthService;
  let mockJwtHelper: ReturnType<typeof createMockAuthService>['mockJwtHelper'];
  let mockStore: StorageService;

  beforeEach(() => {
    localStorage.clear();
    const mocks = createMockAuthService();
    service = mocks.service;
    mockJwtHelper = mocks.mockJwtHelper;
    mockStore = mocks.mockStore;
  });

  describe('loggedIn', () => {
    it('should return false when no token exists', () => {
      mockJwtHelper.tokenGetter.mockReturnValue(null);
      expect(service.loggedIn()).toBe(false);
    });

    it('should return false when token is expired', () => {
      mockJwtHelper.tokenGetter.mockReturnValue('some-token');
      mockJwtHelper.isTokenExpired.mockReturnValue(true);
      expect(service.loggedIn()).toBe(false);
    });

    it('should return true when token exists and is not expired', () => {
      mockJwtHelper.tokenGetter.mockReturnValue('valid-token');
      mockJwtHelper.isTokenExpired.mockReturnValue(false);
      expect(service.loggedIn()).toBe(true);
    });

    it('should return false for empty string token', () => {
      mockJwtHelper.tokenGetter.mockReturnValue('');
      expect(service.loggedIn()).toBe(false);
    });
  });

  describe('claims', () => {
    it('should return empty object when not logged in', () => {
      mockJwtHelper.tokenGetter.mockReturnValue(null);
      const claims = service.claims();
      expect(claims).toEqual({});
    });

    it('should parse token with array of roles', () => {
      mockJwtHelper.tokenGetter.mockReturnValue('valid-token');
      mockJwtHelper.isTokenExpired.mockReturnValue(false);
      mockStore.save('id_token', 'valid-token');
      mockJwtHelper.decodeToken.mockReturnValue({
        sub: 'testuser',
        Email: 'test@example.com',
        role: ['Admin', 'User'],
      });

      const claims = service.claims();
      expect(claims.name).toBe('testuser');
      expect(claims.email).toBe('test@example.com');
      expect(claims.roles).toEqual(['Admin', 'User']);
    });

    it('should wrap single role string in array', () => {
      mockJwtHelper.tokenGetter.mockReturnValue('valid-token');
      mockJwtHelper.isTokenExpired.mockReturnValue(false);
      mockStore.save('id_token', 'valid-token');
      mockJwtHelper.decodeToken.mockReturnValue({
        sub: 'testuser',
        Email: 'test@example.com',
        role: 'Admin',
      });

      const claims = service.claims();
      expect(claims.roles).toEqual(['Admin']);
    });

    it('should throw error when logged in but no id_token in store', () => {
      mockJwtHelper.tokenGetter.mockReturnValue('valid-token');
      mockJwtHelper.isTokenExpired.mockReturnValue(false);
      // Don't save id_token to store

      expect(() => service.claims()).toThrow('Invalid token');
    });
  });

  describe('hasRole', () => {
    beforeEach(() => {
      mockJwtHelper.tokenGetter.mockReturnValue('valid-token');
      mockJwtHelper.isTokenExpired.mockReturnValue(false);
      mockStore.save('id_token', 'valid-token');
    });

    it('should return true when user has the role', () => {
      mockJwtHelper.decodeToken.mockReturnValue({
        sub: 'user', Email: 'u@e.com', role: ['Admin', 'User'],
      });
      expect(service.hasRole('Admin')).toBe(true);
    });

    it('should be case-insensitive', () => {
      mockJwtHelper.decodeToken.mockReturnValue({
        sub: 'user', Email: 'u@e.com', role: ['Admin'],
      });
      expect(service.hasRole('admin')).toBe(true);
      expect(service.hasRole('ADMIN')).toBe(true);
    });

    it('should return false when user does not have the role', () => {
      mockJwtHelper.decodeToken.mockReturnValue({
        sub: 'user', Email: 'u@e.com', role: ['User'],
      });
      expect(service.hasRole('Admin')).toBe(false);
    });

    it('should return false when not logged in', () => {
      mockJwtHelper.tokenGetter.mockReturnValue(null);
      expect(service.hasRole('Admin')).toBe(false);
    });
  });

  describe('isAdmin', () => {
    beforeEach(() => {
      mockJwtHelper.tokenGetter.mockReturnValue('valid-token');
      mockJwtHelper.isTokenExpired.mockReturnValue(false);
      mockStore.save('id_token', 'valid-token');
    });

    it('should return true for Admin role', () => {
      mockJwtHelper.decodeToken.mockReturnValue({
        sub: 'user', Email: 'u@e.com', role: ['Admin'],
      });
      expect(service.isAdmin()).toBe(true);
    });

    it('should return true for PowerUser role', () => {
      mockJwtHelper.decodeToken.mockReturnValue({
        sub: 'user', Email: 'u@e.com', role: ['PowerUser'],
      });
      expect(service.isAdmin()).toBe(true);
    });

    it('should return false for regular User role', () => {
      mockJwtHelper.decodeToken.mockReturnValue({
        sub: 'user', Email: 'u@e.com', role: ['User'],
      });
      expect(service.isAdmin()).toBe(false);
    });
  });

  describe('logout', () => {
    it('should remove id_token from store', () => {
      mockStore.save('id_token', 'some-token');
      service.logout();
      expect(mockStore.get('id_token')).toBeNull();
    });
  });
});
