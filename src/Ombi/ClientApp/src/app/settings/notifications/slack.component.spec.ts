import { describe, it, expect, vi, beforeEach } from 'vitest';
import { SlackComponent } from './slack.component';
import { UntypedFormBuilder } from '@angular/forms';
import { of } from 'rxjs';

function createComponent() {
  const mockSettingsService = {
    getSlackNotificationSettings: vi.fn().mockReturnValue(of({
      enabled: true, username: 'Ombi', webhookUrl: 'https://hooks.slack.com/test',
      iconEmoji: '', iconUrl: '', channel: '#requests',
      notificationTemplates: [{ id: 1 }],
    })),
    saveSlackNotificationSettings: vi.fn().mockReturnValue(of(true)),
  };
  const mockNotify = { error: vi.fn(), success: vi.fn() };
  const fb = new UntypedFormBuilder();
  const mockTesterService = { slackTest: vi.fn().mockReturnValue(of(true)) };

  const comp = new SlackComponent(mockSettingsService as any, mockNotify as any, fb, mockTesterService as any);
  return { comp, mockSettingsService, mockNotify, mockTesterService };
}

describe('SlackComponent', () => {
  it('should load settings and build form', () => {
    const { comp } = createComponent();
    comp.ngOnInit();
    expect(comp.form.controls['webhookUrl'].value).toBe('https://hooks.slack.com/test');
    expect(comp.form.controls['channel'].value).toBe('#requests');
  });

  it('should save settings on valid submit', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.onSubmit(comp.form);
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved the Slack settings');
  });

  it('should error when both iconEmoji and iconUrl are set on submit', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.form.controls['iconEmoji'].setValue(':robot:');
    comp.form.controls['iconUrl'].setValue('https://example.com/icon.png');
    comp.onSubmit(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('You cannot have a Emoji icon and a URL icon');
  });

  it('should error when both iconEmoji and iconUrl are set on test', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.form.controls['iconEmoji'].setValue(':robot:');
    comp.form.controls['iconUrl'].setValue('https://example.com/icon.png');
    comp.test(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('You cannot have a Emoji icon and a URL icon');
  });

  it('should test and notify success', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.test(comp.form);
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully sent a Slack message, please check the slack channel');
  });

  it('should test and notify error on failure', () => {
    const { comp, mockNotify, mockTesterService } = createComponent();
    mockTesterService.slackTest.mockReturnValue(of(false));
    comp.ngOnInit();
    comp.test(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('There was an error when sending the Slack message. Please check your settings');
  });
});
