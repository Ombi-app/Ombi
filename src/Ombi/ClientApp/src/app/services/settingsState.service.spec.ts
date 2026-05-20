import { describe, it, expect, beforeEach } from 'vitest';
import { SettingsStateService } from './settingsState.service';

describe('SettingsStateService', () => {
  let service: SettingsStateService;

  beforeEach(() => {
    service = new SettingsStateService();
  });

  it('should return undefined when issues not set', () => {
    expect(service.getIssue()).toBeUndefined();
  });

  it('should store and retrieve issues enabled state', () => {
    service.setIssue(true);
    expect(service.getIssue()).toBe(true);
  });

  it('should store false for issues', () => {
    service.setIssue(false);
    expect(service.getIssue()).toBe(false);
  });

  it('should overwrite previously set value', () => {
    service.setIssue(true);
    service.setIssue(false);
    expect(service.getIssue()).toBe(false);
  });
});
