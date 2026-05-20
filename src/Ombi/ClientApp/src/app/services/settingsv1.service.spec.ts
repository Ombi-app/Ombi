import { describe, it, expect, vi, beforeEach } from 'vitest';
import { SettingsService } from './settings.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of({})),
    post: vi.fn().mockReturnValue(of(true)),
  };
  const service = Object.create(SettingsService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/Settings';
  service.headers = { set: vi.fn() };
  return { service: service as SettingsService, mockHttp };
}

describe('SettingsService', () => {
  let service: SettingsService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  // About & Ombi
  it('should GET about', () => { service.about(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/About/', expect.anything()); });
  it('should GET getOmbi', () => { service.getOmbi(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Ombi/', expect.anything()); });
  it('should GET getDefaultLanguage', () => { service.getDefaultLanguage(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/defaultlanguage/', expect.anything()); });
  it('should POST saveOmbi', () => { service.saveOmbi({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Ombi/', expect.any(String), expect.anything()); });
  it('should POST resetOmbiApi', () => { service.resetOmbiApi(); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Ombi/resetApi', expect.anything()); });

  // Media servers
  it('should GET getEmby', () => { service.getEmby(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Emby/'); });
  it('should POST saveEmby', () => { service.saveEmby({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Emby/', expect.any(String), expect.anything()); });
  it('should GET getJellyfin', () => { service.getJellyfin(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Jellyfin/'); });
  it('should POST saveJellyfin', () => { service.saveJellyfin({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Jellyfin/', expect.any(String), expect.anything()); });
  it('should GET getPlex', () => { service.getPlex(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Plex/', expect.anything()); });
  it('should POST savePlex', () => { service.savePlex({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Plex/', expect.any(String), expect.anything()); });

  // Arr services
  it('should GET getSonarr', () => { service.getSonarr(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Sonarr', expect.anything()); });
  it('should POST saveSonarr', () => { service.saveSonarr({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Sonarr', expect.any(String), expect.anything()); });
  it('should GET getRadarr', () => { service.getRadarr(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Radarr', expect.anything()); });
  it('should POST saveRadarr', () => { service.saveRadarr({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Radarr', expect.any(String), expect.anything()); });
  it('should GET getLidarr', () => { service.getLidarr(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Lidarr', expect.anything()); });
  it('should GET lidarrEnabled', () => { service.lidarrEnabled(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/lidarrenabled', expect.anything()); });
  it('should POST saveLidarr', () => { service.saveLidarr({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Lidarr', expect.any(String), expect.anything()); });

  // Auth & customization
  it('should GET getAuthentication', () => { service.getAuthentication(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Authentication', expect.anything()); });
  it('should GET getClientId', () => { service.getClientId(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/clientid', expect.anything()); });
  it('should POST saveAuthentication', () => { service.saveAuthentication({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Authentication', expect.any(String), expect.anything()); });
  it('should GET getLandingPage', () => { service.getLandingPage(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/LandingPage', expect.anything()); });
  it('should POST saveLandingPage', () => { service.saveLandingPage({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/LandingPage', expect.any(String), expect.anything()); });
  it('should GET getCustomization', () => { service.getCustomization(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/customization', expect.anything()); });
  it('should POST saveCustomization', () => { service.saveCustomization({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/customization', expect.any(String), expect.anything()); });

  // Notification settings
  it('should GET getEmailNotificationSettings', () => { service.getEmailNotificationSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/email', expect.anything()); });
  it('should GET getEmailSettingsEnabled', () => { service.getEmailSettingsEnabled(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/email/enabled', expect.anything()); });
  it('should POST saveEmailNotificationSettings', () => { service.saveEmailNotificationSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/email', expect.any(String), expect.anything()); });
  it('should GET getDiscordNotificationSettings', () => { service.getDiscordNotificationSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/discord', expect.anything()); });
  it('should POST saveDiscordNotificationSettings', () => { service.saveDiscordNotificationSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/discord', expect.any(String), expect.anything()); });
  it('should GET getMattermostNotificationSettings', () => { service.getMattermostNotificationSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/mattermost', expect.anything()); });
  it('should POST saveMattermostNotificationSettings', () => { service.saveMattermostNotificationSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/mattermost', expect.any(String), expect.anything()); });
  it('should GET getPushbulletNotificationSettings', () => { service.getPushbulletNotificationSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/pushbullet', expect.anything()); });
  it('should POST savePushbulletNotificationSettings', () => { service.savePushbulletNotificationSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/pushbullet', expect.any(String), expect.anything()); });
  it('should GET getPushoverNotificationSettings', () => { service.getPushoverNotificationSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/pushover', expect.anything()); });
  it('should POST savePushoverNotificationSettings', () => { service.savePushoverNotificationSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/pushover', expect.any(String), expect.anything()); });
  it('should GET getGotifyNotificationSettings', () => { service.getGotifyNotificationSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/gotify', expect.anything()); });
  it('should POST saveGotifyNotificationSettings', () => { service.saveGotifyNotificationSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/gotify', expect.any(String), expect.anything()); });
  it('should GET getNtfyNotificationSettings', () => { service.getNtfyNotificationSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/ntfy', expect.anything()); });
  it('should POST saveNtfyNotificationSettings', () => { service.saveNtfyNotificationSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/ntfy', expect.any(String), expect.anything()); });
  it('should GET getWebhookNotificationSettings', () => { service.getWebhookNotificationSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/webhook', expect.anything()); });
  it('should POST saveWebhookNotificationSettings', () => { service.saveWebhookNotificationSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/webhook', expect.any(String), expect.anything()); });
  it('should GET getSlackNotificationSettings', () => { service.getSlackNotificationSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/slack', expect.anything()); });
  it('should POST saveSlackNotificationSettings', () => { service.saveSlackNotificationSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/slack', expect.any(String), expect.anything()); });
  it('should GET getMobileNotificationSettings', () => { service.getMobileNotificationSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/mobile', expect.anything()); });
  it('should POST saveMobileNotificationSettings', () => { service.saveMobileNotificationSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/mobile', expect.any(String), expect.anything()); });
  it('should GET getTelegramNotificationSettings', () => { service.getTelegramNotificationSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/telegram', expect.anything()); });
  it('should POST saveTelegramNotificationSettings', () => { service.saveTelegramNotificationSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/telegram', expect.any(String), expect.anything()); });
  it('should GET getTwilioSettings', () => { service.getTwilioSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/twilio', expect.anything()); });
  it('should POST saveTwilioSettings', () => { service.saveTwilioSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/twilio', expect.any(String), expect.anything()); });
  it('should GET getNewsletterSettings', () => { service.getNewsletterSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/newsletter', expect.anything()); });
  it('should POST updateNewsletterDatabase', () => { service.updateNewsletterDatabase(); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/newsletterdatabase', expect.anything()); });
  it('should POST saveNewsletterSettings', () => { service.saveNewsletterSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/newsletter', expect.any(String), expect.anything()); });

  // Update, user management, misc
  it('should GET getUpdateSettings', () => { service.getUpdateSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/update', expect.anything()); });
  it('should POST saveUpdateSettings', () => { service.saveUpdateSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/update', expect.any(String), expect.anything()); });
  it('should GET getUserManagementSettings', () => { service.getUserManagementSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/UserManagement', expect.anything()); });
  it('should POST saveUserManagementSettings', () => { service.saveUserManagementSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/UserManagement', expect.any(String), expect.anything()); });
  it('should GET getCouchPotatoSettings', () => { service.getCouchPotatoSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/CouchPotato', expect.anything()); });
  it('should POST saveCouchPotatoSettings', () => { service.saveCouchPotatoSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/CouchPotato', expect.any(String), expect.anything()); });
  it('should GET getDogNzbSettings', () => { service.getDogNzbSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/DogNzb', expect.anything()); });
  it('should POST saveDogNzbSettings', () => { service.saveDogNzbSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/DogNzb', expect.any(String), expect.anything()); });
  it('should GET getSickRageSettings', () => { service.getSickRageSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/sickrage', expect.anything()); });
  it('should POST saveSickRageSettings', () => { service.saveSickRageSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/sickrage', expect.any(String), expect.anything()); });
  it('should GET getJobSettings', () => { service.getJobSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/jobs', expect.anything()); });
  it('should POST saveJobSettings', () => { service.saveJobSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/jobs', expect.any(String), expect.anything()); });
  it('should POST testCron', () => {
    service.testCron({ expression: '0 * * * *' });
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/testcron', expect.any(String), expect.anything());
    expect(mockHttp.post.mock.calls[0][1]).toContain('0 * * * *');
  });
  it('should GET getIssueSettings', () => { service.getIssueSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/issues', expect.anything()); });
  it('should GET issueEnabled', () => { service.issueEnabled(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/issuesenabled', expect.anything()); });
  it('should POST saveIssueSettings', () => { service.saveIssueSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/issues', expect.any(String), expect.anything()); });
  it('should GET getVoteSettings', () => { service.getVoteSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/vote', expect.anything()); });
  it('should GET voteEnabled', () => { service.voteEnabled(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/voteenabled', expect.anything()); });
  it('should POST saveVoteSettings', () => { service.saveVoteSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/vote', expect.any(String), expect.anything()); });
  it('should GET getTheMovieDbSettings', () => { service.getTheMovieDbSettings(); expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/themoviedb', expect.anything()); });
  it('should POST saveTheMovieDbSettings', () => { service.saveTheMovieDbSettings({} as any); expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/themoviedb', expect.any(String), expect.anything()); });
  it('should POST verifyUrl', () => {
    service.verifyUrl('http://test.com');
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/customization/urlverify', expect.any(String), expect.anything());
    expect(mockHttp.post.mock.calls[0][1]).toContain('http://test.com');
  });
});
