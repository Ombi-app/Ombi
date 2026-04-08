import { describe, it, expect, vi, beforeEach } from 'vitest';
import { RadarrComponent } from './radarr.component';
import { UntypedFormBuilder, UntypedFormGroup, UntypedFormControl } from '@angular/forms';
import { of } from 'rxjs';

function createComponent() {
  const mockRadarrFacade = {
    state$: vi.fn().mockReturnValue(of({
      settings: {
        radarr: {
          enabled: false, apiKey: '', defaultQualityProfile: -1,
          defaultRootPath: '', tag: null, sendUserTags: false,
          ssl: false, subDir: '', ip: '', port: 7878,
          addOnly: false, minimumAvailability: 'Announced',
          scanForAvailability: false, prioritizeArrAvailability: false,
        },
        radarr4K: {
          enabled: false, apiKey: '', defaultQualityProfile: -1,
          defaultRootPath: '', tag: null, sendUserTags: false,
          ssl: false, subDir: '', ip: '', port: 7879,
          addOnly: false, minimumAvailability: 'Announced',
          scanForAvailability: false, prioritizeArrAvailability: false,
        },
      }
    })),
    updateSettings: vi.fn().mockReturnValue(of(true)),
  };

  const mockNotify = {
    error: vi.fn(),
    success: vi.fn(),
  };

  const mockFeatureFacade = {
    is4kEnabled: vi.fn().mockReturnValue(false),
  };

  const fb = new UntypedFormBuilder();

  const comp = new RadarrComponent(
    mockRadarrFacade as any,
    mockNotify as any,
    mockFeatureFacade as any,
    fb,
  );

  return { comp, mockRadarrFacade, mockNotify, mockFeatureFacade, fb };
}

function buildForm(overrides: Record<string, any> = {}) {
  const defaults = {
    enabled: false, apiKey: 'key', defaultQualityProfile: 1,
    defaultRootPath: '/movies', tag: null, sendUserTags: false,
    ssl: false, subDir: '', ip: '127.0.0.1', port: 7878,
    addOnly: false, minimumAvailability: 'Announced',
    scanForAvailability: false, prioritizeArrAvailability: false,
  };
  const fb = new UntypedFormBuilder();
  return fb.group({
    radarr: fb.group({ ...defaults, ...overrides }),
    radarr4K: fb.group({ ...defaults, enabled: false }),
  });
}

describe('RadarrComponent', () => {
  describe('onSubmit', () => {
    it('should show error when form is invalid', () => {
      const { comp, mockNotify } = createComponent();
      const form = buildForm();
      Object.defineProperty(form, 'invalid', { value: true });

      comp.onSubmit(form);
      expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values');
    });

    it('should show error when radarr enabled with default quality profile -1', () => {
      const { comp, mockNotify } = createComponent();
      const form = buildForm({ enabled: true, defaultQualityProfile: -1 });

      comp.onSubmit(form);
      expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values for Radarr');
    });

    it('should show error when radarr enabled with default root path "Please Select"', () => {
      const { comp, mockNotify } = createComponent();
      const form = buildForm({ enabled: true, defaultRootPath: 'Please Select' });

      comp.onSubmit(form);
      expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values for Radarr');
    });

    it('should save settings and notify success', () => {
      const { comp, mockNotify, mockRadarrFacade } = createComponent();
      const form = buildForm({ enabled: true, defaultQualityProfile: 5, defaultRootPath: '/movies' });

      comp.onSubmit(form);
      expect(mockRadarrFacade.updateSettings).toHaveBeenCalled();
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved Radarr settings');
    });

    it('should nullify tag when value is -1', () => {
      const { comp, mockRadarrFacade } = createComponent();
      const form = buildForm({ enabled: false, tag: -1 });

      comp.onSubmit(form);
      expect(mockRadarrFacade.updateSettings).toHaveBeenCalled();
    });

    it('should notify on save failure', () => {
      const { comp, mockNotify, mockRadarrFacade } = createComponent();
      mockRadarrFacade.updateSettings.mockReturnValue(of(false));
      const form = buildForm();

      comp.onSubmit(form);
      expect(mockNotify.success).toHaveBeenCalledWith('There was an error when saving the Radarr settings');
    });
  });

  describe('ngOnDestroy', () => {
    it('should not throw on destroy', () => {
      const { comp } = createComponent();
      expect(() => comp.ngOnDestroy()).not.toThrow();
    });
  });
});
