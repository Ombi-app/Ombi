import { describe, it, expect, vi } from 'vitest';
import { EmailNotificationComponent } from './emailnotification.component';
import { UntypedFormBuilder } from '@angular/forms';
import { of } from 'rxjs';

function createComponent(authEnabled = false) {
  const mockSettingsService = {
    getEmailNotificationSettings: vi.fn().mockReturnValue(of({
      enabled: true, authentication: authEnabled, host: 'smtp.example.com',
      password: '', port: 587, senderAddress: 'ombi@example.com',
      senderName: 'Ombi', username: '', disableTLS: false,
      disableCertificateChecking: false, notificationTemplates: [{ id: 1 }],
    })),
    saveEmailNotificationSettings: vi.fn().mockReturnValue(of(true)),
  };
  const mockNotify = { error: vi.fn(), success: vi.fn() };
  const fb = new UntypedFormBuilder();
  const mockValidationService = {
    enableValidation: vi.fn(),
    disableValidation: vi.fn(),
  };
  const mockTesterService = { emailTest: vi.fn().mockReturnValue(of(true)) };

  const comp = new EmailNotificationComponent(
    mockSettingsService as any, mockNotify as any, fb,
    mockValidationService as any, mockTesterService as any,
  );
  return { comp, mockSettingsService, mockNotify, mockValidationService, mockTesterService };
}

describe('EmailNotificationComponent', () => {
  it('should load settings and build form', () => {
    const { comp } = createComponent();
    comp.ngOnInit();
    expect(comp.emailForm).toBeDefined();
    expect(comp.emailForm.controls['host'].value).toBe('smtp.example.com');
    expect(comp.emailForm.controls['port'].value).toBe(587);
  });

  it('should enable username/password validation when authentication is enabled', () => {
    const { comp, mockValidationService } = createComponent(true);
    comp.ngOnInit();
    expect(mockValidationService.enableValidation).toHaveBeenCalledWith(comp.emailForm, 'username');
    expect(mockValidationService.enableValidation).toHaveBeenCalledWith(comp.emailForm, 'password');
  });

  it('should toggle validation when authentication changes', () => {
    const { comp, mockValidationService } = createComponent(false);
    comp.ngOnInit();

    // Toggle auth on
    comp.emailForm.controls['authentication'].setValue(true);
    expect(mockValidationService.enableValidation).toHaveBeenCalledWith(comp.emailForm, 'username');

    // Toggle auth off
    comp.emailForm.controls['authentication'].setValue(false);
    expect(mockValidationService.disableValidation).toHaveBeenCalledWith(comp.emailForm, 'username');
  });

  it('should save and notify success', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.onSubmit(comp.emailForm);
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved Email settings');
  });

  it('should error when form is invalid on submit', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.emailForm.controls['host'].setValue('');
    comp.onSubmit(comp.emailForm);
    expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values');
  });

  it('should test and notify success', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.test(comp.emailForm);
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully sent an email message, please check your inbox');
  });

  it('should test and notify error on failure', () => {
    const { comp, mockNotify, mockTesterService } = createComponent();
    mockTesterService.emailTest.mockReturnValue(of(false));
    comp.ngOnInit();
    comp.test(comp.emailForm);
    expect(mockNotify.error).toHaveBeenCalledWith('There was an error when sending the Email message, please check your settings.');
  });
});
