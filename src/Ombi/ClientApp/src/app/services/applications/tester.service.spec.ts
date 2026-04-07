import { describe, it, expect, vi, beforeEach } from 'vitest';
import { TesterService } from './tester.service';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    post: vi.fn().mockReturnValue(of(true)),
  };
  const service = Object.create(TesterService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/tester/';
  service.headers = { set: vi.fn() };
  return { service: service as TesterService, mockHttp };
}

describe('TesterService', () => {
  let service: TesterService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  const testCases: Array<{ method: string; endpoint: string }> = [
    { method: 'discordTest', endpoint: 'discord' },
    { method: 'pushbulletTest', endpoint: 'pushbullet' },
    { method: 'pushoverTest', endpoint: 'pushover' },
    { method: 'gotifyTest', endpoint: 'gotify' },
    { method: 'ntfyTest', endpoint: 'ntfy' },
    { method: 'webhookTest', endpoint: 'webhook' },
    { method: 'mattermostTest', endpoint: 'mattermost' },
    { method: 'whatsAppTest', endpoint: 'whatsapp' },
    { method: 'slackTest', endpoint: 'slack' },
    { method: 'emailTest', endpoint: 'email' },
    { method: 'plexTest', endpoint: 'plex' },
    { method: 'embyTest', endpoint: 'emby' },
    { method: 'jellyfinTest', endpoint: 'jellyfin' },
    { method: 'radarrTest', endpoint: 'radarr' },
    { method: 'lidarrTest', endpoint: 'lidarr' },
    { method: 'sonarrTest', endpoint: 'sonarr' },
    { method: 'couchPotatoTest', endpoint: 'couchpotato' },
    { method: 'telegramTest', endpoint: 'telegram' },
    { method: 'sickrageTest', endpoint: 'sickrage' },
    { method: 'newsletterTest', endpoint: 'newsletter' },
    { method: 'mobileNotificationTest', endpoint: 'mobile' },
  ];

  testCases.forEach(({ method, endpoint }) => {
    it(`should POST for ${method} to ${endpoint}`, () => {
      const settings = { enabled: true } as any;
      (service as any)[method](settings);
      expect(mockHttp.post).toHaveBeenCalledWith(
        `/api/v1/tester/${endpoint}`,
        JSON.stringify(settings),
        expect.anything()
      );
    });
  });
});
