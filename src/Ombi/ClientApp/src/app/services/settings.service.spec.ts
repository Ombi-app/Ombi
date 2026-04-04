import { describe, it, expect, beforeEach, vi } from 'vitest';
import { SettingsService } from './settings.service';
import { of } from 'rxjs';

function createMockSettingsService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of({})),
    post: vi.fn().mockReturnValue(of(true)),
    put: vi.fn().mockReturnValue(of(true)),
  };

  const service = Object.create(SettingsService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/Settings';
  service.headers = {};

  return { service: service as SettingsService, mockHttp };
}

describe('SettingsService', () => {
  let service: SettingsService;
  let mockHttp: ReturnType<typeof createMockSettingsService>['mockHttp'];

  beforeEach(() => {
    const mocks = createMockSettingsService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  describe('About & General', () => {
    it('should GET about', () => {
      service.about();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/About/', expect.anything());
    });

    it('should GET Ombi settings', () => {
      service.getOmbi();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Ombi/', expect.anything());
    });

    it('should POST to save Ombi settings', () => {
      const settings = { baseUrl: '/' } as any;
      service.saveOmbi(settings);
      expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Ombi/', JSON.stringify(settings), expect.anything());
    });

    it('should GET default language', () => {
      service.getDefaultLanguage();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/defaultlanguage/', expect.anything());
    });

    it('should POST to reset API key', () => {
      service.resetOmbiApi();
      expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Ombi/resetApi', expect.anything());
    });
  });

  describe('Media Server settings', () => {
    it('should GET Plex settings', () => {
      service.getPlex();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Plex/', expect.anything());
    });

    it('should POST to save Plex settings', () => {
      const settings = { enable: true } as any;
      service.savePlex(settings);
      expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Plex/', JSON.stringify(settings), expect.anything());
    });

    it('should GET Emby settings', () => {
      service.getEmby();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Emby/');
    });

    it('should GET Jellyfin settings', () => {
      service.getJellyfin();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Jellyfin/');
    });
  });

  describe('Download client settings', () => {
    it('should GET Sonarr settings', () => {
      service.getSonarr();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Sonarr', expect.anything());
    });

    it('should POST to save Sonarr settings', () => {
      const settings = { enabled: true } as any;
      service.saveSonarr(settings);
      expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Sonarr', JSON.stringify(settings), expect.anything());
    });

    it('should GET Radarr settings', () => {
      service.getRadarr();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Radarr', expect.anything());
    });

    it('should GET Lidarr settings', () => {
      service.getLidarr();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Lidarr', expect.anything());
    });

    it('should GET lidarr enabled status', () => {
      service.lidarrEnabled();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/lidarrenabled', expect.anything());
    });
  });

  describe('Authentication settings', () => {
    it('should GET authentication settings', () => {
      service.getAuthentication();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/Authentication', expect.anything());
    });

    it('should POST to save authentication settings', () => {
      const settings = { allowNoPassword: true } as any;
      service.saveAuthentication(settings);
      expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/Authentication', JSON.stringify(settings), expect.anything());
    });

    it('should GET client id', () => {
      service.getClientId();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/clientid', expect.anything());
    });
  });

  describe('Notification settings', () => {
    it('should GET email notification settings', () => {
      service.getEmailNotificationSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/email', expect.anything());
    });

    it('should GET email enabled status', () => {
      service.getEmailSettingsEnabled();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/email/enabled', expect.anything());
    });

    it('should GET Discord settings', () => {
      service.getDiscordNotificationSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/discord', expect.anything());
    });

    it('should GET Slack settings', () => {
      service.getSlackNotificationSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/slack', expect.anything());
    });

    it('should GET Telegram settings', () => {
      service.getTelegramNotificationSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/telegram', expect.anything());
    });

    it('should GET Pushbullet settings', () => {
      service.getPushbulletNotificationSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/pushbullet', expect.anything());
    });

    it('should GET Pushover settings', () => {
      service.getPushoverNotificationSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/pushover', expect.anything());
    });

    it('should GET Gotify settings', () => {
      service.getGotifyNotificationSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/gotify', expect.anything());
    });

    it('should GET Ntfy settings', () => {
      service.getNtfyNotificationSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/ntfy', expect.anything());
    });

    it('should GET Webhook settings', () => {
      service.getWebhookNotificationSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/webhook', expect.anything());
    });

    it('should GET Mobile settings', () => {
      service.getMobileNotificationSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/mobile', expect.anything());
    });

    it('should GET Mattermost settings', () => {
      service.getMattermostNotificationSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/mattermost', expect.anything());
    });

    it('should GET Twilio settings', () => {
      service.getTwilioSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/twilio', expect.anything());
    });

    it('should GET Newsletter settings', () => {
      service.getNewsletterSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/notifications/newsletter', expect.anything());
    });

    it('should POST to save Discord settings', () => {
      const settings = { webhookUrl: 'https://discord.test' } as any;
      service.saveDiscordNotificationSettings(settings);
      expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/discord', JSON.stringify(settings), expect.anything());
    });

    it('should POST to save Slack settings', () => {
      const settings = { webhookUrl: 'https://slack.test' } as any;
      service.saveSlackNotificationSettings(settings);
      expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/notifications/slack', JSON.stringify(settings), expect.anything());
    });
  });

  describe('Other settings', () => {
    it('should GET Landing Page settings', () => {
      service.getLandingPage();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/LandingPage', expect.anything());
    });

    it('should GET customization settings', () => {
      service.getCustomization();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/customization', expect.anything());
    });

    it('should GET job settings', () => {
      service.getJobSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/jobs', expect.anything());
    });

    it('should GET issue settings', () => {
      service.getIssueSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/issues', expect.anything());
    });

    it('should GET issues enabled', () => {
      service.issueEnabled();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/issuesenabled', expect.anything());
    });

    it('should GET vote settings', () => {
      service.getVoteSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/vote', expect.anything());
    });

    it('should GET vote enabled', () => {
      service.voteEnabled();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/voteenabled', expect.anything());
    });

    it('should GET TheMovieDb settings', () => {
      service.getTheMovieDbSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/themoviedb', expect.anything());
    });

    it('should GET UserManagement settings', () => {
      service.getUserManagementSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/UserManagement', expect.anything());
    });

    it('should GET Update settings', () => {
      service.getUpdateSettings();
      expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Settings/update', expect.anything());
    });

    it('should POST to test cron expression', () => {
      const body = { expression: '0 * * * *' } as any;
      service.testCron(body);
      expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Settings/testcron', JSON.stringify(body), expect.anything());
    });

    it('should POST to verify URL', () => {
      service.verifyUrl('https://ombi.example.com');
      expect(mockHttp.post).toHaveBeenCalledWith(
        '/api/v1/Settings/customization/urlverify',
        JSON.stringify({ url: 'https://ombi.example.com' }),
        expect.anything()
      );
    });
  });
});
