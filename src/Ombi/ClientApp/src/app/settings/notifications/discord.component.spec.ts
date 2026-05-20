import { describe, it, expect, vi, beforeEach } from 'vitest';
import { DiscordComponent } from './discord.component';
import { UntypedFormBuilder } from '@angular/forms';
import { of } from 'rxjs';

function createComponent() {
  const mockSettingsService = {
    getDiscordNotificationSettings: vi.fn().mockReturnValue(of({
      enabled: true, username: 'Ombi', webhookUrl: 'https://discord.com/hook',
      icon: '', hideUser: false, notificationTemplates: [{ id: 1 }],
    })),
    saveDiscordNotificationSettings: vi.fn().mockReturnValue(of(true)),
  };
  const mockNotify = { error: vi.fn(), success: vi.fn() };
  const fb = new UntypedFormBuilder();
  const mockTesterService = { discordTest: vi.fn().mockReturnValue(of(true)) };

  const comp = new DiscordComponent(mockSettingsService as any, mockNotify as any, fb, mockTesterService as any);
  return { comp, mockSettingsService, mockNotify, mockTesterService };
}

describe('DiscordComponent', () => {
  it('should load settings and build form on init', () => {
    const { comp } = createComponent();
    comp.ngOnInit();
    expect(comp.form).toBeDefined();
    expect(comp.form.controls['webhookUrl'].value).toBe('https://discord.com/hook');
    expect(comp.templates).toHaveLength(1);
  });

  it('should save settings and notify success', () => {
    const { comp, mockSettingsService, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.onSubmit(comp.form);
    expect(mockSettingsService.saveDiscordNotificationSettings).toHaveBeenCalled();
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved the Discord settings');
  });

  it('should show error when form is invalid on submit', () => {
    const { comp, mockNotify, mockSettingsService } = createComponent();
    comp.ngOnInit();
    comp.form.controls['webhookUrl'].setValue('');
    comp.onSubmit(comp.form);
    expect(mockSettingsService.saveDiscordNotificationSettings).not.toHaveBeenCalled();
    expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values');
  });

  it('should test and notify success', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.test(comp.form);
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully sent a Discord message, please check the discord channel');
  });

  it('should test and notify error on failure', () => {
    const { comp, mockNotify, mockTesterService } = createComponent();
    mockTesterService.discordTest.mockReturnValue(of(false));
    comp.ngOnInit();
    comp.test(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('There was an error when sending the Discord message. Please check your settings');
  });
});
