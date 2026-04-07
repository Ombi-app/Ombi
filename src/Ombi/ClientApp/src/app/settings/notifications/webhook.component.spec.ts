import { describe, it, expect, vi } from 'vitest';
import { WebhookComponent } from './webhook.component';
import { UntypedFormBuilder } from '@angular/forms';
import { of } from 'rxjs';

function createComponent() {
  const mockSettingsService = {
    getWebhookNotificationSettings: vi.fn().mockReturnValue(of({
      enabled: true, webhookUrl: 'https://example.com/hook', applicationToken: 'tok',
    })),
    saveWebhookNotificationSettings: vi.fn().mockReturnValue(of(true)),
  };
  const mockNotify = { error: vi.fn(), success: vi.fn() };
  const fb = new UntypedFormBuilder();
  const mockTesterService = { webhookTest: vi.fn().mockReturnValue(of(true)) };

  const comp = new WebhookComponent(mockSettingsService as any, mockNotify as any, fb, mockTesterService as any);
  return { comp, mockSettingsService, mockNotify, mockTesterService };
}

describe('WebhookComponent', () => {
  it('should load settings and build form', () => {
    const { comp } = createComponent();
    comp.ngOnInit();
    expect(comp.form.controls['webhookUrl'].value).toBe('https://example.com/hook');
  });

  it('should save and notify success', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.onSubmit(comp.form);
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved the Webhook settings');
  });

  it('should test and notify success', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.test(comp.form);
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully sent a Webhook message');
  });

  it('should test and notify error on failure', () => {
    const { comp, mockNotify, mockTesterService } = createComponent();
    mockTesterService.webhookTest.mockReturnValue(of(false));
    comp.ngOnInit();
    comp.test(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('There was an error when sending the Webhook message. Please check your settings');
  });
});
