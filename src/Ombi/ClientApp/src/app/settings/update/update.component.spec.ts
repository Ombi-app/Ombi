import { describe, it, expect, vi } from 'vitest';
import { UpdateComponent } from './update.component';
import { UntypedFormBuilder } from '@angular/forms';
import { of } from 'rxjs';

function createComponent() {
  const mockSettingsService = {
    getUpdateSettings: vi.fn().mockReturnValue(of({
      autoUpdateEnabled: false,
      username: '',
      password: '',
      processName: 'Ombi',
      useScript: false,
      scriptLocation: '',
      windowsService: false,
      windowsServiceName: '',
      isWindows: true,
      testMode: false,
      updateSchedule: '0 0 0/6 1/1 * ? *',
    })),
    saveUpdateSettings: vi.fn().mockReturnValue(of(true)),
  };
  const mockUpdateService = {
    checkForUpdate: vi.fn().mockReturnValue(of({
      updateAvailable: true,
      updateVersionString: '4.58.3',
      updateVersion: 0,
      updateDate: new Date(),
      changeLogs: '',
      downloads: [],
    })),
  };
  const fb = new UntypedFormBuilder();
  const mockNotify = { error: vi.fn(), success: vi.fn() };

  const comp = new UpdateComponent(
    mockSettingsService as any,
    mockUpdateService as any,
    mockNotify as any,
    fb,
  );
  return { comp, mockSettingsService, mockUpdateService, mockNotify };
}

describe('UpdateComponent', () => {
  it('should load update settings and build form', () => {
    const { comp } = createComponent();
    comp.ngOnInit();
    expect(comp.form).toBeDefined();
    expect(comp.form.controls['autoUpdateEnabled'].value).toBe(false);
    expect(comp.form.controls['updateSchedule'].value).toBe('0 0 0/6 1/1 * ? *');
    expect(comp.form.controls['processName'].value).toBe('Ombi');
  });

  it('should load update status from update check', () => {
    const { comp } = createComponent();
    comp.ngOnInit();
    expect(comp.updateStatus).toBeDefined();
    expect(comp.updateStatus.updateAvailable).toBe(true);
    expect(comp.updateStatus.updateVersionString).toBe('4.58.3');
  });

  it('should check for update and notify when available', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.checkForUpdate();
    expect(comp.updateStatus).toBeDefined();
    expect(comp.updateStatus.updateAvailable).toBe(true);
    expect(mockNotify.success).toHaveBeenCalledWith('Update available: v4.58.3');
  });

  it('should notify when already on latest version', () => {
    const { comp, mockNotify, mockUpdateService } = createComponent();
    mockUpdateService.checkForUpdate.mockReturnValue(of({
      updateAvailable: false,
      updateVersionString: '4.53.4',
      updateVersion: 0,
      updateDate: new Date(),
      changeLogs: '',
      downloads: [],
    }));
    comp.ngOnInit();
    comp.checkForUpdate();
    expect(mockNotify.success).toHaveBeenCalledWith('You are running the latest version.');
  });

  it('should save settings and notify success', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.onSubmit(comp.form);
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved Update settings');
  });

  it('should notify error on save failure', () => {
    const { comp, mockNotify, mockSettingsService } = createComponent();
    mockSettingsService.saveUpdateSettings.mockReturnValue(of(false));
    comp.ngOnInit();
    comp.onSubmit(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('There was an error when saving the Update settings');
  });

  it('should not submit when form is invalid', () => {
    const { comp, mockNotify, mockSettingsService } = createComponent();
    comp.ngOnInit();
    // Force form invalid
    Object.defineProperty(comp.form, 'invalid', { get: () => true });
    comp.onSubmit(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values');
    expect(mockSettingsService.saveUpdateSettings).not.toHaveBeenCalled();
  });
});
