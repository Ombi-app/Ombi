import { describe, it, expect, vi } from 'vitest';
import { TelegramComponent } from './telegram.component';
import { UntypedFormBuilder } from '@angular/forms';
import { of } from 'rxjs';

function createComponent() {
  const mockSettingsService = {
    getTelegramNotificationSettings: vi.fn().mockReturnValue(of({
      enabled: true, botApi: 'bot-token', chatId: '123456', parseMode: 'HTML',
      notificationTemplates: [{ id: 1 }],
    })),
    saveTelegramNotificationSettings: vi.fn().mockReturnValue(of(true)),
  };
  const mockNotify = { error: vi.fn(), success: vi.fn() };
  const fb = new UntypedFormBuilder();
  const mockTesterService = { telegramTest: vi.fn().mockReturnValue(of(true)) };

  const comp = new TelegramComponent(mockSettingsService as any, mockNotify as any, fb, mockTesterService as any);
  return { comp, mockSettingsService, mockNotify, mockTesterService };
}

describe('TelegramComponent', () => {
  it('should load settings and build form', () => {
    const { comp } = createComponent();
    comp.ngOnInit();
    expect(comp.form.controls['botApi'].value).toBe('bot-token');
    expect(comp.form.controls['chatId'].value).toBe('123456');
  });

  it('should save and notify success', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.onSubmit(comp.form);
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved the Telegram settings');
  });

  it('should test and notify success', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.test(comp.form);
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully sent a Telegram message, please check the Telegram channel');
  });

  it('should test and notify error on failure', () => {
    const { comp, mockNotify, mockTesterService } = createComponent();
    mockTesterService.telegramTest.mockReturnValue(of(false));
    comp.ngOnInit();
    comp.test(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('There was an error when sending the Telegram message. Please check your settings');
  });

  it('should error on invalid form submit', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.form.controls['botApi'].setValue('');
    comp.onSubmit(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values');
  });
});
