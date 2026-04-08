import { describe, it, expect, vi } from 'vitest';
import { PushoverComponent } from './pushover.component';
import { UntypedFormBuilder } from '@angular/forms';
import { of } from 'rxjs';

function createComponent() {
  const mockSettingsService = {
    getPushoverNotificationSettings: vi.fn().mockReturnValue(of({
      enabled: true, userToken: 'user-tok', accessToken: 'access-tok',
      priority: 0, sound: 'default', notificationTemplates: [{ id: 1 }],
    })),
    savePushoverNotificationSettings: vi.fn().mockReturnValue(of(true)),
  };
  const mockNotify = { error: vi.fn(), success: vi.fn() };
  const fb = new UntypedFormBuilder();
  const mockTesterService = { pushoverTest: vi.fn().mockReturnValue(of(true)) };

  const comp = new PushoverComponent(mockSettingsService as any, mockNotify as any, fb, mockTesterService as any);
  return { comp, mockSettingsService, mockNotify, mockTesterService };
}

describe('PushoverComponent', () => {
  it('should load settings and build form', () => {
    const { comp } = createComponent();
    comp.ngOnInit();
    expect(comp.form.controls['accessToken'].value).toBe('access-tok');
  });

  it('should save and notify success', () => {
    const { comp, mockNotify, mockSettingsService } = createComponent();
    comp.ngOnInit();
    comp.onSubmit(comp.form);
    expect(mockSettingsService.savePushoverNotificationSettings).toHaveBeenCalled();
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved the Pushover settings');
  });

  it('should test and notify success', () => {
    const { comp, mockNotify, mockTesterService } = createComponent();
    comp.ngOnInit();
    comp.test(comp.form);
    expect(mockTesterService.pushoverTest).toHaveBeenCalled();
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully sent a Pushover message');
  });

  it('should test and notify error on failure', () => {
    const { comp, mockNotify, mockTesterService } = createComponent();
    mockTesterService.pushoverTest.mockReturnValue(of(false));
    comp.ngOnInit();
    comp.test(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('There was an error when sending the Pushover message. Please check your settings');
  });
});
