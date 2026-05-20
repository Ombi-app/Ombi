import { describe, it, expect, vi } from 'vitest';
import { GotifyComponent } from './gotify.component';
import { UntypedFormBuilder } from '@angular/forms';
import { of } from 'rxjs';

function createComponent() {
  const mockSettingsService = {
    getGotifyNotificationSettings: vi.fn().mockReturnValue(of({
      enabled: true, baseUrl: 'https://gotify.local', applicationToken: 'tok',
      priority: 5, notificationTemplates: [{ id: 1 }],
    })),
    saveGotifyNotificationSettings: vi.fn().mockReturnValue(of(true)),
  };
  const mockNotify = { error: vi.fn(), success: vi.fn() };
  const fb = new UntypedFormBuilder();
  const mockTesterService = { gotifyTest: vi.fn().mockReturnValue(of(true)) };

  const comp = new GotifyComponent(mockSettingsService as any, mockNotify as any, fb, mockTesterService as any);
  return { comp, mockSettingsService, mockNotify, mockTesterService };
}

describe('GotifyComponent', () => {
  it('should load settings and build form', () => {
    const { comp } = createComponent();
    comp.ngOnInit();
    expect(comp.form.controls['baseUrl'].value).toBe('https://gotify.local');
    expect(comp.form.controls['applicationToken'].value).toBe('tok');
  });

  it('should save and notify success', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.onSubmit(comp.form);
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved the Gotify settings');
  });

  it('should test and notify success', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.test(comp.form);
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully sent a Gotify message');
  });

  it('should error on invalid form submit', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.form.controls['baseUrl'].setValue('');
    comp.onSubmit(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values');
  });
});
