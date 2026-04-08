import { describe, it, expect, vi, beforeEach } from 'vitest';
import { IdentityService } from './identity.service';
import { of } from 'rxjs';

function createService(baseHref = '/') {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of({})),
    post: vi.fn().mockReturnValue(of({})),
    put: vi.fn().mockReturnValue(of({})),
    delete: vi.fn().mockReturnValue(of({})),
  };

  const service = Object.create(IdentityService.prototype);
  service.http = mockHttp;
  service.url = baseHref.length > 1 ? baseHref + '/api/v1/Identity/' : '/api/v1/Identity/';
  service.headers = { set: vi.fn() };

  return { service: service as IdentityService, mockHttp };
}

describe('IdentityService', () => {
  let service: IdentityService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should call GET for getUser', () => {
    service.getUser();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Identity/', expect.anything());
  });

  it('should call GET for getAccessToken', () => {
    service.getAccessToken();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Identity/accesstoken', expect.anything());
  });

  it('should call GET for getUserAccessToken with userId', () => {
    service.getUserAccessToken('user-123');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Identity/accesstoken/user-123', expect.anything());
  });

  it('should call GET for getUserById', () => {
    service.getUserById('abc');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Identity/User/abc', expect.anything());
  });

  it('should call GET for getUsers', () => {
    service.getUsers();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Identity/Users', expect.anything());
  });

  it('should call GET for getUsersDropdown', () => {
    service.getUsersDropdown();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Identity/dropdown/Users', expect.anything());
  });

  it('should call GET for getAllAvailableClaims', () => {
    service.getAllAvailableClaims();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Identity/Claims', expect.anything());
  });

  it('should call POST for createWizardUser', () => {
    const user = { username: 'admin', password: 'pass', usePlexAdminAccount: false } as any;
    service.createWizardUser(user);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Identity/Wizard/', JSON.stringify(user), expect.anything());
  });

  it('should call POST for createUser', () => {
    const user = { id: '1', userName: 'test' } as any;
    service.createUser(user);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Identity/', JSON.stringify(user), expect.anything());
  });

  it('should call PUT for updateUser', () => {
    const user = { id: '1', userName: 'updated' } as any;
    service.updateUser(user);
    expect(mockHttp.put).toHaveBeenCalledWith('/api/v1/Identity/', JSON.stringify(user), expect.anything());
  });

  it('should call PUT for updateLocalUser', () => {
    const user = { currentPassword: 'old', newPassword: 'new' } as any;
    service.updateLocalUser(user);
    expect(mockHttp.put).toHaveBeenCalledWith('/api/v1/Identity/local', JSON.stringify(user), expect.anything());
  });

  it('should call DELETE for deleteUser', () => {
    const user = { id: 'user-456' } as any;
    service.deleteUser(user);
    expect(mockHttp.delete).toHaveBeenCalledWith('/api/v1/Identity/user-456', expect.anything());
  });

  it('should call GET for hasUserRequested', () => {
    service.hasUserRequested('user-789');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Identity/userhasrequest/user-789', expect.anything());
  });

  it('should call POST for submitResetPassword', () => {
    service.submitResetPassword('user@example.com');
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Identity/reset', JSON.stringify({ email: 'user@example.com' }), expect.anything());
  });

  it('should call POST for resetPassword', () => {
    const token = { email: 'a@b.com', password: 'new', token: 'tok' } as any;
    service.resetPassword(token);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Identity/resetpassword', JSON.stringify(token), expect.anything());
  });

  it('should call POST for sendWelcomeEmail', () => {
    const user = { id: '1', email: 'a@b.com' } as any;
    service.sendWelcomeEmail(user);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Identity/welcomeEmail', JSON.stringify(user), expect.anything());
  });

  it('should call GET for getNotificationPreferences', () => {
    service.getNotificationPreferences();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Identity/notificationpreferences', expect.anything());
  });

  it('should call GET for getNotificationPreferencesForUser', () => {
    service.getNotificationPreferencesForUser('user-1');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Identity/notificationpreferences/user-1', expect.anything());
  });

  it('should call POST for updateNotificationPreferences', () => {
    const prefs = [{ id: 1 }] as any;
    service.updateNotificationPreferences(prefs);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Identity/NotificationPreferences', JSON.stringify(prefs), expect.anything());
  });

  it('should call POST for updateLanguage', () => {
    service.updateLanguage('en');
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Identity/language', { lang: 'en' }, expect.anything());
  });

  it('should call GET for getSupportedStreamingCountries', () => {
    service.getSupportedStreamingCountries();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Identity/streamingcountry', expect.anything());
  });

  it('should call POST for updateStreamingCountry', () => {
    service.updateStreamingCountry('US');
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Identity/streamingcountry', { code: 'US' }, expect.anything());
  });

  it('should call GET for unsubscribeNewsletter', () => {
    service.unsubscribeNewsletter('user-1');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Identity/newsletter/unsubscribe/user-1', expect.anything());
  });

  describe('with non-root base href', () => {
    beforeEach(() => {
      const mocks = createService('/ombi');
      service = mocks.service;
      mockHttp = mocks.mockHttp;
    });

    it('should prepend base href to URL', () => {
      service.getUser();
      expect(mockHttp.get).toHaveBeenCalledWith('/ombi/api/v1/Identity/', expect.anything());
    });
  });
});
